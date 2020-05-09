using Microsoft.Extensions.Logging;
using MyChess.Data;

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
    }
}
