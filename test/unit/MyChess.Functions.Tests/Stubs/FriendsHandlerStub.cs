using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Functions.Tests.Stubs
{
    public class FriendsHandlerStub : IFriendsHandler
    {
        public User? SingleFriend { get; set; }

        public List<User> Friends { get; set; } = new List<User>();

        public HandlerError? Error { get; set; }

        public async Task<(User? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, User player)
        {
            await Task.CompletedTask;
            return (SingleFriend, Error);
        }

        public async Task<User?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID)
        {
            return await Task.FromResult(SingleFriend);
        }

        public async Task<List<User>> GetFriendsAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(Friends);
        }
    }
}
