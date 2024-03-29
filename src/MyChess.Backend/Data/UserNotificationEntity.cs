﻿using System;
using Azure;
using Azure.Data.Tables;

namespace MyChess.Backend.Data;

public class UserNotificationEntity : ITableEntity
{
    public UserNotificationEntity()
    {
    }

    public string PartitionKey { get; set; } = string.Empty;

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public int FailedCount { get; set; } = 0;

    public bool Enabled { get; set; } = false;
}
