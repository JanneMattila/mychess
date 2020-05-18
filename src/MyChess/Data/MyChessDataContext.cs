using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyChess.Data.Internal;

namespace MyChess.Data
{
    public class MyChessDataContext : IMyChessDataContext
    {
        private readonly ILogger<MyChessDataContext> _log;

        private readonly CloudStorageAccount _cloudStorageAccount;

        private readonly CloudTable _usersTable;
        private readonly CloudTable _userFriendsTable;
        private readonly CloudTable _userNotificationsTable;
        private readonly CloudTable _userSettingsTable;
        private readonly CloudTable _userID2UserTable;

        private readonly CloudTable _gamesWaitingForYouTable;
        private readonly CloudTable _gamesWaitingForOpponentTable;
        private readonly CloudTable _gamesArchiveTable;
        private bool _initialized = false;

        public MyChessDataContext(ILogger<MyChessDataContext> log, IOptions<MyChessDataContextOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _log = log;
            _cloudStorageAccount = CloudStorageAccount.Parse(options.Value.StorageConnectionString);

            var tableClient = _cloudStorageAccount.CreateCloudTableClient();
            _usersTable = tableClient.GetTableReference(TableNames.Users);
            _userFriendsTable = tableClient.GetTableReference(TableNames.UserFriends);
            _userNotificationsTable = tableClient.GetTableReference(TableNames.UserNotifications);
            _userSettingsTable = tableClient.GetTableReference(TableNames.UserSettings);
            _userID2UserTable = tableClient.GetTableReference(TableNames.UserID2User);

            _gamesWaitingForYouTable = tableClient.GetTableReference(TableNames.GamesWaitingForYou);
            _gamesWaitingForOpponentTable = tableClient.GetTableReference(TableNames.GamesWaitingForOpponent);
            _gamesArchiveTable = tableClient.GetTableReference(TableNames.GamesArchive);
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                _log.DataContextInitializing();

                var usersTableCreated = _usersTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.Users, usersTableCreated);

                var userFriendsTableCreated = _userFriendsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserFriends, userFriendsTableCreated);

                var userNotificationsTableCreated = _userNotificationsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserNotifications, userNotificationsTableCreated);

                var userSettingsTableCreated = _userSettingsTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserSettings, userSettingsTableCreated);

                var userID2UserTableCreated = _userID2UserTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.UserID2User, userID2UserTableCreated);

                var gamesWaitingForYouTableCreated = _gamesWaitingForYouTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesWaitingForYou, gamesWaitingForYouTableCreated);

                var gamesWaitingForOpponentTableCreated = _gamesWaitingForOpponentTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesWaitingForOpponent, gamesWaitingForOpponentTableCreated);

                var gamesArchiveTableCreated = _gamesArchiveTable.CreateIfNotExists();
                _log.DataContextInitializeTable(TableNames.GamesArchive, gamesArchiveTableCreated);

                _log.DataContextInitialized();
                _initialized = true;
            }
        }

        private CloudTable GetTable(string tableName)
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
            where T : TableEntity
        {
            Initialize();
            var table = GetTable(tableName);
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(retrieveOperation);
            return result.Result as T;
        }

        public async Task<TableResult> UpsertAsync<T>(string tableName, T entity)
            where T : TableEntity
        {
            Initialize();
            var table = GetTable(tableName);
            var upsertOperation = TableOperation.InsertOrReplace(entity);
            return await table.ExecuteAsync(upsertOperation);
        }

        public async Task<TableResult> DeleteAsync<T>(string tableName, T entity)
            where T : TableEntity
        {
            Initialize();
            var table = GetTable(tableName);
            var deleteOperation = TableOperation.Delete(entity);
            return await table.ExecuteAsync(deleteOperation);
        }

        public async IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey)
            where T : TableEntity, new()
        {
            Initialize();
            var table = GetTable(tableName);
            var query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var token = new TableContinuationToken();

            do
            {
                var result = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                foreach (var item in result)
                {
                    yield return item;
                }
                token = result.ContinuationToken;
            } while (token != null);
        }
    }
}
