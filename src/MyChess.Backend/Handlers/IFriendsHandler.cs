using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Backend.Handlers;

public interface IFriendsHandler
{
    Task<(User? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, User player);
    Task<User?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID);
    Task<List<User>> GetFriendsAsync(AuthenticatedUser authenticatedUser);
}
