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

        private static readonly Action<ILogger, string, Exception> _gameHandlerMoveGameNotFound;
        private static readonly Action<ILogger, string, string, string, string, Exception> _gameHandlerMoveInvalidPlayer;
        private static readonly Action<ILogger, string, string, string, string, Exception> _gameHandlerMoveNotPlayerTurn;

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

            _gameHandlerMoveGameNotFound = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.GameHandlerMoveGameNotFound, nameof(GameHandlerMoveGameNotFound)),
                "Game not found {GameID} while making move");
            _gameHandlerMoveInvalidPlayer = LoggerMessage.Define<string, string, string, string>(
                LogLevel.Error,
                new EventId(LoggingEvents.GameHandlerMoveInvalidPlayer, nameof(GameHandlerMoveInvalidPlayer)),
                "User {UserID} is not player of game {GameID}. Reference: White: {WhiteID} and Black: {BlackID}");
            _gameHandlerMoveNotPlayerTurn = LoggerMessage.Define<string, string, string, string>(
                LogLevel.Error,
                new EventId(LoggingEvents.GameHandlerMoveNotPlayerTurn, nameof(GameHandlerMoveNotPlayerTurn)),
                "User {UserID} is not currently in turn to make move in game {GameID}. Reference: Current players side: {PlayersSide} and current side: {CurrentSide}");
        }

        public static void GameHandlerGameFound(this ILogger logger, string gameID) => _gameHandlerGameFound(logger, gameID, null);
        public static void GameHandlerGameNotFound(this ILogger logger, string gameID) => _gameHandlerGameNotFound(logger, gameID, null);
        public static void GameHandlerGamesFound(this ILogger logger, int count) => _gameHandlerGamesFound(logger, count, null);

        public static void GameHandlerMoveGameNotFound(this ILogger logger, string gameID) => _gameHandlerMoveGameNotFound(logger, gameID, null);
        public static void GameHandlerMoveInvalidPlayer(this ILogger logger, string gameID, string userID, string whiteID, string blackID) => _gameHandlerMoveInvalidPlayer(logger, gameID, userID, whiteID, blackID, null);
        public static void GameHandlerMoveNotPlayerTurn(this ILogger logger, string gameID, string userID, string playersSide, string currentSide) => _gameHandlerMoveInvalidPlayer(logger, gameID, userID, playersSide, currentSide, null);
    }
}
