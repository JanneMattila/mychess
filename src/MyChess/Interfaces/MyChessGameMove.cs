using System;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class MyChessGameMove
    {
        [JsonPropertyName("move")]
        public string Move { get; set; } = string.Empty;

        [JsonPropertyName("specialMove")]
        public string? SpecialMove { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }
    }
}
