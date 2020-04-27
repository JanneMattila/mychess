using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class MyChessGamePlayers
    {
        [JsonPropertyName("white")]
        public Player White { get; set; } = new Player();

        [JsonPropertyName("black")]
        public Player Black { get; set; } = new Player();
    }
}
