using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class GameEntity : TableEntity
    {
        public GameEntity()
        {
        }

        public byte[] Data { get; set; } = new byte[0];
    }
}
