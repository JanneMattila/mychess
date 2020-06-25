using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class PlayerSettings
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("notifications")]
        public List<PlayerNotifications> Notifications { get; set; } = new List<PlayerNotifications>();
    }
}
