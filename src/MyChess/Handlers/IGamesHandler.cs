using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public interface IGamesHandler
    {
        Task<MyChessGame?> GetGameAsync(AuthenticatedUser authenticatedUser, string gameID);
        Task<List<MyChessGame>> GetGamesAsync(AuthenticatedUser authenticatedUser);
    }
}