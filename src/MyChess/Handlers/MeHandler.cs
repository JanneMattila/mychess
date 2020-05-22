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

        public async Task<Player> LoginAsync(AuthenticatedUser authenticatedUser)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            return new Player()
            {
                ID = userID
            };
        }
    }
}
