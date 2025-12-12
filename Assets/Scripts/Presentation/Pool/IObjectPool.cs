using UnityEngine;

namespace Presentation.Pool
{
    public interface IObjectPool<T> where T : Component
    {
        T Get(T prefab);
        void Return(T item);
        void Clear();
        int AvailableCount { get; }
        int ActiveCount { get; }
    }
}
