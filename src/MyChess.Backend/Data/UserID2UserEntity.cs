using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data
{
    public class UserID2UserEntity : ITableEntity
    {
        public UserID2UserEntity()
        {
        }

        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string UserPrimaryKey { get; set; } = string.Empty;

        public string UserRowKey { get; set; } = string.Empty;
    }
}
