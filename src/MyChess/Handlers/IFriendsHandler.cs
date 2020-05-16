using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public interface IFriendsHandler
    {
        Task<(Player? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, MyChessGame game);
        Task<Player?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID);
        Task<List<Player>> GetFriendsAsync(AuthenticatedUser authenticatedUser);
    }
}
