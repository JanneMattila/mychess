using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public class FriendsHandler : BaseHandler, IFriendsHandler
    {
        public FriendsHandler(ILogger<FriendsHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }

        public Task<(Player? Friend, HandlerError? Error)> AddNewFriend(AuthenticatedUser authenticatedUser, MyChessGame game)
        {
            throw new NotImplementedException();
        }

        public Task<Player?> GetFriendAsync(AuthenticatedUser authenticatedUser, string friendID)
        {
            throw new NotImplementedException();
        }

        public Task<List<Player>> GetFriendsAsync(AuthenticatedUser authenticatedUser)
        {
            throw new NotImplementedException();
        }
    }
}
