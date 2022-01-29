using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data
{
    public class UserNotificationEntity : ITableEntity
    {
        public UserNotificationEntity()
        {
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;

        public bool Enabled { get; set; } = false;
    }
}
