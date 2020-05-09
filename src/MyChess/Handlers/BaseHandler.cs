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

        public async Task<string> GetUserAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(string.Empty);
        }
    }
}
