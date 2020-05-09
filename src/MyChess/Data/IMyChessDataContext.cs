using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public interface IMyChessDataContext
    {
        IAsyncEnumerable<T> GetAllAsync<T>(string tableName, string partitionKey) where T : TableEntity, new();
        Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity;
        void Initialize();
        Task<TableResult> UpsertAsync<T>(string tableName, T entity) where T : TableEntity;
    }
}
