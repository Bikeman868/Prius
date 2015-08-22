using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
{
    /// <summary>
    /// Provides a container for buckets of items where the buckets slip
    /// backwards in the list over time and new items are added to the
    /// current time bucket. This is particlarly useful for calculating a
    /// sliding average over the last short while.
    /// Note that when enumerating the collection, the bucket at the head is
    /// excluded because this bucket is incomplete, and may even be completely
    /// empty if the time interval just rolled over.
    /// </summary>
    /// <typeparam name="TBucket">The type that represents a bucket</typeparam>
    /// <typeparam name="TItem">The type it item stored in each bucket</typeparam>
    public interface IHistoryBucketQueue<TBucket, TItem> : IEnumerable<TBucket>
    {
        /// <summary>
        /// Set to true if you want to include the head when enumerating buckets
        /// </summary>
        bool EnumerateHead { get; set; }

        /// <summary>
        /// Clears the queue
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds an item to the current time bucket
        /// </summary>
        void Add(TItem item);

        /// <summary>
        /// Adds an item to a specific time bucket.
        /// Returns false if this time is not within the bucket queues history depth
        /// </summary>
        bool Add(TItem item, DateTime whenUtc);

        /// <summary>
        /// Adds an item to a specific time bucket.
        /// Returns false if this time is not within the bucket queues history depth
        /// </summary>
        bool Add(TItem item, TimeSpan howLongAgo);

        /// <summary>
        /// Adds an item to a specific time bucket based on PerformanceTimer ticks.
        /// Returns false if this time is not within the bucket queues history depth
        /// </summary>
        bool Add(TItem item, long performanceTimerTicks);
    }
}
