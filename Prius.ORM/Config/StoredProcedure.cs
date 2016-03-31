using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class StoredProcedure
    {
        public StoredProcedure()
        {
            TimeoutSeconds = 5;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("timeout")]
        public int TimeoutSeconds { get; set; }
    }
}
