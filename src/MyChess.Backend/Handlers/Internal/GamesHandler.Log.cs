// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;
using MyChess.Backend;

namespace MyChess.Backend.Handlers.Internal
{
    internal static class GamesHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _gameHandlerGameFound;
        private static readonly Action<ILogger, string, Exception> _gameHandlerGameNotFound;
        private static readonly Action<ILogger, string, string, Exception> _gameHandlerGameNotFoundFromTable;
        private static readonly Action<ILogger, int, Exception> _gameHandlerGamesFound;

        private static readonly Action<ILogger, string, Exception> _gameHandlerMoveGameNotFound;
        private static readonly Action<ILogger, string, string, string, string, Exception> _gameHandlerMoveInvalidPlayer;
        private static readonly Action<ILogger, string, string, string, Exception> _gameHandlerMoveNotPlayerTurn;

        private static readonly Action<ILogger, string, Exception> _gameHandlerDeleteGameNotFound;
        private static readonly Action<ILogger, string, Exception> _gameHandlerDeleteGameAlreadyArchived;

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
            _gameHandlerGameNotFoundFromTable = LoggerMessage.Define<string, string>(
                LogLevel.Trace,
                new EventId(LoggingEvents.GameHandlerGameNotFoundFromTable, nameof(GameHandlerGameNotFoundFromTable)),
                "Game not found {GameID} from {Table}");
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
            _gameHandlerMoveNotPlayerTurn = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(LoggingEvents.GameHandlerMoveNotPlayerTurn, nameof(GameHandlerMoveNotPlayerTurn)),
                "User {UserID} is not currently in turn to make move in game {GameID}. Current turn: {CurrentTurn}");

            _gameHandlerDeleteGameNotFound = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.GameHandlerDeleteGameNotFound, nameof(GameHandlerDeleteGameNotFound)),
                "Could not find game {GameID} to be deleted.");
            _gameHandlerDeleteGameAlreadyArchived = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.GameHandlerDeleteGameAlreadyArchived, nameof(GameHandlerDeleteGameAlreadyArchived)),
                "Game {GameID} cannot be archived, since it's already archived.");
        }

        public static void GameHandlerGameFound(this ILogger logger, string gameID) => _gameHandlerGameFound(logger, gameID, null);
        public static void GameHandlerGameNotFoundFromTable(this ILogger logger, string gameID, string table) => _gameHandlerGameNotFoundFromTable(logger, gameID, table, null);
        public static void GameHandlerGameNotFound(this ILogger logger, string gameID) => _gameHandlerGameNotFound(logger, gameID, null);
        public static void GameHandlerGamesFound(this ILogger logger, int count) => _gameHandlerGamesFound(logger, count, null);

        public static void GameHandlerMoveGameNotFound(this ILogger logger, string gameID) => _gameHandlerMoveGameNotFound(logger, gameID, null);
        public static void GameHandlerMoveInvalidPlayer(this ILogger logger, string gameID, string userID, string whiteID, string blackID) => _gameHandlerMoveInvalidPlayer(logger, gameID, userID, whiteID, blackID, null);
        public static void GameHandlerMoveNotPlayerTurn(this ILogger logger, string gameID, string userID, string currentTurn) => _gameHandlerMoveNotPlayerTurn(logger, gameID, userID, currentTurn, null);

        public static void GameHandlerDeleteGameNotFound(this ILogger logger, string gameID) => _gameHandlerDeleteGameNotFound(logger, gameID, null);
        public static void GameHandlerDeleteGameAlreadyArchived(this ILogger logger, string gameID) => _gameHandlerDeleteGameAlreadyArchived(logger, gameID, null);
    }
}
