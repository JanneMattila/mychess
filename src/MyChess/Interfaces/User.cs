using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class User
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
