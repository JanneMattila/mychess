using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
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

        protected async Task<string> GetOrCreateUserAsync(AuthenticatedUser authenticatedUser)
        {
            var user = await _context.GetAsync<UserEntity>(TableNames.Users,
                authenticatedUser.UserIdentifier, authenticatedUser.ProviderIdentifier);
            if (user == null)
            {
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
                return userID;
            }
            else
            {
                return user.UserID;
            }
        }
    }
}
