using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data
{
    public class UserSettingEntity : ITableEntity
    {
        public UserSettingEntity()
        {
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public bool PlayAlwaysUp { get; set; } = false;
    }
}
