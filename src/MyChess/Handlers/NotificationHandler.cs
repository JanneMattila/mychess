using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;
using WebPush;

namespace MyChess.Handlers
{
    public class NotificationHandler : BaseHandler, INotificationHandler
    {
        private readonly NotificationOptions _options;

        public NotificationHandler(ILogger<NotificationHandler> log, IMyChessDataContext context, IOptions<NotificationOptions> options)
            : base(log, context)
        {
            _options = options.Value;
        }

        public async Task SendNotificationAsync(string userID, string gameID, string comment)
        {
            _log.NotificationsHandlerSendingNotifications(userID);
            var maxLength = 25;
            if (comment.Length > maxLength)
            {
                var index = comment.IndexOf(' ', maxLength);
                if (index < 0 || index > maxLength + 5)
                {
                    index = maxLength;
                }
                comment = comment.Substring(0, index);
                comment += "...";
            }

            var uri = $"/play/{gameID}";
            var vapidDetails = new VapidDetails($"{_options.PublicServerUri}{uri}", _options.PublicKey, _options.PrivateKey);
            var webPushClient = new WebPushClient();
            var json = JsonSerializer.Serialize(new NotificationMessage()
            {
                Text = comment,
                Uri = uri
            });

            var success = 0;
            var failed = 0;
            await foreach (var notificationEntity in _context.GetAllAsync<UserNotificationEntity>(TableNames.UserNotifications, userID))
            {
                if (notificationEntity.Enabled)
                {
                    var endpoint = notificationEntity.Endpoint;
                    var p256dh = notificationEntity.P256dh;
                    var auth = notificationEntity.Auth;

                    var subscription = new PushSubscription(endpoint, p256dh, auth);

                    try
                    {
                        await webPushClient.SendNotificationAsync(subscription, json, vapidDetails);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _log.NotificationsHandlerSendFailed(userID, ex);

                        notificationEntity.Enabled = false;

                        // Disable sending notifications to this endpoint
                        await _context.UpsertAsync(TableNames.UserNotifications, notificationEntity);
                    }
                }
            }

            _log.NotificationsHandlerSendStatistics(success, failed);
        }
    }
}
