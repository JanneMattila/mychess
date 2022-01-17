using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyChess.Backend.Data.Internal;

namespace MyChess.Backend.Data
{
    public class MyChessDataContext : IMyChessDataContext
    {
        private readonly ILogger<MyChessDataContext> _log;

        private readonly TableClient _usersTable;
        private readonly TableClient _userFriendsTable;
        private readonly TableClient _userNotificationsTable;
        private readonly TableClient _userSettingsTable;
        private readonly TableClient _userID2UserTable;

        private readonly TableClient _gamesWaitingForYouTable;
        private readonly TableClient _gamesWaitingForOpponentTable;
        private readonly TableClient _gamesArchiveTable;
        private bool _initialized = false;
        private string _configuration;

        public MyChessDataContext(ILogger<MyChessDataContext> log, IOptions<MyChessDataContextOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _log = log;
            _configuration = options.Value.StorageConnectionString;

            _usersTable = CreateTable(TableNames.Users);
            _userFriendsTable = CreateTable(TableNames.UserFriends);
            _userNotificationsTable = CreateTable(TableNames.UserNotifications);
            _userSettingsTable = CreateTable(TableNames.UserSettings);
            _userID2UserTable = CreateTable(TableNames.UserID2User);

            _gamesWaitingForYouTable = CreateTable(TableNames.GamesWaitingForYou);
            _gamesWaitingForOpponentTable = CreateTable(TableNames.GamesWaitingForOpponent);
            _gamesArchiveTable = CreateTable(TableNames.GamesArchive);
        }

        private TableClient CreateTable(string tableName)
        {
            // https://github.com/Azure/static-web-apps/issues/466
            // https://docs.microsoft.com/en-us/azure/static-web-apps/apis
            // Managed Functions -> Managed identity not supported.
            //var credential = new DefaultAzureCredential();
            //var tableStorageUri = new Uri(_configuration);
            //return new TableClient(tableStorageUri, tableName, credential);
            return new TableClient(_configuration, tableName);
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                _log.DataContextInitializing();

                _usersTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.Users, false);

                _userFriendsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserFriends, false);

                _userNotificationsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserNotifications, false);

                _userSettingsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserSettings, false);

                _userID2UserTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserID2User, false);

                _gamesWaitingForYouTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesWaitingForYou, false);

                _gamesWaitingForOpponentTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesWaitingForOpponent, false);

                _gamesArchiveTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesArchive, false);

                _log.DataContextInitialized();
                _initialized = true;
            }
        }

        private TableClient GetTable(string tableName)
        {
            return tableName switch
            {
                TableNames.Users => _usersTable,
                TableNames.UserFriends => _userFriendsTable,
                TableNames.UserNotifications => _userNotificationsTable,
                TableNames.UserSettings => _userSettingsTable,
                TableNames.UserID2User => _userID2UserTable,
                TableNames.GamesWaitingForYou => _gamesWaitingForYouTable,
                TableNames.GamesWaitingForOpponent => _gamesWaitingForOpponentTable,
                TableNames.GamesArchive => _gamesArchiveTable,
                _ => throw new ArgumentOutOfRangeException(nameof(tableName))
            };
        }

        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey)
            where T : class, ITableEntity, new()
        {
            Initialize();
            var table = GetTable(tableName);
            var entity = await table.GetEntityAsync<T>(partitionKey, rowKey);
            return entity.Value as T;
        }

        public async Task UpsertAsync<T>(string tableName, T entity)
            where T : ITableEntity
        {
            Initialize();
            var table = GetTable(tableName);
            await table.UpsertEntityAsync<T>(entity);
        }

        public async Task DeleteAsync<T>(string tableName, T entity)
            where T : ITableEntity
        {
            Initialize();
            var table = GetTable(tableName);
            await table.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, ETag.All);
        }

        public async IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey)
            where T : class, ITableEntity, new()
        {
            Initialize();
            var table = GetTable(tableName);
            var query = table.QueryAsync<T>($"PartitionKey eq '{partitionKey}'");
            var result = query.AsPages(string.Empty);
            await foreach (var items in result)
            {
                foreach (var item in items.Values)
                {
                    yield return item;
                }
            }
        }
    }
}
