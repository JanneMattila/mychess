using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Data;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Backend.Handlers
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
