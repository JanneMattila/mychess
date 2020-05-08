using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace MyChess.Data
{
    public class MyChessDataContext
    {
        private readonly CloudStorageAccount _cloudStorageAccount;
        private readonly ILogger _log;

        private readonly CloudTable _usersTable;
        private readonly CloudTable _userFriendsTable;
        private readonly CloudTable _userNotificationsTable;
        private readonly CloudTable _userSettingsTable;

        private readonly CloudTable _gamesWaitingForYouTable;
        private readonly CloudTable _gamesWaitingForOpponentTable;
        private readonly CloudTable _gamesArchiveTable;

        public MyChessDataContext(ILogger log, MyChessDataContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _log = log;

            _cloudStorageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
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

        public async Task<GameEntity?> GetGameAsync(string userID, string gameID)
        {
            var retrieveOperation = TableOperation.Retrieve<GameEntity>(userID, gameID);
            var result = await _gamesWaitingForYouTable.ExecuteAsync(retrieveOperation);
            return result.Result as GameEntity;
        }

        public async IAsyncEnumerable<GameEntity> GetGamesAsync(string userID)
        {
            var query = new TableQuery<GameEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userID));
            var token = new TableContinuationToken();

            do
            {
                var result = await _gamesWaitingForYouTable.ExecuteQuerySegmentedAsync(query, token);
                foreach (var item in result)
                {
                    yield return item;
                }
                token = result.ContinuationToken;
            } while (token != null);
        }
    }
}
