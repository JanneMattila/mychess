using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public class FriendsHandler : BaseHandler, IFriendsHandler
    {
        public FriendsHandler(ILogger<FriendsHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }

        public async Task<(Player? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, Player player)
        {
            var friendID = player.ID;
            var friend = await GetUserByUserIDAsync(friendID);
            if (friend == null)
            {
                return (null, new HandlerError()
                {
                    Instance = LoggingEvents.CreateLinkToProblemDescription(LoggingEvents.FriendHandlerPlayerNotFound),
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "User not found",
                    Detail = "For some reason player could not be found"
                });
            }

            var userID = await GetOrCreateUserAsync(authenticatedUser);

            await _context.UpsertAsync(TableNames.UserFriends, new UserFriendEntity
            {
                PartitionKey = userID,
                RowKey = friendID,
                Name = player.Name
            });
            
            return (player, null);
        }

        public async Task<Player?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var userFriendEntity = await _context.GetAsync<UserFriendEntity>(TableNames.UserFriends, userID, friendID);
            if (userFriendEntity != null)
            {
                _log.FriendHandlerFriendFound(friendID);
                return new Player()
                {
                    ID = userFriendEntity.RowKey,
                    Name = userFriendEntity.Name
                };
            }
            else
            {
                _log.FriendHandlerFriendNotFound(friendID);
                return null;
            }
        }

        public async Task<List<Player>> GetFriendsAsync(AuthenticatedUser authenticatedUser)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var friends = new List<Player>();

            await foreach (var userFriendEntity in _context.GetAllAsync<UserFriendEntity>(TableNames.UserFriends, userID))
            {
                friends.Add(new Player()
                {
                    ID = userFriendEntity.RowKey,
                    Name = userFriendEntity.Name
                });
            }

            _log.FriendHandlerFriendsFound(friends.Count);
            return friends;
        }
    }
}
