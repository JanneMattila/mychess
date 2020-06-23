using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Interfaces;
using WebPush;

namespace MyChess.Handlers
{
    public class NotificationHandler : BaseHandler
    {
        private readonly NotificationOptions _options;

        public NotificationHandler(ILogger<NotificationHandler> log, IMyChessDataContext context, NotificationOptions options)
            : base(log, context)
        {
            _options = options;
        }

        public async Task SendNotificationAsync(string userID, string gameID, string comment)
        {
            // TODO: Fetch from storage
            var endpoint = string.Empty;
            var p256dh = string.Empty;
            var auth = string.Empty;

            var uri = $"/play/{gameID}";
            var subscription = new PushSubscription(endpoint, p256dh, auth);
            var vapidDetails = new VapidDetails($"{_options.PublicServerUri}{uri}", _options.PublicKey, _options.PrivateKey);
            var webPushClient = new WebPushClient();
            var json = JsonSerializer.Serialize(new NotificationMessage()
            {
                Text = comment,
                Uri = uri
            });
            await webPushClient.SendNotificationAsync(subscription, json, vapidDetails);
        }
    }
}
