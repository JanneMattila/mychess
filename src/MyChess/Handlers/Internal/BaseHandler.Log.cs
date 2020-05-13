// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Handlers.Internal
{
    internal static class BaseHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _baseHandlerCreateNewUser;
        private static readonly Action<ILogger, string, Exception> _baseHandlerNewUserCreated;
        private static readonly Action<ILogger, string, Exception> _baseHandlerExistingUserFound;

        static BaseHandlerLoggerExtensions()
        {
            _baseHandlerCreateNewUser = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.BaseHandlerCreateNewUser, nameof(BaseHandlerCreateNewUser)),
                "Create new user");
            _baseHandlerNewUserCreated = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.BaseHandlerNewUserCreated, nameof(BaseHandlerNewUserCreated)),
                "New user created {User}");
            _baseHandlerExistingUserFound = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.BaseHandlerExistingUserFound, nameof(BaseHandlerExistingUserFound)),
                "Existing user found {User}");
        }

        public static void BaseHandlerCreateNewUser(this ILogger logger) => _baseHandlerCreateNewUser(logger, null);
        public static void BaseHandlerNewUserCreated(this ILogger logger, string userID) => _baseHandlerNewUserCreated(logger, userID, null);
        public static void BaseHandlerExistingUserFound(this ILogger logger, string userID) => _baseHandlerExistingUserFound(logger, userID, null);
    }
}
