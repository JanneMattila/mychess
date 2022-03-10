// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Backend.Handlers.Internal;

internal static class BaseHandlerLoggerExtensions
{
    private static readonly Action<ILogger, Exception> _baseHandlerCreateNewUser;
    private static readonly Action<ILogger, string, Exception> _baseHandlerNewUserCreated;
    private static readonly Action<ILogger, string, Exception> _baseHandlerExistingUserFound;

    private static readonly Action<ILogger, string, Exception> _baseHandlerUserLookupFoundByUserID;
    private static readonly Action<ILogger, string, Exception> _baseHandlerUserFoundByUserID;
    private static readonly Action<ILogger, string, Exception> _baseHandlerUserNotFoundByUserID;
    private static readonly Action<ILogger, string, Exception> _baseHandlerUserLookupNotFoundByUserID;

    static BaseHandlerLoggerExtensions()
    {
        _baseHandlerCreateNewUser = LoggerMessage.Define(
            LogLevel.Trace,
            new EventId(LoggingEvents.BaseHandlerCreateNewUser, nameof(BaseHandlerCreateNewUser)),
            "Create new user");
        _baseHandlerNewUserCreated = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(LoggingEvents.BaseHandlerNewUserCreated, nameof(BaseHandlerNewUserCreated)),
            "New user created {UserID}");
        _baseHandlerExistingUserFound = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(LoggingEvents.BaseHandlerExistingUserFound, nameof(BaseHandlerExistingUserFound)),
            "Existing user found {UserID}");

        _baseHandlerUserLookupFoundByUserID = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(LoggingEvents.BaseHandlerUserLookupFoundByUserID, nameof(BaseHandlerUserLookupFoundByUserID)),
            "User lookup found for user {UserID}");
        _baseHandlerUserFoundByUserID = LoggerMessage.Define<string>(
             LogLevel.Information,
             new EventId(LoggingEvents.BaseHandlerUserFoundByUserID, nameof(BaseHandlerUserFoundByUserID)),
             "Found user {UserID}");
        _baseHandlerUserNotFoundByUserID = LoggerMessage.Define<string>(
             LogLevel.Information,
             new EventId(LoggingEvents.BaseHandlerUserNotFoundByUserID, nameof(BaseHandlerUserNotFoundByUserID)),
             "User not found {UserID}");
        _baseHandlerUserLookupNotFoundByUserID = LoggerMessage.Define<string>(
             LogLevel.Information,
             new EventId(LoggingEvents.BaseHandlerUserLookupNotFoundByUserID, nameof(BaseHandlerUserLookupNotFoundByUserID)),
             "User lookup not found for user {UserID}");
    }

    public static void BaseHandlerCreateNewUser(this ILogger logger) => _baseHandlerCreateNewUser(logger, null);
    public static void BaseHandlerNewUserCreated(this ILogger logger, string userID) => _baseHandlerNewUserCreated(logger, userID, null);
    public static void BaseHandlerExistingUserFound(this ILogger logger, string userID) => _baseHandlerExistingUserFound(logger, userID, null);

    public static void BaseHandlerUserLookupFoundByUserID(this ILogger logger, string userID) => _baseHandlerUserLookupFoundByUserID(logger, userID, null);
    public static void BaseHandlerUserFoundByUserID(this ILogger logger, string userID) => _baseHandlerUserFoundByUserID(logger, userID, null);
    public static void BaseHandlerUserNotFoundByUserID(this ILogger logger, string userID) => _baseHandlerUserNotFoundByUserID(logger, userID, null);
    public static void BaseHandlerUserLookupNotFoundByUserID(this ILogger logger, string userID) => _baseHandlerUserLookupNotFoundByUserID(logger, userID, null);
}
