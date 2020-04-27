using System;
using Newtonsoft.Json;

namespace MyChess.Functions.Interfaces
{
    public class GameHeader
    {
        [JsonProperty(PropertyName = "id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "opponent", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Opponent { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "comment", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Comment { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "updated", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset Updated { get; set; }
    }
}
