using System;
using System.Collections.Generic;
namespace Kurisu.GOAP
{
    internal class ObjectPool<T>
    {
        private const int PoolCapacity = 10;
        private readonly Func<T> instanceFunc;
        public ObjectPool(Func<T> instanceFunc)
        {
            this.instanceFunc = instanceFunc;
            poolQueue = new(PoolCapacity);
        }
        public ObjectPool(Func<T> instanceFunc, int capacity)
        {
            this.instanceFunc = instanceFunc;
            poolQueue = new(capacity);
        }
        internal readonly Queue<T> poolQueue;
        public void Push(T obj)
        {
            if (obj != null)
                poolQueue.Enqueue(obj);
        }
        public T Get()
        {
            if (!poolQueue.TryDequeue(out T result))
            {
                result = instanceFunc();
            }
            return result;
        }
    }
}
