using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public interface IGamesHandler
    {
        Task<(MyChessGame? Game, HandlerError? Error)> CreateGameAsync(AuthenticatedUser authenticatedUser, MyChessGame game);
        Task<MyChessGame?> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID);
        Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser);
        Task<HandlerError?> AddMoveAsync(AuthenticatedUser authenticatedUser, string gameID, MyChessGameMove move);
    }
}
