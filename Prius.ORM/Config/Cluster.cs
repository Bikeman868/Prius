using System.Collections.Generic;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    /// <summary>
    /// A cluster is a collection of databases that can have requests
    /// sent to them in a round-robin fashion to distribute load. All
    /// databases in the cluster must provide the same set of stored
    /// procedures and access the same data (or a copy of the data).
    /// </summary>
    public class Cluster
    {
        public Cluster()
        {
            DatabaseNames = new List<string>();
            Enabled = true;
            FallbackPolicyName = "default";
        }

        /// <summary>
        /// Optional name for this cluster
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Database requests for this cluster will be distributed in
        /// round-robin fashion between the databases listed here
        /// </summary>
        [JsonProperty("databases")]
        public List<string> DatabaseNames { get; set; }

        /// <summary>
        /// The repository will only have one active cluster and will direct
        /// all database requests to that cluster. The active cluster will
        /// be the one which is enabled, healthy and has the lowest sequence number.
        /// </summary>
        [JsonProperty("sequence")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Set to false to stop any requests from going to this cluster. You might
        /// want to do this during scheduled maintenance.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Specifies how to determine when to stop using this cluster because it
        /// has become unhealthy, and how long to wait after it becomes healthy
        /// again before sending database requests to it.
        /// </summary>
        [JsonProperty("fallbackPolicy")]
        public string FallbackPolicyName { get; set; }
    }
}
