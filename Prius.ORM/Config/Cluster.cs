using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class Cluster : ConfigurationElement
    {
        [JsonProperty("databases")]
        public List<string> Databases { get; set; }

        [JsonProperty("sequence")]
        public int SequenceNumber { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("fallbackPolicy")]
        public string FallbackPolicyName { get; set; }
    }
}
