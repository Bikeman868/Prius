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

        /// <summary>
        /// The name of this repository. Applications have
        /// this name hard-coded into the source code to identify
        /// the source of data that it wants to access.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A cluster is a collection of databases that contain the
        /// same set of stored procedures and can be used as an
        /// alternate source of the same data. Database requests
        /// for the repository will be sent to the first healthy
        /// cluster defined for that repository.
        /// </summary>
        [JsonProperty("clusters")]
        public List<Cluster> Clusters { get; set; }

        /// <summary>
        /// Configures how stored procedures are executed in this
        /// repository. In particular this defines which stored
        /// procedures must be executed on the replication master
        /// and which can be executed on the replication slave.
        /// </summary>
        [JsonProperty("procedures")]
        public List<StoredProcedure> StoredProcedures { get; set; }
    }

}
