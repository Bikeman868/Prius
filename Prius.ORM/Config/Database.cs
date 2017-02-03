using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{

    public class Database
    {
        public Database()
        {
            ServerType = "SqlServer";
            Enabled = true;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sequenceNumber")]
        public int SequenceNumber { get; set; }

        [JsonProperty("type")]
        public string ServerType { get; set; }

        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("procedures")]
        public List<StoredProcedure> StoredProcedures { get; set; }
    }
}
