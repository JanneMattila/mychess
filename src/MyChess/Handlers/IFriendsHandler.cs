using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public interface IFriendsHandler
    {
        Task<(User? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, User player);
        Task<User?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID);
        Task<List<User>> GetFriendsAsync(AuthenticatedUser authenticatedUser);
    }
}
