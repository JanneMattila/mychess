using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class MyChessGamePlayers
    {
        [JsonPropertyName("white")]
        public User White { get; set; } = new User();

        [JsonPropertyName("black")]
        public User Black { get; set; } = new User();
    }
}
