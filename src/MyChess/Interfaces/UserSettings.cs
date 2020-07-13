using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class UserSettings
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("playAlwaysUp")]
        public bool PlayAlwaysUp { get; set; }

        [JsonPropertyName("notifications")]
        public List<UserNotifications> Notifications { get; set; } = new List<UserNotifications>();
    }
}
