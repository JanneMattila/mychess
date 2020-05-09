using Microsoft.Extensions.Logging;
using MyChess.Data;

namespace MyChess.Handlers
{
    public class GamesHandler
    {
        private readonly ILogger _log;
        private readonly IMyChessDataContext _context;

        public GamesHandler(ILogger log, IMyChessDataContext context)
        {
            _log = log;
            _context = context;
        }
    }
}
