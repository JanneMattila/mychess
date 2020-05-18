using System.Collections.Generic;
using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Functions.Tests.Stubs
{
    public class FriendsHandlerStub : IFriendsHandler
    {
        public Player? SingleFriend { get; set; }

        public List<Player> Friends { get; set; } = new List<Player>();

        public HandlerError? Error { get; set; }

        public async Task<(Player? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            await Task.CompletedTask;
            return (SingleFriend, Error);
        }

        public async Task<Player?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID)
        {
            return await Task.FromResult(SingleFriend);
        }

        public async Task<List<Player>> GetFriendsAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(Friends);
        }
    }
}
