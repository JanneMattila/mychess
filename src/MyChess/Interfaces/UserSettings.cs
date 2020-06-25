using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class UserSettings
    {
        [JsonPropertyName("playAlwaysUp")]
        public bool PlayAlwaysUp { get; set; }

        [JsonPropertyName("notifications")]
        public List<UserNotifications> Notifications { get; set; } = new List<UserNotifications>();
    }
}
