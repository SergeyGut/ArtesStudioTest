using System;
using System.Collections.Generic;
using UnityEngine.Pool;

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

    public static PooledList<T> Get() => new PooledList<T>(ListPool<T>.Get());

    public void Dispose()
    {
        if (!disposed && list != null)
        {
            ListPool<T>.Release(list);
            list = null;
            disposed = true;
        }
    }

    public static implicit operator List<T>(PooledList<T> pooled) => pooled.list;
}

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

    public static PooledDictionary<TKey, TValue> Get() => new PooledDictionary<TKey, TValue>(DictionaryPool<TKey, TValue>.Get());

    public void Dispose()
    {
        if (!disposed && dictionary != null)
        {
            DictionaryPool<TKey, TValue>.Release(dictionary);
            dictionary = null;
            disposed = true;
        }
    }

    public static implicit operator Dictionary<TKey, TValue>(PooledDictionary<TKey, TValue> pooled) => pooled.dictionary;
}

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

    public static PooledHashSet<T> Get() => new PooledHashSet<T>(HashSetPool<T>.Get());

    public void Dispose()
    {
        if (!disposed && hashSet != null)
        {
            HashSetPool<T>.Release(hashSet);
            hashSet = null;
            disposed = true;
        }
    }

    public static implicit operator HashSet<T>(PooledHashSet<T> pooled) => pooled.hashSet;
}

