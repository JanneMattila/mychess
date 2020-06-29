using System;
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

            var userID = await GetOrCreateUserAsync(authenticatedUser);

            game.ID = Guid.NewGuid().ToString("D");
            game.Players.White.ID = userID;
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
                PartitionKey = userID,
                RowKey = game.ID,
                Data = data
            });

            await _notificationHandler.SendNotificationAsync(opponentID, game.ID, comment);

            return (game, null);
        }

        public async Task<MyChessGame?> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var gameEntity = await _context.GetAsync<GameEntity>(TableNames.GamesWaitingForYou, userID, gameID);
            if (gameEntity != null)
            {
                _log.GameHandlerGameFound(gameID);
                return _compactor.Decompress(gameEntity.Data);
            }
            else
            {
                _log.GameHandlerGameNotFound(gameID);
                return null;
            }
        }

        public async Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var games = new List<MyChessGame>();

            await foreach (var gameEntity in _context.GetAllAsync<GameEntity>(TableNames.GamesWaitingForYou, userID))
            {
                var game = _compactor.Decompress(gameEntity.Data);
                games.Add(game);
            }

            _log.GameHandlerGamesFound(games.Count);
            return games;
        }

        public async Task<HandlerError?> AddMoveAsync(AuthenticatedUser authenticatedUser, string gameID, MyChessGameMove move)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var gameEntity = await _context.GetAsync<GameEntity>(TableNames.GamesWaitingForYou, userID, gameID);
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

            if (game.Players.White.ID != userID &&
                game.Players.Black.ID != userID)
            {
                _log.GameHandlerMoveInvalidPlayer(gameID, userID, game.Players.White.ID, game.Players.Black.ID);
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
                game.Players.Black.ID == userID)
            {
                opponentID = game.Players.White.ID;
            }
            else if (_chessBoard.CurrentPlayer == PiecePlayer.Black && 
                     game.Players.White.ID == userID)
            {
                opponentID = game.Players.Black.ID;
            }
            else
            {
                _log.GameHandlerMoveNotPlayerTurn(gameID, userID, _chessBoard.CurrentPlayer.ToString());
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
                PartitionKey = userID,
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
            await _context.UpsertAsync(TableNames.GamesWaitingForYou, new GameEntity
            {
                PartitionKey = userID,
                RowKey = game.ID,
                Data = data
            });

            await _notificationHandler.SendNotificationAsync(opponentID, game.ID, move.Comment);

            return null;
        }
    }
}
