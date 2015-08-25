using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{

    public class Database
    {
        public Database()
        {
            ServerType = ServerType.SqlServer;
            Enabled = true;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sequenceNumber")]
        public int SequenceNumber { get; set; }

        [JsonProperty("type")]
        public ServerType ServerType { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
