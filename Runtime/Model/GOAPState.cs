using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public class GOAPState
    {
        /// <summary>
        /// Generated state unique key
        /// </summary>
        /// <value></value>
        public string UniqueID { get; private set; }
        public string Key { get; private set; }
        public bool Value { get; private set; }
        private static readonly string On = "_on";
        private static readonly string Off = "_off";
        private static readonly ObjectPool<GOAPState> pool = new(() => new());
        public static GOAPState Get(KeyValuePair<string, bool> pair)
        {
            var state = pool.Get();
            state.Key = pair.Key;
            state.Value = pair.Value;
            state.UniqueID = $"{pair.Key}{(pair.Value ? On : Off)}";
            return state;
        }
        public void Pooled()
        {
            pool.Push(this);
        }
    }
}
