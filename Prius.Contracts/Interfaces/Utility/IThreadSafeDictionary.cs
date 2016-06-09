using System;
using System.Collections.Generic;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Utility
{
    public interface IThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets a value from the dictionary if it contains it, otherwise adds the
        /// value to the dictionary and returns it in a thread-safe manner.
        /// </summary>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> getValueFunction);

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IDisposableEnumerable<TKey> KeysLocked { get; }

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IDisposableEnumerable<TValue> ValuesLocked { get; }

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IDisposableEnumerable<KeyValuePair<TKey, TValue>> KeyValuePairsLocked { get; }

        /// <summary>
        /// Returns the value at the specified index, and increments the index.
        /// Rolls the index over to 0 if it is beyond the end of the dictionary.
        /// </summary>
        TValue GetNextValue(ref int index);
    }
}
