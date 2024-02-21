using System.Collections;
using System.Collections.Generic;
namespace Kurisu.GOAP
{
    /// <summary>
    /// State cache for path-finding
    /// </summary>
    public class StateCache : IStateCollection, IEnumerable<KeyValuePair<string, bool>>
    {
        private bool isPooled;
        private static readonly ObjectPool<StateCache> pool = new(() => new(), 5);
        private readonly Dictionary<string, bool> states = new(10);
        public void Clear()
        {
            states.Clear();
        }
        /// <summary>
        /// Delete intersection states from effects
        /// </summary>
        /// <param name="effects"></param>
        /// <returns></returns>
        public StateCache DeleteIntersection(IEnumerable<KeyValuePair<string, bool>> effects)
        {
            foreach (var state in effects)
            {
                if (states.TryGetValue(state.Key, out bool value) && value == state.Value)
                    states.Remove(state.Key);
            }
            return this;
        }
        /// <summary>
        /// Join new states
        /// </summary>
        /// <param name="otherStates"></param>
        /// <returns></returns>
        public bool TryJoin(IEnumerable<KeyValuePair<string, bool>> otherStates)
        {
            foreach (var state in otherStates)
            {
                if (states.TryGetValue(state.Key, out bool value) && value != state.Value)
                {
                    return false;
                }
                states[state.Key] = state.Value;
            }
            return true;
        }
        public static StateCache Copy(StateCache oldCache)
        {
            return Get(oldCache.states);
        }
        public static StateCache Get(IEnumerable<KeyValuePair<string, bool>> states)
        {
            var cache = pool.Get();
            cache.Clear();
            foreach (var state in states)
            {
                cache.states[state.Key] = state.Value;
            }
            cache.isPooled = false;
            return cache;
        }
        public bool IsSubset(IEnumerable<KeyValuePair<string, bool>> state)
        {
            foreach (var i in state)
            {
                if (!InSet(i.Key, i.Value))
                {
                    return false;
                }
            }
            return true;
        }
        public bool InStates(string name)
        {
            return states.ContainsKey(name);
        }
        public bool InSet(string name, bool value)
        {
            if (!InStates(name))
            {
                return value == false;
            }
            if (states[name] != value)
            {
                return false;
            }
            return true;
        }
        public void Pooled()
        {
            if (isPooled) return;
            isPooled = true;
            pool.Push(this);
        }
        public Dictionary<string, bool> ToDictionary()
        {
            return states;
        }

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator()
        {
            return states.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return states.GetEnumerator();
        }
    }
}