using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class Repository
    {
        public Repository()
        {
            Clusters = new List<Cluster>();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clusters")]
        public List<Cluster> Clusters { get; set; }
    }

}
