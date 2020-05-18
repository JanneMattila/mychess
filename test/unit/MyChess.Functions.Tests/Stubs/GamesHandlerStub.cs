using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Functions.Tests.Stubs
{
    public class GamesHandlerStub : IGamesHandler
    {
        public MyChessGame? SingleGame { get; set; }
        
        public List<MyChessGame> Games { get; set; } = new List<MyChessGame>();

        public HandlerError? Error { get; set; }

        public async Task<(MyChessGame? Game, HandlerError? Error)> CreateGameAsync(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            await Task.CompletedTask;
            return (SingleGame, Error);
        }

        public async Task<MyChessGame?> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID)
        {
            return await Task.FromResult(SingleGame);
        }

        public async Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(Games);
        }
    }
}
