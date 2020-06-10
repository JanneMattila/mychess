using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class NotificationMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}
