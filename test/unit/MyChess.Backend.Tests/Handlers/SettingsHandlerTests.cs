using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Backend.Tests.Handlers.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Backend.Tests.Handlers
{
    public class SettingsHandlerTests
    {
        private readonly SettingsHandler _settingsHandler;
        private readonly MyChessContextStub _context;

        public SettingsHandlerTests()
        {
            _context = new MyChessContextStub();
            _settingsHandler = new SettingsHandler(NullLogger<SettingsHandler>.Instance, _context);
        }

        [Fact]
        public async Task Get_Settings_As_New_User()
        {
            // Arrange
            var expected = 0;
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            // Act
            var actual = await _settingsHandler.GetSettingsAsync(user);

            // Assert
            Assert.Equal(expected, actual.Notifications.Count);
        }

        [Fact]
        public async Task Get_Settings_As_Existing_User_With_Notification()
        {
            // Arrange
            var expectedNotifications = 1;
            var expectedPlayAlwaysUp = true;
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u",
                RowKey = "p",
                UserID = "user123"
            });
            await _context.UpsertAsync(TableNames.UserSettings, new UserSettingEntity()
            {
                PartitionKey = "user123",
                RowKey = "user123",
                PlayAlwaysUp = true
            });
            await _context.UpsertAsync(TableNames.UserNotifications, new UserNotificationEntity()
            {
                PartitionKey = "user123",
                RowKey = "user123",
                Name = "Browser"
            });

            // Act
            var actual = await _settingsHandler.GetSettingsAsync(user);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expectedNotifications, actual.Notifications.Count);
            Assert.Equal(expectedPlayAlwaysUp, actual.PlayAlwaysUp);
        }

        [Fact]
        public async Task Update_Settings_Test()
        {
            // Arrange
            var expectedRows = 1;
            var expectedPlayAlwaysUp = true;
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            var playerSettings = new UserSettings();
            playerSettings.PlayAlwaysUp = true;

            // Act
            var actual = await _settingsHandler.UpdateSettingsAsync(user, playerSettings);

            // Assert
            Assert.Null(actual);
            var table = _context.Tables[TableNames.UserSettings];
            Assert.Equal(expectedRows, table.Count);
            var row = table.First() as UserSettingEntity;
            Assert.Equal(expectedPlayAlwaysUp, row.PlayAlwaysUp);
        }
    }
}
