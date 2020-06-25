// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Handlers.Internal
{
    internal static class FriendHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _friendHandlerFriendFound;
        private static readonly Action<ILogger, string, Exception> _friendHandlerFriendNotFound;
        private static readonly Action<ILogger, int, Exception> _friendHandlerFriendsFound;

        static FriendHandlerLoggerExtensions()
        {
            _friendHandlerFriendFound = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.FriendHandlerFriendFound, nameof(FriendHandlerFriendFound)),
                "Friend found {FriendID}");
            _friendHandlerFriendNotFound = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FriendHandlerFriendNotFound, nameof(FriendHandlerFriendNotFound)),
                "Friend not found {FriendID}");
            _friendHandlerFriendsFound = LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(LoggingEvents.FriendHandlerFriendsFound, nameof(FriendHandlerFriendsFound)),
                "Found {Count} friends");
        }

        public static void FriendHandlerFriendFound(this ILogger logger, string friendID) => _friendHandlerFriendFound(logger, friendID, null);
        public static void FriendHandlerFriendNotFound(this ILogger logger, string friendID) => _friendHandlerFriendNotFound(logger, friendID, null);
        public static void FriendHandlerFriendsFound(this ILogger logger, int count) => _friendHandlerFriendsFound(logger, count, null);
    }
}
