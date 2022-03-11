using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data;

public class GameEntity : ITableEntity
{
    public GameEntity()
    {
    }

    public string PartitionKey { get; set; } = string.Empty;

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public byte[] Data { get; set; } = new byte[0];
}
