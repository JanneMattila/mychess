using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data
{
    public class GameEntity : ITableEntity
    {
        public GameEntity()
        {
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
        
        public byte[] Data { get; set; } = new byte[0];
    }
}
