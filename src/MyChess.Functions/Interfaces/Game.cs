using System;
using Newtonsoft.Json;

namespace MyChess.Functions.Interfaces
{
    public class Game
    {
        [JsonProperty(PropertyName = "id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "opponent", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Opponent { get; set; }

        [JsonProperty(PropertyName = "updated", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset Updated { get; set; }
    }
}
