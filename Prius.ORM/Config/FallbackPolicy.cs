using System;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class FallbackPolicy
    {
        [JsonProperty("name")]
        public string Name { get; set;}

        [JsonProperty("failureWindowSeconds")]
        public float FailureWindowSeconds { get; set; }

        [JsonProperty("allowedFailurePercent")]
        public int AllowedFailurePercent { get; set; }

        [JsonProperty("warningFailurePercent")]
        public int WarningFailurePercent { get; set; }

        [JsonProperty("backOffTime")]
        public TimeSpan BackOffTime { get; set; }
    }

}
