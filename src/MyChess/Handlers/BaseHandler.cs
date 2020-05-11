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

        public async Task<string> GetOrCreateUserAsync(AuthenticatedUser authenticatedUser)
        {
            var userMap = await _context.GetAsync<UserID2UserEntity>(TableNames.UserID2User,
                authenticatedUser.UserIdentifier, authenticatedUser.ProviderIdentifier);

            return await Task.FromResult(string.Empty);
        }
    }
}
