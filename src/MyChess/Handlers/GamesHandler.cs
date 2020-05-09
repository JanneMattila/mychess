using Microsoft.Extensions.Logging;
using MyChess.Data;

namespace MyChess.Handlers
{
    public class GamesHandler : BaseHandler
    {
        public GamesHandler(ILogger log, IMyChessDataContext context)
            : base(log, context)
        {
        }
    }
}
