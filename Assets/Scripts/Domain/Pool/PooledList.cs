using System;
using System.Collections.Generic;

public struct PooledList<T> : IDisposable
{
    private List<T> list;
    private bool disposed;

    public List<T> Value => list;

    public PooledList(List<T> list)
    {
        this.list = list;
        this.disposed = false;
    }

    public static PooledList<T> Get()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolTracker.TrackGet($"List<{typeof(T).Name}>");
#endif
        return new PooledList<T>(CollectionPool<List<T>>.Get());
    }

    public void Dispose()
    {
        if (!disposed && list != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackRelease($"List<{typeof(T).Name}>");
#endif
            CollectionPool<List<T>>.Release(list);
            list = null;
            disposed = true;
        }
    }

    public static implicit operator List<T>(PooledList<T> pooled) => pooled.list;
}

