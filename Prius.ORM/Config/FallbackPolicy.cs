using System;
using Newtonsoft.Json;

namespace Prius.Orm.Config
{
    /// <summary>
    /// A fallback policy defines how to determine if a cluster
    /// is healthy, and how long to wait before failing over
    /// to a backup etc.
    /// </summary>
    public class FallbackPolicy
    {
        public FallbackPolicy()
        {
            Name = "default";
            FailureWindowSeconds = 10;
            AllowedFailurePercent = 100;
            WarningFailurePercent = 25;
            BackOffTime = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// This name is referenced by the cluster
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set;}

        /// <summary>
        /// The amount of time over which the database is monitored
        /// to determine if it is healthy or not. Specifying a longer
        /// time will make the cluster fail over less often but it
        /// will take longer after a failure for the system to
        /// recover.
        /// </summary>
        [JsonProperty("failureWindowSeconds")]
        public float FailureWindowSeconds { get; set; }

        /// <summary>
        /// The percentage of requests that can fail within the 
        /// monitoring window without the cluster being marked
        /// as unhealthy. Note that any error returned from the
        /// database will be counted as a failure including
        /// errors resulting from progrmming errors (for example
        /// trying to insert data into a column that is not wide
        /// enough to hold the inserted value).
        /// </summary>
        [JsonProperty("allowedFailurePercent")]
        public int AllowedFailurePercent { get; set; }

        /// <summary>
        /// If the percentage of database requests that fail 
        /// exceeds this threshold then warnings will be reported
        /// back to the application. It is the responsibility of
        /// the application to generate alerts fo this condition.
        /// </summary>
        [JsonProperty("warningFailurePercent")]
        public int WarningFailurePercent { get; set; }

        /// <summary>
        /// After a cluster becomes healthy again it will not be put
        /// back into service immediately because databases take time
        /// to populate caches and become ready to take the full
        /// load. The amount of time to wait for the database to warm
        /// up can be configured here.
        /// </summary>
        [JsonProperty("backOffTime")]
        public TimeSpan BackOffTime { get; set; }
    }

}
