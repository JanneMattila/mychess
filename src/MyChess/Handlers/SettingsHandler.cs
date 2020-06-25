using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyChess.Data;
using MyChess.Handlers.Internal;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public class SettingsHandler : BaseHandler, ISettingsHandler
    {
        public SettingsHandler(ILogger<SettingsHandler> log, IMyChessDataContext context)
            : base(log, context)
        {
        }


        public async Task<PlayerSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser)
        {
            var playerSettings = new PlayerSettings();
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var userSettingsEntity = await _context.GetAsync<UserSettingEntity>(TableNames.UserSettings, userID, userID);
            if (userSettingsEntity != null)
            {
                _log.SettingsHandlerSettingsFound(userID);
                playerSettings.PlayAlwaysUp = userSettingsEntity.PlayAlwaysUp;
            }
            else
            {
                _log.SettingsHandlerSettingsNotFound(userID);
            }

            await foreach (var userNotificationEntity in _context.GetAllAsync<UserNotificationEntity>(TableNames.UserNotifications, userID))
            {
                playerSettings.Notifications.Add(new PlayerNotifications()
                {
                    Name = userNotificationEntity.Name,
                    Enabled = userNotificationEntity.Enabled,
                    Uri = userNotificationEntity.Uri,
                });
            }

            _log.SettingsHandlerNotificationsFound(playerSettings.Notifications.Count);

            return playerSettings;
        }

        public async Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, PlayerSettings playerSettings)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);

            await _context.UpsertAsync(TableNames.UserSettings, new UserSettingEntity
            {
                PartitionKey = userID,
                RowKey = userID
            });

            foreach (var userNotifications in playerSettings.Notifications)
            {
                await _context.UpsertAsync(TableNames.UserNotifications, new UserNotificationEntity
                {
                    PartitionKey = userID,
                    RowKey = userNotifications.Name,
                    Enabled = userNotifications.Enabled,
                    Uri = userNotifications.Uri
                });
            }

            return null;
        }
    }
}
