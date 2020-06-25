using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class PlayerSettings
    {
        [JsonPropertyName("playAlwaysUp")]
        public bool PlayAlwaysUp { get; set; }

        [JsonPropertyName("notifications")]
        public List<PlayerNotifications> Notifications { get; set; } = new List<PlayerNotifications>();
    }
}
