using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions.Tests.Stubs
{
    public class GamesHandlerStub : IGamesHandler
    {
        public MyChessGame SingleGame { get; set; } = new MyChessGame();

        public List<MyChessGame> Games { get; set; } = new List<MyChessGame>();

        public async Task<MyChessGame> CreateGameAsync(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            return await Task.FromResult(SingleGame);
        }

        public async Task<MyChessGame> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID)
        {
            return await Task.FromResult(SingleGame);
        }

        public async Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(Games);
        }
    }
}
