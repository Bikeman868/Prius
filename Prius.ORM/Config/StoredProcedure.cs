using System.Collections.Generic;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    public class StoredProcedure
    {
        public StoredProcedure()
        {
            TimeoutSeconds = 5;
        }

        /// <summary>
        /// The name of the stored procedure in the database
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// How long to wait for this procedure to complete
        /// </summary>
        [JsonProperty("timeout")]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// This is a list of database roles that are compatible 
        /// with this stored procedure. When this property is
        /// null or an empty list then the stored procedure is
        /// considered compatible with all database roles. Database
        /// roles are typically "Master" and "Slave" but any
        /// role names can be configured.
        /// </summary>
        [JsonProperty("roles")]
        public List<string> AllowedRoles { get; set; }
    }
}
