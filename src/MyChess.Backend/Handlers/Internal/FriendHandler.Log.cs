// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Backend.Handlers.Internal
{
    internal static class FriendHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _friendHandlerFriendFound;
        private static readonly Action<ILogger, string, Exception> _friendHandlerFriendNotFound;
        private static readonly Action<ILogger, int, Exception> _friendHandlerFriendsFound;

        private static readonly Action<ILogger, string, string, Exception> _friendHandlerAddingNameToFriend;
        private static readonly Action<ILogger, string, string, Exception> _friendHandlerExistingFriend;

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

            _friendHandlerAddingNameToFriend = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(LoggingEvents.FriendHandlerAddingNameToFriend, nameof(FriendHandlerAddingNameToFriend)),
                "Adding friend to another player when {UserID} added {FriendID} as friend");
            _friendHandlerExistingFriend = LoggerMessage.Define<string, string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FriendHandlerExistingFriend, nameof(FriendHandlerExistingFriend)),
                "Skip adding friend to another player when {UserID} added {FriendID} as friend since it already existed");
        }

        public static void FriendHandlerFriendFound(this ILogger logger, string friendID) => _friendHandlerFriendFound(logger, friendID, null);
        public static void FriendHandlerFriendNotFound(this ILogger logger, string friendID) => _friendHandlerFriendNotFound(logger, friendID, null);
        public static void FriendHandlerFriendsFound(this ILogger logger, int count) => _friendHandlerFriendsFound(logger, count, null);

        public static void FriendHandlerAddingNameToFriend(this ILogger logger, string userID, string friendID) => _friendHandlerAddingNameToFriend(logger, userID, friendID, null);
        public static void FriendHandlerExistingFriend(this ILogger logger, string userID, string friendID) => _friendHandlerExistingFriend(logger, userID, friendID, null);
    }
}
