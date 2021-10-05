using Microsoft.Extensions.Logging;
using MyChess.Backend.Data;

namespace MyChess.Backend.Handlers
{
    public class UserHandler : BaseHandler
    {
        public UserHandler(ILogger<UserHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }
    }
}
