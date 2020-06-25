using System;
using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class UserNotificationEntity : TableEntity
    {
        public UserNotificationEntity()
        {
        }

        public string Name { get; set; } = string.Empty;

        public string Uri { get; set; } = string.Empty;

        public bool Enabled { get; set; } = false;
    }
}
