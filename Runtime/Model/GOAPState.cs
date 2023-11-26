using System;
using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public readonly struct GOAPState : IEquatable<GOAPState>
    {
        /// <summary>
        /// Generated state unique key
        /// </summary>
        /// <value></value>
        public readonly string UniqueID { get; }
        public readonly string Key { get; }
        public readonly bool Value { get; }
        private static readonly string On = "_on";
        private static readonly string Off = "_off";
        public GOAPState(KeyValuePair<string, bool> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
            UniqueID = $"{pair.Key}{(Value ? On : Off)}";
        }

        public bool Equals(GOAPState other)
        {
            return UniqueID == other.UniqueID;
        }
    }
}
