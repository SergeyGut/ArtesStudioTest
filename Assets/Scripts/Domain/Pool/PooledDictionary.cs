using System;
using System.Collections.Generic;

namespace Domain.Pool
{
    public struct PooledDictionary<TKey, TValue> : IDisposable
    {
        private Dictionary<TKey, TValue> dictionary;
        private bool disposed;

        public Dictionary<TKey, TValue> Value => dictionary;

        public PooledDictionary(Dictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
            this.disposed = false;
        }

        public static PooledDictionary<TKey, TValue> Get()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackGet($"Dictionary<{typeof(TKey).Name},{typeof(TValue).Name}>");
#endif
            return new PooledDictionary<TKey, TValue>(CollectionPool<Dictionary<TKey, TValue>>.Get());
        }

        public void Dispose()
        {
            if (!disposed && dictionary != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                PoolTracker.TrackRelease($"Dictionary<{typeof(TKey).Name},{typeof(TValue).Name}>");
#endif
                CollectionPool<Dictionary<TKey, TValue>>.Release(dictionary);
                dictionary = null;
                disposed = true;
            }
        }

        public static implicit operator Dictionary<TKey, TValue>(PooledDictionary<TKey, TValue> pooled) =>
            pooled.dictionary;
    }
}