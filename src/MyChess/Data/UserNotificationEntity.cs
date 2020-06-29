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

        public string Endpoint { get; set; } = string.Empty;
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;

        public bool Enabled { get; set; } = false;
    }
}
