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
        private readonly CloudTable _playersTable;
        private readonly CloudTable _gamesTable;
        private readonly CloudTable _settingsTable;
        private readonly ILogger _log;

        public MyChessDataContext(ILogger log, MyChessDataContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _log = log;

            _cloudStorageAccount = CloudStorageAccount.Parse(options.StorageConnectionString);
            var tableClient = _cloudStorageAccount.CreateCloudTableClient();
            _playersTable = tableClient.GetTableReference(TableNames.Players);
            _gamesTable = tableClient.GetTableReference(TableNames.Games);
            _settingsTable = tableClient.GetTableReference(TableNames.Settings);
        }

        public void Initialize()
        {
            _playersTable.CreateIfNotExists();
            _settingsTable.CreateIfNotExists();
            _gamesTable.CreateIfNotExists();
        }

        public async Task<GameEntity?> GetGameAsync(string userID, string gameID)
        {
            var retrieveOperation = TableOperation.Retrieve<GameEntity>(userID, gameID);
            var result = await _gamesTable.ExecuteAsync(retrieveOperation);
            return result.Result as GameEntity;
        }

        public async IAsyncEnumerable<GameEntity> GetGamesAsync(string userID)
        {
            var query = new TableQuery<GameEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userID));
            var token = new TableContinuationToken();

            do
            {
                var result = await _gamesTable.ExecuteQuerySegmentedAsync(query, token);
                foreach (var item in result)
                {
                    yield return item;
                }
                token = result.ContinuationToken;
            } while (token != null);
        }
    }
}
