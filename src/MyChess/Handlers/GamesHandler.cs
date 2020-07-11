﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public class GamesHandler : BaseHandler, IGamesHandler
    {
        private readonly Compactor _compactor = new Compactor();
        private readonly INotificationHandler _notificationHandler;
        private readonly ChessBoard _chessBoard;

        public GamesHandler(ILogger<GamesHandler> log, IMyChessDataContext context, INotificationHandler notificationHandler, ChessBoard chessBoard)
            : base(log, context)
        {
            _notificationHandler = notificationHandler;
            _chessBoard = chessBoard;
            _chessBoard.Initialize();
        }

        public async Task<(MyChessGame? Game, HandlerError? Error)> CreateGameAsync(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            var opponentID = game.Players.Black.ID;
            var opponent = await GetUserByUserIDAsync(opponentID);
            if (opponent == null)
            {
                return (null, new HandlerError()
                {
                    Instance = LoggingEvents.CreateLinkToProblemDescription(LoggingEvents.GameHandlerOpponentNotFound),
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "User not found",
                    Detail = "For some reason your opponent could not be found"
                });
            }

            var user = await GetOrCreateUserAsync(authenticatedUser);

            game.ID = Guid.NewGuid().ToString("D");
            game.Players.White.ID = user.UserID;
            var data = _compactor.Compact(game);

            // TODO: Validate game data
            var comment = game.Moves[0].Comment;

            await _context.UpsertAsync(TableNames.GamesWaitingForYou, new GameEntity
            {
                PartitionKey = opponentID,
                RowKey = game.ID,
                Data = data
            });
            await _context.UpsertAsync(TableNames.GamesWaitingForOpponent, new GameEntity
            {
                PartitionKey = user.UserID,
                RowKey = game.ID,
                Data = data
            });

            await _notificationHandler.SendNotificationAsync(opponentID, game.ID, comment);

            return (game, null);
        }

        public async Task<MyChessGame?> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID, string state)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var tableNames = GetTableNames(state);
            foreach (var tableName in tableNames)
            {
                var gameEntity = await _context.GetAsync<GameEntity>(tableName, user.UserID, gameID);
                if (gameEntity != null)
                {
                    _log.GameHandlerGameFound(gameID);
                    return _compactor.Decompress(gameEntity.Data);
                }
                _log.GameHandlerGameNotFoundFromTable(gameID, tableName);
            }

            _log.GameHandlerGameNotFound(gameID);
            return null;
        }

        private List<string> GetTableNames(string state)
        {
            var tables = new List<string>();
            if (string.IsNullOrEmpty(state) || state == GameFilterType.WaitingForYou)
            {
                tables.Add(TableNames.GamesWaitingForYou);
                tables.Add(TableNames.GamesWaitingForOpponent);
                tables.Add(TableNames.GamesArchive);
            }
            else if (state == GameFilterType.WaitingForOpponent)
            {
                tables.Add(TableNames.GamesWaitingForOpponent);
                tables.Add(TableNames.GamesWaitingForYou);
                tables.Add(TableNames.GamesArchive);
            }
            else
            {
                tables.Add(TableNames.GamesArchive);
                tables.Add(TableNames.GamesWaitingForOpponent);
                tables.Add(TableNames.GamesWaitingForYou);
            }
            return tables;
        }

        public async Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser, string state)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var games = new List<MyChessGame>();
            var table = state switch
            {
                GameFilterType.WaitingForYou => TableNames.GamesWaitingForYou,
                GameFilterType.WaitingForOpponent => TableNames.GamesWaitingForOpponent,
                GameFilterType.Archive => TableNames.GamesArchive,
                _ => TableNames.GamesWaitingForYou
            };

            await foreach (var gameEntity in _context.GetAllAsync<GameEntity>(table, user.UserID))
            {
                var game = _compactor.Decompress(gameEntity.Data);
                games.Add(game);
            }

            _log.GameHandlerGamesFound(games.Count);
            return games;
        }

        public async Task<HandlerError?> AddMoveAsync(AuthenticatedUser authenticatedUser, string gameID, MyChessGameMove move)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var gameEntity = await _context.GetAsync<GameEntity>(TableNames.GamesWaitingForYou, user.UserID, gameID);
            if (gameEntity == null)
            {
                _log.GameHandlerMoveGameNotFound(gameID);
                return new HandlerError()
                {
                    Instance = LoggingEvents.CreateLinkToProblemDescription(LoggingEvents.GameHandlerMoveGameNotFound),
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Game not found",
                    Detail = "For some reason your game could not be found"
                };
            }

            _log.GameHandlerGameFound(gameID);
            var game = _compactor.Decompress(gameEntity.Data);

            if (game.Players.White.ID != user.UserID &&
                game.Players.Black.ID != user.UserID)
            {
                _log.GameHandlerMoveInvalidPlayer(gameID, user.UserID, game.Players.White.ID, game.Players.Black.ID);
                return new HandlerError()
                {
                    Instance = LoggingEvents.CreateLinkToProblemDescription(LoggingEvents.GameHandlerMoveInvalidPlayer),
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Player profile not found",
                    Detail = "For some reason your player profile was not found"
                };
            }

            _chessBoard.Initialize();
            game.Moves.Add(move); // Add new move and validate the entire move chain
            foreach (var gameMove in game.Moves)
            {
                _chessBoard.MakeMove(gameMove.Move);
                if (gameMove.SpecialMove != null)
                {
                    switch (gameMove.SpecialMove)
                    {
                        case MyChessGameSpecialMove.PromotionToRook:
                            _chessBoard.ChangePromotion(PieceRank.Rook);
                            break;
                        case MyChessGameSpecialMove.PromotionToKnight:
                            _chessBoard.ChangePromotion(PieceRank.Knight);
                            break;
                        case MyChessGameSpecialMove.PromotionToBishop:
                            _chessBoard.ChangePromotion(PieceRank.Bishop);
                            break;
                    }
                }
            }

            string opponentID;
            if (_chessBoard.CurrentPlayer == PiecePlayer.White &&
                game.Players.Black.ID == user.UserID)
            {
                opponentID = game.Players.White.ID;
            }
            else if (_chessBoard.CurrentPlayer == PiecePlayer.Black && 
                     game.Players.White.ID == user.UserID)
            {
                opponentID = game.Players.Black.ID;
            }
            else
            {
                _log.GameHandlerMoveNotPlayerTurn(gameID, user.UserID, _chessBoard.CurrentPlayer.ToString());
                return new HandlerError()
                {
                    Instance = LoggingEvents.CreateLinkToProblemDescription(LoggingEvents.GameHandlerMoveNotPlayerTurn),
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Not your turn to make move",
                    Detail = "It does not seem to be your turn to make the move"
                };
            }

            // TODO: If game is over then move to archive.

            var data = _compactor.Compact(game);

            // Add new games to storage
            await _context.UpsertAsync(TableNames.GamesWaitingForYou, new GameEntity
            {
                PartitionKey = opponentID,
                RowKey = game.ID,
                Data = data
            });
            await _context.UpsertAsync(TableNames.GamesWaitingForOpponent, new GameEntity
            {
                PartitionKey = user.UserID,
                RowKey = game.ID,
                Data = data
            });

            // Delete old ones from storage
            await _context.DeleteAsync(TableNames.GamesWaitingForOpponent, new GameEntity
            {
                PartitionKey = opponentID,
                RowKey = game.ID,
                Data = data
            });
            await _context.DeleteAsync(TableNames.GamesWaitingForYou, new GameEntity
            {
                PartitionKey = user.UserID,
                RowKey = game.ID,
                Data = data
            });

            await _notificationHandler.SendNotificationAsync(opponentID, game.ID, move.Comment);

            return null;
        }
    }
}
