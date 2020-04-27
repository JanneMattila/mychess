using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class GameEntity : TableEntity
    {
        public GameEntity()
        {
        }

        public string Data { get; set; } = string.Empty;
    }
}
