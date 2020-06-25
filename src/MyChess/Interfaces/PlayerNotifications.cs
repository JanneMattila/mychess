using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class PlayerNotifications
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}
