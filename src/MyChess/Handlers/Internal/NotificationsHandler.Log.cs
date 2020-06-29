// LoggerMessage.Define requires Exception which is null majority of times
#nullable disable
using System;
using Microsoft.Extensions.Logging;

namespace MyChess.Handlers.Internal
{
    internal static class NotificationsHandler
    {
        private static readonly Action<ILogger, string, Exception> _notificationsHandlerSendingNotifications;
        private static readonly Action<ILogger, int, int, Exception> _notificationsHandlerSendStatistics;
        private static readonly Action<ILogger, string, Exception> _notificationsHandlerSendFailed;


        static NotificationsHandler()
        {
            _notificationsHandlerSendingNotifications = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(LoggingEvents.NotificationsHandlerSendingNotifications, nameof(NotificationsHandlerSendingNotifications)),
                "Sending notifications for user {UserID}");
            _notificationsHandlerSendStatistics = LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(LoggingEvents.NotificationsHandlerSendStatistics, nameof(NotificationsHandlerSendStatistics)),
                "Send successfully {Success} notifications and {Failed} failed.");
            _notificationsHandlerSendFailed = LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(LoggingEvents.NotificationsHandlerSendFailed, nameof(NotificationsHandlerSendFailed)),
                "Send failed for user {UserID}");
        }

        public static void NotificationsHandlerSendingNotifications(this ILogger logger, string userID) => _notificationsHandlerSendingNotifications(logger, userID, null);
        public static void NotificationsHandlerSendStatistics(this ILogger logger, int success, int failed) => _notificationsHandlerSendStatistics(logger, success, failed, null);
        public static void NotificationsHandlerSendFailed(this ILogger logger, string userID, Exception ex) => _notificationsHandlerSendFailed(logger, userID, ex);
    }
}
