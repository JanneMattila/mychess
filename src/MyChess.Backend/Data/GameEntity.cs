using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Backend.Data
{
    public class GameEntity : TableEntity
    {
        public GameEntity()
        {
        }

        public byte[] Data { get; set; } = new byte[0];
    }
}
