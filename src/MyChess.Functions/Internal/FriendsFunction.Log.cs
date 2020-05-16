// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Internal
{
    internal static class FriendsFunctionLoggerExtensions
    {
        private static readonly Func<ILogger, IDisposable> _funcFriendsScope;
        private static readonly Action<ILogger, Exception> _funcFriendsStarted;
        private static readonly Action<ILogger, string, string, Exception> _funcFriendsUserDoesNotHavePermission;
        private static readonly Action<ILogger, string, Exception> _funcFriendsProcessingMethod;
        private static readonly Action<ILogger, Exception> _funcFriendsFetchAllFriends;
        private static readonly Action<ILogger, string, Exception> _funcFriendsFetchSingleFriend;
        private static readonly Action<ILogger, Exception> _funcFriendsAddNewFriend;

        static FriendsFunctionLoggerExtensions()
        {
            _funcFriendsScope = LoggerMessage.DefineScope("Friends");
            _funcFriendsStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncFriendsStarted, nameof(FuncFriendsStarted)),
                "Friends function processing request");
            _funcFriendsUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FuncFriendsUserDoesNotHavePermission, nameof(FuncFriendsUserDoesNotHavePermission)),
                "User {User} does not have permission {Permission}");
            _funcFriendsProcessingMethod = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncFriendsUserDoesNotHavePermission, nameof(FuncFriendsProcessingMethod)),
                "Processing {Method} request");
            _funcFriendsFetchAllFriends = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncFriendsFetchAllFriends, nameof(FuncFriendsFetchAllFriends)),
                "Fetch all friends");
            _funcFriendsFetchSingleFriend = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncFriendsFetchSingleFriend, nameof(FuncFriendsFetchSingleFriend)),
                "Fetch single friend {FriendID}");
            _funcFriendsAddNewFriend = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncFriendsAddNewFriend, nameof(FuncFriendsAddNewFriend)),
                "Add new friend");
        }

        public static IDisposable FuncFriendsScope(this ILogger logger) => _funcFriendsScope(logger);
        public static void FuncFriendsStarted(this ILogger logger) => _funcFriendsStarted(logger, null);
        public static void FuncFriendsUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcFriendsUserDoesNotHavePermission(logger, user, permission, null);
        public static void FuncFriendsProcessingMethod(this ILogger logger, string method) => _funcFriendsProcessingMethod(logger, method, null);
        public static void FuncFriendsFetchAllFriends(this ILogger logger) => _funcFriendsFetchAllFriends(logger, null);
        public static void FuncFriendsFetchSingleFriend(this ILogger logger, string gameID) => _funcFriendsFetchSingleFriend(logger, gameID, null);
        public static void FuncFriendsAddNewFriend(this ILogger logger) => _funcFriendsAddNewFriend(logger, null);
    }
}
