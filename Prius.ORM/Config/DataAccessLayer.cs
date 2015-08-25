using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class DataAccessLayer
    {
        public DataAccessLayer()
        {
            Databases = new List<Database>();
            FallbackPolicies = new List<FallbackPolicy>();
            Repositories = new List<Repository>();
        }

        [JsonProperty("databases")]
        public List<Database> Databases { get; set; }

        [JsonProperty("fallbackPolicies")]
        public List<FallbackPolicy> FallbackPolicies { get; set; }

        [ConfigurationProperty("repositories")]
        public List<Repository> Repositories { get; set; }
    }

}
