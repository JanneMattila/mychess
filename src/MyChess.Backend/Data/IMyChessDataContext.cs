using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace MyChess.Backend.Data;

public interface IMyChessDataContext
{
    IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey) where T : class, ITableEntity, new();
    Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
    void Initialize();
    Task UpsertAsync<T>(string tableName, T entity) where T : ITableEntity;
    Task DeleteAsync<T>(string tableName, T entity) where T : ITableEntity;
}
