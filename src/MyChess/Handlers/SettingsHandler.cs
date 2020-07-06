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


        public async Task<UserSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser)
        {
            var userSettings = new UserSettings();
            var user = await GetOrCreateUserAsync(authenticatedUser);
            var userSettingsEntity = await _context.GetAsync<UserSettingEntity>(TableNames.UserSettings, user.UserID, user.UserID);
            if (userSettingsEntity != null)
            {
                _log.SettingsHandlerSettingsFound(user.UserID);
                userSettings.PlayAlwaysUp = userSettingsEntity.PlayAlwaysUp;
            }
            else
            {
                _log.SettingsHandlerSettingsNotFound(user.UserID);
            }

            await foreach (var userNotificationEntity in _context.GetAllAsync<UserNotificationEntity>(TableNames.UserNotifications, user.UserID))
            {
                userSettings.Notifications.Add(new UserNotifications()
                {
                    Name = userNotificationEntity.Name,
                    Enabled = userNotificationEntity.Enabled,
                    Endpoint = userNotificationEntity.Endpoint,
                    P256dh = userNotificationEntity.P256dh,
                    Auth = userNotificationEntity.Auth,
                });
            }

            _log.SettingsHandlerNotificationsFound(userSettings.Notifications.Count);

            return userSettings;
        }

        public async Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, UserSettings userSettings)
        {
            var user = await GetOrCreateUserAsync(authenticatedUser);

            _log.SettingsHandlerUpdateSettings(user.UserID);

            await _context.UpsertAsync(TableNames.UserSettings, new UserSettingEntity
            {
                PartitionKey = user.UserID,
                RowKey = user.UserID,
                PlayAlwaysUp = userSettings.PlayAlwaysUp
            });

            // Remove any existing user notifications
            await foreach (var notificationEntity in _context.GetAllAsync<UserNotificationEntity>(TableNames.UserNotifications, user.UserID))
            {
                await _context.DeleteAsync(TableNames.UserNotifications, notificationEntity);
            }

            foreach (var userNotifications in userSettings.Notifications)
            {
                await _context.UpsertAsync(TableNames.UserNotifications, new UserNotificationEntity
                {
                    PartitionKey = user.UserID,
                    RowKey = userNotifications.Name,
                    Name = userNotifications.Name,
                    Enabled = userNotifications.Enabled,
                    Endpoint = userNotifications.Endpoint,
                    P256dh = userNotifications.P256dh,
                    Auth = userNotifications.Auth,
                });
            }

            return null;
        }
    }
}
