using System.Collections.Generic;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class Cluster
    {
        public Cluster()
        {
            DatabaseNames = new List<string>();
            Enabled = true;
            FallbackPolicyName = "default";
        }

        [JsonProperty("databases")]
        public List<string> DatabaseNames { get; set; }

        [JsonProperty("sequence")]
        public int SequenceNumber { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("fallbackPolicy")]
        public string FallbackPolicyName { get; set; }
    }
}
