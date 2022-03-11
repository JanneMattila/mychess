using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data;

public class UserFriendEntity : ITableEntity
{
    public UserFriendEntity()
    {
    }

    public string PartitionKey { get; set; } = string.Empty;

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string Name { get; set; } = string.Empty;
}
