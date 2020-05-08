using System;
using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class UserEntity : TableEntity
    {
        public UserEntity()
        {
        }

        public string Name { get; set; } = string.Empty;

        public string UserID { get; set; } = string.Empty;

        public bool Enabled { get; set; } = false;

        public DateTimeOffset Created { get; set; }
    }
}
