using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using MyChess.Data;

namespace MyChess.Tests.Handlers.Stubs
{
    public class MyChessContextStub : IMyChessDataContext
    {
        public Dictionary<string, List<TableEntity>> Tables { get; set; } = new Dictionary<string, List<TableEntity>>();

        public MyChessContextStub()
        {
            Tables[TableNames.Users] = new List<TableEntity>();
            Tables[TableNames.UserFriends] = new List<TableEntity>();
            Tables[TableNames.UserNotifications] = new List<TableEntity>();
            Tables[TableNames.UserSettings] = new List<TableEntity>();
            Tables[TableNames.UserID2User] = new List<TableEntity>();
            Tables[TableNames.GamesWaitingForYou] = new List<TableEntity>();
            Tables[TableNames.GamesWaitingForOpponent] = new List<TableEntity>();
            Tables[TableNames.GamesArchive] = new List<TableEntity>();
        }

        public void Initialize()
        {
        }

        public async Task<TableResult> DeleteAsync<T>(string tableName, T entity) where T : TableEntity
        {
            Tables[tableName].RemoveAll(r => r.PartitionKey == entity.PartitionKey && r.RowKey == entity.RowKey);
            return await Task.FromResult(new TableResult());
        }

        public async IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey) where T : TableEntity, new()
        {
            var list = Tables[tableName];
            foreach (var item in list)
            {
#pragma warning disable CS8603 // Possible null reference return.
                yield return item as T;
#pragma warning restore CS8603 // Possible null reference return.
            }
            await Task.CompletedTask;
        }

        public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity
        {
            var list = Tables[tableName];
            var item = list.FirstOrDefault(t => t.PartitionKey == partitionKey && t.RowKey == rowKey);
            return await Task.FromResult(item as T);
        }

        public async Task<TableResult> UpsertAsync<T>(string tableName, T entity) where T : TableEntity
        {
            await DeleteAsync(tableName, entity);
            Tables[tableName].Add(entity);
            return await Task.FromResult(new TableResult());
        }
    }
}
