using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data;

public class UserEntity : ITableEntity
{
    public UserEntity()
    {
    }

    public string PartitionKey { get; set; } = string.Empty;

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string Name { get; set; } = string.Empty;

    public string UserID { get; set; } = string.Empty;

    public bool Enabled { get; set; } = false;

    public DateTime Created { get; set; }
}
