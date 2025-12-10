using System;
using System.Collections.Generic;

public struct PooledHashSet<T> : IDisposable
{
    private HashSet<T> hashSet;
    private bool disposed;

    public HashSet<T> Value => hashSet;

    public PooledHashSet(HashSet<T> hashSet)
    {
        this.hashSet = hashSet;
        this.disposed = false;
    }

    public static PooledHashSet<T> Get()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolTracker.TrackGet($"HashSet<{typeof(T).Name}>");
#endif
        return new PooledHashSet<T>(CollectionPool<HashSet<T>>.Get());
    }

    public void Dispose()
    {
        if (!disposed && hashSet != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackRelease($"HashSet<{typeof(T).Name}>");
#endif
            CollectionPool<HashSet<T>>.Release(hashSet);
            hashSet = null;
            disposed = true;
        }
    }

    public static implicit operator HashSet<T>(PooledHashSet<T> pooled) => pooled.hashSet;
}

