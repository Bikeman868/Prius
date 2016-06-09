using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Utility;

namespace Prius.Orm.Utility
{
    public class DictionaryLockTimeoutException : ApplicationException
    {
        public string LockType { get; set; }
        public string MethodName { get; set; }

        public DictionaryLockTimeoutException(object dictionary, string lockType, string methodName)
            : base(string.Format("Failed to acquire {0} lock within timeout period in {1}:{2}", lockType, dictionary.GetType().Name, methodName))
        {
            LockType = lockType;
            MethodName = methodName;
        }
    }

    public class ThreadSafeDictionary<TKey, TValue> : IThreadSafeDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary;
        private ReaderWriterLockSlim _lock;
        private const int _lockTimeout = 10000;

        public ThreadSafeDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public void Add(TKey key, TValue value)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "Add(K, V)");

            try
            {
                _dictionary.Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "ContainsKey()");

            try
            {
                return _dictionary.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool ContainsValue(TValue value)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "ContainsValue()");

            try
            {
                return _dictionary.ContainsValue(value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public ICollection<TKey> Keys
        {
            get { return new KeyCollection(this); }
        }

        public bool Remove(TKey key)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "Remove()");

            try
            {
                return _dictionary.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "TryGetValue()");

            try
            {
                return _dictionary.TryGetValue(key, out value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public ICollection<TValue> Values
        {
            get { return new ValueCollection(this); }
        }

        public TValue GetNextValue(ref int index)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "GetNextValue()");

            try
            {
                if (_dictionary.Count == 0)
                    return default(TValue);

                if (index >= _dictionary.Count) index = 0;

                var value = _dictionary.Values.ElementAt(index);

                index++;
                return value;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_lock.TryEnterReadLock(_lockTimeout))
                    throw new DictionaryLockTimeoutException(this, "read", "get_this()");

                try
                {
                    return _dictionary[key];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                if (!_lock.TryEnterWriteLock(_lockTimeout))
                    throw new DictionaryLockTimeoutException(this, "write", "set_this()");

                try
                {
                    _dictionary[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public IDisposableEnumerable<TKey> KeysLocked
        {
            get
            {
                return new EnumerableLocked<TKey>(Keys,
                    () =>
                    {
                        if (!_lock.TryEnterReadLock(_lockTimeout))
                            throw new DictionaryLockTimeoutException(this, "read", "get_KeysLocked()");
                    },
                    _lock.ExitReadLock);
            }
        }

        public IDisposableEnumerable<TValue> ValuesLocked
        {
            get
            {
                return new EnumerableLocked<TValue>(Values,
                    () =>
                    {
                        if (!_lock.TryEnterReadLock(_lockTimeout))
                            throw new DictionaryLockTimeoutException(this, "read", "get_ValuesLocked()");
                    },
                    _lock.ExitReadLock);
            }
        }

        public IDisposableEnumerable<KeyValuePair<TKey, TValue>> KeyValuePairsLocked
        {
            get
            {
                return new EnumerableLocked<KeyValuePair<TKey, TValue>>(this,
                    () =>
                    {
                        if (!_lock.TryEnterReadLock(_lockTimeout))
                            throw new DictionaryLockTimeoutException(this, "read", "get_KeyValuePairsLocked()");
                    },
                    _lock.ExitReadLock);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "Add(KVP)");

            try
            {
                _dictionary.Add(item.Key, item.Value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "Clear()");

            try
            {
                _dictionary.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "Contains()");

            try
            {
                return _dictionary.ContainsKey(item.Key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "CopyTo()");

            try
            {
                var enumerator = _dictionary.GetEnumerator();
                while (enumerator.MoveNext() && arrayIndex < array.Length)
                    array[arrayIndex++] = enumerator.Current;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                if (!_lock.TryEnterReadLock(_lockTimeout))
                    throw new DictionaryLockTimeoutException(this, "read", "get_Count()");

                try
                {
                    return _dictionary.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> getValueFunction)
        {
            TValue result;

            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "GetOrAdd()");
            try
            {
                if (_dictionary.TryGetValue(key, out result)) return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "GetOrAdd()");
            try
            {
                if (_dictionary.TryGetValue(key, out result)) return result;
                result = getValueFunction(key);
                _dictionary.Add(key, result);
                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "write", "Remove(KVP)");

            try
            {
                return _dictionary.Remove(item.Key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (!_lock.TryEnterReadLock(_lockTimeout))
                throw new DictionaryLockTimeoutException(this, "read", "GetEnumerator()");

            try
            {
                return _dictionary.ToList().GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class KeyValueCollection : ICollection<KeyValuePair<TKey, TValue>>
        {
            private ThreadSafeDictionary<TKey, TValue> _dictionary;

            public KeyValueCollection(ThreadSafeDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public void Add(KeyValuePair<TKey, TValue> keyValuePair)
            {
                _dictionary.Add(keyValuePair);
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
            {
                return _dictionary.Contains(keyValuePair);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext() && arrayIndex < array.Length)
                    array[arrayIndex++] = enumerator.Current;
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<TKey, TValue> keyValuePair)
            {
                return _dictionary.Remove(keyValuePair);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class KeyCollection : ICollection<TKey>
        {
            private ThreadSafeDictionary<TKey, TValue> _dictionary;

            public KeyCollection(ThreadSafeDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public void Add(TKey item)
            {
                _dictionary.Add(item, default(TValue));
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(TKey item)
            {
                return _dictionary.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext() && arrayIndex < array.Length)
                    array[arrayIndex++] = enumerator.Current;
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TKey item)
            {
                return _dictionary.Remove(item);
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return _dictionary.Select(p => p.Key).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ValueCollection : ICollection<TValue>
        {
            private ThreadSafeDictionary<TKey, TValue> _dictionary;

            public ValueCollection(ThreadSafeDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public void Add(TValue item)
            {
                throw new NotImplementedException("You can not add a value without a key");
            }

            public void Clear()
            {
                _dictionary.Clear();
            }

            public bool Contains(TValue item)
            {
                return _dictionary.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext() && arrayIndex < array.Length)
                    array[arrayIndex++] = enumerator.Current;
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TValue item)
            {
                throw new NotImplementedException("You can not remove a value without a key");
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return _dictionary.Select(p => p.Value).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}
