using System;
using System.Collections.Generic;
using UnityEngine.Pool;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public static class PoolTracker
{
    private static Dictionary<string, int> activeCounts = new Dictionary<string, int>();
    private static Dictionary<string, int> totalGets = new Dictionary<string, int>();
    private static Dictionary<string, int> totalReleases = new Dictionary<string, int>();
    
    public static void TrackGet(string poolName)
    {
        if (!activeCounts.ContainsKey(poolName))
            activeCounts[poolName] = 0;
        activeCounts[poolName]++;
        
        if (!totalGets.ContainsKey(poolName))
            totalGets[poolName] = 0;
        totalGets[poolName]++;
    }
    
    public static void TrackRelease(string poolName)
    {
        if (activeCounts.ContainsKey(poolName) && activeCounts[poolName] > 0)
            activeCounts[poolName]--;
            
        if (!totalReleases.ContainsKey(poolName))
            totalReleases[poolName] = 0;
        totalReleases[poolName]++;
    }
    
    public static int GetActiveCount(string poolName)
    {
        return activeCounts.ContainsKey(poolName) ? activeCounts[poolName] : 0;
    }
    
    public static void GetStats(out Dictionary<string, int> active, out Dictionary<string, int> gets, out Dictionary<string, int> releases)
    {
        active = new Dictionary<string, int>(activeCounts);
        gets = new Dictionary<string, int>(totalGets);
        releases = new Dictionary<string, int>(totalReleases);
    }
    
    public static void Clear()
    {
        activeCounts.Clear();
        totalGets.Clear();
        totalReleases.Clear();
    }
}
#endif

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
        return new PooledList<T>(ListPool<T>.Get());
    }

    public void Dispose()
    {
        if (!disposed && list != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackRelease($"List<{typeof(T).Name}>");
#endif
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

    public static PooledDictionary<TKey, TValue> Get()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolTracker.TrackGet($"Dictionary<{typeof(TKey).Name},{typeof(TValue).Name}>");
#endif
        return new PooledDictionary<TKey, TValue>(DictionaryPool<TKey, TValue>.Get());
    }

    public void Dispose()
    {
        if (!disposed && dictionary != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackRelease($"Dictionary<{typeof(TKey).Name},{typeof(TValue).Name}>");
#endif
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

    public static PooledHashSet<T> Get()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolTracker.TrackGet($"HashSet<{typeof(T).Name}>");
#endif
        return new PooledHashSet<T>(HashSetPool<T>.Get());
    }

    public void Dispose()
    {
        if (!disposed && hashSet != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PoolTracker.TrackRelease($"HashSet<{typeof(T).Name}>");
#endif
            HashSetPool<T>.Release(hashSet);
            hashSet = null;
            disposed = true;
        }
    }

    public static implicit operator HashSet<T>(PooledHashSet<T> pooled) => pooled.hashSet;
}

