using System;
using System.Collections.Generic;
using System.Reflection;

namespace Domain.Pool
{
    internal static class CollectionPool<T> where T : class, new()
    {
        private static readonly Queue<T> pool = new Queue<T>();
        private static readonly Func<T> factory;
        private static readonly Action<T> clearAction;

        static CollectionPool()
        {
            factory = () => new T();
            
            var clearMethod = typeof(T).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (clearMethod != null)
            {
                clearAction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), clearMethod);
            }
            else
            {
                clearAction = null;
            }
        }

        public static T Get()
        {
            if (pool.Count > 0)
            {
                var item = pool.Dequeue();
                clearAction?.Invoke(item);
                return item;
            }
            return factory();
        }

        public static void Release(T item)
        {
            if (item == null) return;
            
            clearAction?.Invoke(item);
            pool.Enqueue(item);
        }
    }
}

