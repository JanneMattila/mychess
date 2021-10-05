using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers.Internal;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Backend.Handlers
{
    public class FriendsHandler : BaseHandler, IFriendsHandler
    {
        public FriendsHandler(ILogger<FriendsHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }

        public async Task<(User? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, User player)
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

            var user = await GetOrCreateUserAsync(authenticatedUser);

            await _context.UpsertAsync(TableNames.UserFriends, new UserFriendEntity
            {
                PartitionKey = user.UserID,
                RowKey = friendID,
                Name = player.Name
            });

            var existingFriendMapping = await _context.GetAsync<UserFriendEntity>(TableNames.UserFriends, friendID, user.UserID);
            if (existingFriendMapping == null)
            {
                _log.FriendHandlerAddingNameToFriend(user.UserID, friendID);

                await _context.UpsertAsync(TableNames.UserFriends, new UserFriendEntity
                {
                    PartitionKey = friendID,
                    RowKey = user.UserID,
                    Name = user.Name
                });
            }
            else
            {
                _log.FriendHandlerExistingFriend(user.UserID, friendID);
            }

            return (player, null);
        }

        public async Task<User?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var userFriendEntity = await _context.GetAsync<UserFriendEntity>(TableNames.UserFriends, user.UserID, friendID);
            if (userFriendEntity != null)
            {
                _log.FriendHandlerFriendFound(friendID);
                return new User()
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

        public async Task<List<User>> GetFriendsAsync(AuthenticatedUser authenticatedUser)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var friends = new List<User>();

            await foreach (var userFriendEntity in _context.GetAllAsync<UserFriendEntity>(TableNames.UserFriends, user.UserID))
            {
                friends.Add(new User()
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
