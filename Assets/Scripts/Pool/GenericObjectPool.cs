using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> : IObjectPool<T> where T : Component
{
    private readonly Dictionary<T, Queue<T>> availableObjectsByPrefab = new Dictionary<T, Queue<T>>();
    private readonly Dictionary<T, T> instanceToPrefabMap = new Dictionary<T, T>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private readonly Transform parentTransform;

    public int AvailableCount
    {
        get
        {
            int count = 0;
            foreach (var queue in availableObjectsByPrefab.Values)
                count += queue.Count;
            return count;
        }
    }

    public int ActiveCount => activeObjects.Count;

    public GenericObjectPool(Transform parent = null)
    {
        parentTransform = parent;
    }

    public T Get(T prefab)
    {
        if (!availableObjectsByPrefab.ContainsKey(prefab))
            availableObjectsByPrefab[prefab] = new Queue<T>();

        Queue<T> prefabPool = availableObjectsByPrefab[prefab];
        T instance;

        if (prefabPool.Count > 0)
        {
            instance = prefabPool.Dequeue();
            instance.gameObject.SetActive(true);
        }
        else
        {
            instance = Object.Instantiate(prefab);
            if (parentTransform != null)
                instance.transform.SetParent(parentTransform);
            
            instanceToPrefabMap[instance] = prefab;
        }

        activeObjects.Add(instance);

        if (instance is IPoolable poolable)
            poolable.OnSpawnFromPool();

        return instance;
    }

    public void Return(T item)
    {
        if (item == null)
            return;

        if (!activeObjects.Remove(item))
            return;

        if (item is IPoolable poolable)
            poolable.OnReturnToPool();

        item.gameObject.SetActive(false);

        if (instanceToPrefabMap.TryGetValue(item, out T prefab))
        {
            if (!availableObjectsByPrefab.ContainsKey(prefab))
                availableObjectsByPrefab[prefab] = new Queue<T>();

            availableObjectsByPrefab[prefab].Enqueue(item);
        }
    }

    public void Clear()
    {
        foreach (var queue in availableObjectsByPrefab.Values)
        {
            foreach (var obj in queue)
            {
                if (obj != null)
                    Object.Destroy(obj.gameObject);
            }
            queue.Clear();
        }
        availableObjectsByPrefab.Clear();

        foreach (var obj in activeObjects)
        {
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }
        activeObjects.Clear();
        instanceToPrefabMap.Clear();
    }
}

