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
            var userID = await GetOrCreateUserAsync(authenticatedUser);
            var userSettingsEntity = await _context.GetAsync<UserSettingEntity>(TableNames.UserSettings, userID, userID);
            if (userSettingsEntity != null)
            {
                _log.SettingsHandlerSettingsFound(userID);
                userSettings.PlayAlwaysUp = userSettingsEntity.PlayAlwaysUp;
            }
            else
            {
                _log.SettingsHandlerSettingsNotFound(userID);
            }

            await foreach (var userNotificationEntity in _context.GetAllAsync<UserNotificationEntity>(TableNames.UserNotifications, userID))
            {
                userSettings.Notifications.Add(new UserNotifications()
                {
                    Name = userNotificationEntity.Name,
                    Enabled = userNotificationEntity.Enabled,
                    Uri = userNotificationEntity.Uri,
                });
            }

            _log.SettingsHandlerNotificationsFound(userSettings.Notifications.Count);

            return userSettings;
        }

        public async Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, UserSettings userSettings)
        {
            var userID = await GetOrCreateUserAsync(authenticatedUser);

            _log.SettingsHandlerUpdateSettings(userID);

            await _context.UpsertAsync(TableNames.UserSettings, new UserSettingEntity
            {
                PartitionKey = userID,
                RowKey = userID,
                PlayAlwaysUp = userSettings.PlayAlwaysUp
            });

            foreach (var userNotifications in userSettings.Notifications)
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
