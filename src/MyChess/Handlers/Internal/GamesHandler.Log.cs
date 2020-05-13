// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Handlers.Internal
{
    internal static class GamesHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _gameHandlerGameFound;
        private static readonly Action<ILogger, string, Exception> _gameHandlerGameNotFound;
        private static readonly Action<ILogger, int, Exception> _gameHandlerGamesFound;

        static GamesHandlerLoggerExtensions()
        {
            _gameHandlerGameFound = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.GameHandlerGameFound, nameof(GameHandlerGameFound)),
                "Game found {GameID}");
            _gameHandlerGameNotFound = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.GameHandlerGameNotFound, nameof(GameHandlerGameNotFound)),
                "Game not found {GameID}");
            _gameHandlerGamesFound = LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(LoggingEvents.GameHandlerGamesFound, nameof(GameHandlerGamesFound)),
                "Found {Count} games");
        }

        public static void GameHandlerGameFound(this ILogger logger, string gameID) => _gameHandlerGameFound(logger, gameID, null);
        public static void GameHandlerGameNotFound(this ILogger logger, string gameID) => _gameHandlerGameNotFound(logger, gameID, null);
        public static void GameHandlerGamesFound(this ILogger logger, int count) => _gameHandlerGamesFound(logger, count, null);
    }
}
