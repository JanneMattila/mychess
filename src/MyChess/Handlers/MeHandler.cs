using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public class MeHandler : BaseHandler, IMeHandler
    {
        public MeHandler(ILogger<MeHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }

        public async Task<User> LoginAsync(AuthenticatedUser authenticatedUser)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);
            return new User()
            {
                ID = user.UserID
            };
        }
    }
}
