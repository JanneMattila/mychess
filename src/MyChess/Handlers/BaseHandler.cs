using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public class BaseHandler
    {
        protected readonly ILogger _log;
        protected readonly IMyChessDataContext _context;

        public BaseHandler(ILogger log, IMyChessDataContext context)
        {
            _log = log;
            _context = context;
        }

        protected async Task<UserEntity> GetOrCreateUserAsync(AuthenticatedUser authenticatedUser)
        {
            var user = await _context.GetAsync<UserEntity>(TableNames.Users,
                authenticatedUser.UserIdentifier, authenticatedUser.ProviderIdentifier);
            if (user == null)
            {
                _log.BaseHandlerCreateNewUser();

                var userID = Guid.NewGuid().ToString("D");
                var userEntity = new UserEntity
                {
                    PartitionKey = authenticatedUser.UserIdentifier,
                    RowKey = authenticatedUser.ProviderIdentifier,
                    Name = authenticatedUser.Name,
                    UserID = userID,
                    Created = DateTime.UtcNow,
                    Enabled = true
                };

                await _context.UpsertAsync(TableNames.Users, userEntity);

                var userID2UserEntity = new UserID2UserEntity
                {
                    PartitionKey = userID,
                    RowKey = userID,
                    UserPrimaryKey = userEntity.PartitionKey,
                    UserRowKey = userEntity.RowKey
                };

                await _context.UpsertAsync(TableNames.UserID2User, userID2UserEntity);
                _log.BaseHandlerNewUserCreated(userID);

                return userEntity;
            }
            else
            {
                _log.BaseHandlerExistingUserFound(user.UserID);
                return user;
            }
        }

        protected async Task<UserEntity?> GetUserByUserIDAsync(string userID)
        {
            var userLookup = await _context.GetAsync<UserID2UserEntity>(TableNames.UserID2User, 
                userID, userID);
            if (userLookup != null)
            {
                _log.BaseHandlerUserLookupFoundByUserID(userID);

                var user = await _context.GetAsync<UserEntity>(TableNames.Users, 
                    userLookup.UserPrimaryKey, userLookup.UserRowKey);
                if (user != null)
                {
                    _log.BaseHandlerUserFoundByUserID(userID);
                }
                else
                {
                    _log.BaseHandlerUserNotFoundByUserID(userID);
                }

                return user;
            }
            else
            {
                _log.BaseHandlerUserLookupNotFoundByUserID(userID);
                return null;
            }
        }
    }
}
