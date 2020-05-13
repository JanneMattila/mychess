// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Internal
{
    internal static class GamesFunctionLoggerExtensions
    {
        private static readonly Func<ILogger, IDisposable> _funcGamesScope;
        private static readonly Action<ILogger, Exception> _funcGamesStarted;
        private static readonly Action<ILogger, string, string, Exception> _funcGamesUserDoesNotHavePermission;
        private static readonly Action<ILogger, string, Exception> _funcGamesProcessingMethod;
        private static readonly Action<ILogger, Exception> _funcGamesFetchAllGames;
        private static readonly Action<ILogger, string, Exception> _funcGamesFetchSingleGame;

        static GamesFunctionLoggerExtensions()
        {
            // GamesFunction
            _funcGamesScope = LoggerMessage.DefineScope("Games");
            _funcGamesStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.FuncGamesStarted, nameof(FuncGamesStarted)),
                "Games function processing request.");
            _funcGamesUserDoesNotHavePermission = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.FuncGamesUserDoesNotHavePermission, nameof(FuncGamesUserDoesNotHavePermission)),
                "User {User} does not have permission {Permission}");
            _funcGamesProcessingMethod = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncGamesUserDoesNotHavePermission, nameof(FuncGamesProcessingMethod)),
                "Processing {Method} request");
            _funcGamesFetchAllGames = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncGamesFetchAllGames, nameof(FuncGamesFetchAllGames)),
                "Fetch all games");
            _funcGamesFetchSingleGame = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.FuncGamesFetchSingleGame, nameof(FuncGamesFetchSingleGame)),
                "Fetch single game {GameID}");
        }

        // GamesFunction
        public static IDisposable FuncGamesScope(this ILogger logger) => _funcGamesScope(logger);
        public static void FuncGamesStarted(this ILogger logger) => _funcGamesStarted(logger, null);
        public static void FuncGamesUserDoesNotHavePermission(this ILogger logger, string user, string permission) => _funcGamesUserDoesNotHavePermission(logger, user, permission, null);
        public static void FuncGamesProcessingMethod(this ILogger logger, string method) => _funcGamesProcessingMethod(logger, method, null);
        public static void FuncGamesFetchAllGames(this ILogger logger) => _funcGamesFetchAllGames(logger, null);
        public static void FuncGamesFetchSingleGame(this ILogger logger, string gameID) => _funcGamesFetchSingleGame(logger, gameID, null);
    }
}
