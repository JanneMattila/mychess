using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using MyChess.Backend.Data;

namespace MyChess.Backend.Tests.Handlers.Stubs;

public class MyChessContextStub : IMyChessDataContext
{
    public Dictionary<string, List<ITableEntity>> Tables { get; set; } = new Dictionary<string, List<ITableEntity>>();

    public MyChessContextStub()
    {
        Tables[TableNames.Users] = new List<ITableEntity>();
        Tables[TableNames.UserFriends] = new List<ITableEntity>();
        Tables[TableNames.UserNotifications] = new List<ITableEntity>();
        Tables[TableNames.UserSettings] = new List<ITableEntity>();
        Tables[TableNames.UserID2User] = new List<ITableEntity>();
        Tables[TableNames.GamesWaitingForYou] = new List<ITableEntity>();
        Tables[TableNames.GamesWaitingForOpponent] = new List<ITableEntity>();
        Tables[TableNames.GamesArchive] = new List<ITableEntity>();
    }

    public void Initialize()
    {
    }

    public async Task DeleteAsync<T>(string tableName, T entity) where T : ITableEntity
    {
        Tables[tableName].RemoveAll(r => r.PartitionKey == entity.PartitionKey && r.RowKey == entity.RowKey);
        await Task.CompletedTask;
    }

    public async IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new()
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

    public async Task<T> GetAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        var list = Tables[tableName];
        var item = list.FirstOrDefault(t => t.PartitionKey == partitionKey && t.RowKey == rowKey);
        return await Task.FromResult(item as T);
    }

    public async Task UpsertAsync<T>(string tableName, T entity) where T : ITableEntity
    {
        await DeleteAsync(tableName, entity);
        Tables[tableName].Add(entity);
        await Task.CompletedTask;
    }
}
