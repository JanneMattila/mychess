using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyChess.Interfaces
{
    public class MyChessGame
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("create")]
        public DateTimeOffset Created { get; set; }

        [JsonPropertyName("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonPropertyName("players")]
        public MyChessGamePlayers Players { get; set; } = new MyChessGamePlayers();

        [JsonPropertyName("state")]
        public string State { get; set; } = GameState.Normal;

        [JsonPropertyName("stateText")]
        public string StateText { get; set; } = GameState.Normal;

        [JsonPropertyName("moves")]
        public List<MyChessGameMove> Moves { get; set; } = new List<MyChessGameMove>();
    }
}
