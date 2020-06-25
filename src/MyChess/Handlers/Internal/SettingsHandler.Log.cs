// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Handlers.Internal
{
    internal static class SettingsHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _settingsHandlerSettingsFound;
        private static readonly Action<ILogger, string, Exception> _settingsHandlerSettingsNotFound;
        private static readonly Action<ILogger, int, Exception> _settingsHandlerNotificationsFound;

        static SettingsHandlerLoggerExtensions()
        {
            _settingsHandlerSettingsFound = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.SettingsHandlerSettingsFound, nameof(SettingsHandlerSettingsFound)),
                "Settings found {UserID}");
            _settingsHandlerSettingsNotFound = LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(LoggingEvents.SettingsHandlerSettingsNotFound, nameof(SettingsHandlerSettingsNotFound)),
                "Settings not found {UserID}");
            _settingsHandlerNotificationsFound = LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(LoggingEvents.SettingsHandlerNotificationsFound, nameof(SettingsHandlerNotificationsFound)),
                "Found {Count} notifications");
        }

        public static void SettingsHandlerSettingsFound(this ILogger logger, string userID) => _settingsHandlerSettingsFound(logger, userID, null);
        public static void SettingsHandlerSettingsNotFound(this ILogger logger, string userID) => _settingsHandlerSettingsNotFound(logger, userID, null);
        public static void SettingsHandlerNotificationsFound(this ILogger logger, int count) => _settingsHandlerNotificationsFound(logger, count, null);
    }
}
