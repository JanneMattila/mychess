// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Backend.Data.Internal
{
    internal static class MyChessDataContextLoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _contextInitializing;
        private static readonly Action<ILogger, Exception> _contextInitialized;
        private static readonly Action<ILogger, string, bool, Exception> _contextInitializeTable;

        static MyChessDataContextLoggerExtensions()
        {
            _contextInitializing = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.DataContextInitializing, nameof(DataContextInitializing)),
                "Initializing data context");
            _contextInitialized = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(LoggingEvents.DataContextInitialized, nameof(DataContextInitialized)),
                "Initialized data context");
            _contextInitializeTable = LoggerMessage.Define<string, bool>(
                LogLevel.Information,
                new EventId(LoggingEvents.DataContextInitializeTable, nameof(DataContextInitializeTable)),
                "Table {TableName} created during initialization {Created}");
        }

        public static void DataContextInitializing(this ILogger logger) => _contextInitializing(logger, null);
        public static void DataContextInitialized(this ILogger logger) => _contextInitialized(logger, null);
        public static void DataContextInitializeTable(this ILogger logger, string tableName, bool created) => _contextInitializeTable(logger, tableName, created, null);
    }
}
