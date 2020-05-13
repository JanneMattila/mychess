using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public class GamesHandler : BaseHandler, IGamesHandler
    {
        private readonly Compactor _compactor = new Compactor();

        public GamesHandler(ILogger<GamesHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }

        public async Task<MyChessGame> CreateGameAsync(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);

            return await Task.FromResult(new MyChessGame()
            {
                ID = Guid.NewGuid().ToString("D")
            });
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
    }
}
