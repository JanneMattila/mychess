// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess
{
    public static class LoggerExtensions
    {
        private static readonly Func<ILogger, IDisposable> _funcGamesScope;
        private static readonly Action<ILogger, Exception> _funcGamesStarted;
        private static readonly Action<ILogger, string, string, Exception> _funcGamesUserDoesNotHavePermission;

        static LoggerExtensions()
        {
            _funcGamesScope = LoggerMessage.DefineScope("Games");
            _funcGamesStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncGamesStarted, nameof(FuncGamesStarted)),
                "Games function processing request.");
            _funcGamesUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FuncGamesUserDoesNotHavePermission, nameof(FuncGamesUserDoesNotHavePermission)),
                "User {User} does not have permission {Permission}");
        }

        public static IDisposable FuncGamesScope(this ILogger logger) => _funcGamesScope(logger);
        public static void FuncGamesStarted(this ILogger logger) => _funcGamesStarted(logger, null);
        public static void FuncGamesUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcGamesUserDoesNotHavePermission(logger, user, permission, null);
    }
}
