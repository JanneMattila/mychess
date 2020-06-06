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

        public GamesHandler(ILogger<GamesHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
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
            throw new NotImplementedException();
        }
    }
}
