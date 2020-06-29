using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class UserNotifications
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonPropertyName("p256dh")]
        public string P256dh { get; set; } = string.Empty;

        [JsonPropertyName("auth")]
        public string Auth { get; set; } = string.Empty;
    }
}
