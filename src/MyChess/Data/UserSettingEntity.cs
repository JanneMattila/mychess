﻿using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class UserSettingEntity : TableEntity
    {
        public UserSettingEntity()
        {
        }

        public bool PlayAlwaysUp { get; set; } = false;
    }
}
