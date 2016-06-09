using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Utility;

namespace Prius.Orm.Utility
{
    /// <summary>
    /// This is a fully thread-safe implementation of IHistoryBucketQueue that does not
    /// create and destroy buckets.
    /// </summary>
    public class HistoryBucketQueue<TBucket, TItem> : IHistoryBucketQueue<TBucket, TItem>
    {
        private readonly Action<TBucket, TItem> _accumulateAction;
        private readonly Action<TBucket, long> _clearAction;
        private readonly TBucket[] _buckets;
        private readonly long _ticksPerBucket;

        private int _headIndex;
        private long _headTicks;

        public bool EnumerateHead { get; set; }

        public HistoryBucketQueue(
            TimeSpan depth,
            int bucketCount,
            Func<TBucket> createBucketAction,
            Action<TBucket, TItem> accumulateAction,
            Action<TBucket, long> clearAction)
        {
            _accumulateAction = accumulateAction;
            _clearAction = clearAction;

            _buckets = new TBucket[bucketCount];
            for (var i = 0; i < bucketCount; i++)
                _buckets[i] = createBucketAction();

            _ticksPerBucket = PerformanceTimer.SecondsToTicks(depth.TotalSeconds / bucketCount);

            Clear();
        }

        public void Add(TItem item)
        {
            var bucket = GetCurrentBucket();
            _accumulateAction(bucket, item);
        }

        public bool Add(TItem item, DateTime whenUtc)
        {
            var performanceTimerTicks = PerformanceTimer.TimeNow - PerformanceTimer.SecondsToTicks((DateTime.UtcNow - whenUtc).TotalSeconds);
            return Add(item, performanceTimerTicks);
        }

        public bool Add(TItem item, TimeSpan howLongAgo)
        {
            var performanceTimerTicks = PerformanceTimer.TimeNow - PerformanceTimer.SecondsToTicks(howLongAgo.TotalSeconds);
            return Add(item, performanceTimerTicks);
        }

        public bool Add(TItem item, long performanceTimerTicks)
        {
            var bucket = GetBucket(performanceTimerTicks);
            if (bucket == null) return false;
            _accumulateAction(bucket, item);
            return true;
        }

        public void Clear()
        {
            var ticks = PerformanceTimer.TimeNow - _ticksPerBucket * _buckets.Length;
            for (var bucketIndex = 0; bucketIndex < _buckets.Length; bucketIndex++)
            {
                _clearAction(_buckets[bucketIndex], ticks);
                ticks += _ticksPerBucket;
            }
            _headTicks = ticks;
            _headIndex = _buckets.Length - 1;
        }

        private TBucket GetCurrentBucket()
        {
            lock (_buckets)
            {
                AdvanceHeadToNow();
                return _buckets[_headIndex];
            }
        }

        private TBucket GetBucket(long ticks)
        {
            lock (_buckets)
            {
                AdvanceHeadToNow();
                var distance = (_headTicks - ticks) / _ticksPerBucket;
                if (distance > 0) return default(TBucket);
                var index = _headIndex + distance;
                if (index < 0) index += _buckets.Length;
                if (index < 0) return default(TBucket);
                return _buckets[index];
            }
        }

        private void AdvanceHeadToNow()
        {
            var now = PerformanceTimer.TimeNow;
            var advanceCount = (now - _headTicks) / _ticksPerBucket;
            if (advanceCount >= _buckets.Length)
                Clear();
            else
                while (advanceCount > 0)
                {
                    AdvanceHead();
                    advanceCount = (now - _headTicks) / _ticksPerBucket;
                }
        }

        private void AdvanceHead()
        {
            _headIndex = (_headIndex + 1) % _buckets.Length;
            _headTicks += _ticksPerBucket;
            _clearAction(_buckets[_headIndex], _headTicks);
        }

        public IEnumerator<TBucket> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<TBucket>
        {
            HistoryBucketQueue<TBucket, TItem> _queue;
            int _index;
            bool _locked;

            public Enumerator(HistoryBucketQueue<TBucket, TItem> queue)
            {
                _queue = queue;
                Reset();
            }

            public TBucket Current
            {
                get { return _queue._buckets[_index]; }
            }

            public void Dispose()
            {
                Unlock();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (_index >= _queue._buckets.Length - 1)
                {
                    Unlock();
                    return false;
                }
                _index++;
                if (!_queue.EnumerateHead && _index == _queue._headIndex) return MoveNext();
                return true;
            }

            public void Reset()
            {
                _index = 0;
                Lock();
            }

            private void Lock()
            {
                if (!_locked)
                {
                    Monitor.Enter(_queue._buckets);
                    _locked = true;
                }
            }

            private void Unlock()
            {
                if (_locked)
                {
                    Monitor.Exit(_queue._buckets);
                    _locked = false;
                }
            }
        }
    }
}
