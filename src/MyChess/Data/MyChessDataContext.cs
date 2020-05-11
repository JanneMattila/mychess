using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

namespace MyChess.Data
{
    public class MyChessDataContext : IMyChessDataContext
    {
        private readonly CloudStorageAccount _cloudStorageAccount;

        private readonly CloudTable _usersTable;
        private readonly CloudTable _userFriendsTable;
        private readonly CloudTable _userNotificationsTable;
        private readonly CloudTable _userSettingsTable;

        private readonly CloudTable _gamesWaitingForYouTable;
        private readonly CloudTable _gamesWaitingForOpponentTable;
        private readonly CloudTable _gamesArchiveTable;

        public MyChessDataContext(IOptions<MyChessDataContextOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _cloudStorageAccount = CloudStorageAccount.Parse(options.Value.StorageConnectionString);

            var tableClient = _cloudStorageAccount.CreateCloudTableClient();
            _usersTable = tableClient.GetTableReference(TableNames.Users);
            _userFriendsTable = tableClient.GetTableReference(TableNames.UserFriends);
            _userNotificationsTable = tableClient.GetTableReference(TableNames.UserNotifications);
            _userSettingsTable = tableClient.GetTableReference(TableNames.UserSettings);

            _gamesWaitingForYouTable = tableClient.GetTableReference(TableNames.GamesWaitingForYou);
            _gamesWaitingForOpponentTable = tableClient.GetTableReference(TableNames.GamesWaitingForOpponent);
            _gamesArchiveTable = tableClient.GetTableReference(TableNames.GamesArchive);
        }

        public void Initialize()
        {
            _usersTable.CreateIfNotExists();
            _userFriendsTable.CreateIfNotExists();
            _userNotificationsTable.CreateIfNotExists();
            _userSettingsTable.CreateIfNotExists();

            _gamesWaitingForYouTable.CreateIfNotExists();
            _gamesWaitingForOpponentTable.CreateIfNotExists();
            _gamesArchiveTable.CreateIfNotExists();
        }

        private CloudTable GetTable(string tableName)
        {
            return tableName switch
            {
                TableNames.Users => _usersTable,
                TableNames.UserFriends => _userFriendsTable,
                TableNames.UserNotifications => _userNotificationsTable,
                TableNames.UserSettings => _userSettingsTable,
                TableNames.GamesWaitingForYou => _gamesWaitingForYouTable,
                TableNames.GamesWaitingForOpponent => _gamesWaitingForOpponentTable,
                TableNames.GamesArchive => _gamesArchiveTable,
                _ => throw new ArgumentOutOfRangeException(nameof(tableName))
            };
        }

        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey)
            where T : TableEntity
        {
            var table = GetTable(tableName);
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(retrieveOperation);
            return result.Result as T;
        }

        public async Task<TableResult> UpsertAsync<T>(string tableName, T entity)
            where T : TableEntity
        {
            var table = GetTable(tableName);
            var upsertOperation = TableOperation.InsertOrReplace(entity);
            return await table.ExecuteAsync(upsertOperation);
        }

        public async IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey)
            where T : TableEntity, new()
        {
            var query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var token = new TableContinuationToken();
            var table = GetTable(tableName);

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
