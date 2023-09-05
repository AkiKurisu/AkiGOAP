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
        public string Key;
        public bool Value;
        private static readonly string On = "_on";
        private static readonly string Off = "_off";
        public GOAPState(KeyValuePair<string, bool> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
            UniqueID = $"{pair.Key}{(Value ? On : Off)}";
        }
    }
}
