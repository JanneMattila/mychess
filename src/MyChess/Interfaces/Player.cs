using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class Player
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
