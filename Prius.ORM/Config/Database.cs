using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    /// <summary>
    /// A database in Prius is an instance of a database server.
    /// Prius supports a number of database technologies including
    /// SQL Server, MySQL, Posgresql, SQLite.
    /// </summary>
    public class Database
    {
        public Database()
        {
            ServerType = "SqlServer";
            Enabled = true;
            Role = "Master";
        }

        /// <summary>
        /// These names are referred to in the cluster configurations
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Defines the order that databases will be accessed when 
        /// distributing requests
        /// </summary>
        [JsonProperty("sequenceNumber")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Specifies which driver software will be used to communicate 
        /// with the database. You must install a package that supports
        /// type of database specified here. Examples are SqlServer,
        /// MySQL, SQLite etc.
        /// </summary>
        [JsonProperty("type")]
        public string ServerType { get; set; }

        /// <summary>
        /// The format of this connection string is specific to the
        /// ServerType specified
        /// </summary>
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Set this to false to stop any requests from being sent to
        /// this database. This is useful during planned maintenance.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Configures how stored procedures are executed against this
        /// database.
        /// </summary>
        [JsonProperty("procedures")]
        public List<StoredProcedure> StoredProcedures { get; set; }

        /// <summary>
        /// The name of this database's replication role. Most often
        /// the roles are called "master" and "slave". Stored procedures
        /// can be configured with a list of the database roles that are
        /// compatible with the stored procedure. For example a replication
        /// master might allow reads and writes whereas a replication slave
        /// might support reads only. If a stored procedure writes to the
        /// database  then it must run on the master and can not run on a
        /// slave.
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
