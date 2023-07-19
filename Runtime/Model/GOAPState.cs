using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public class GOAPState
    {
        /// <summary>
        /// Generated state unique key
        /// </summary>
        /// <value></value>
        public string UniqueID{get;private set;}
        public string Key;
        public bool Value;
        private static string On="_on";
        private static string Off="_off";
        public GOAPState(KeyValuePair<string,bool> pair)
        {
            this.Key=pair.Key;
            this.Value=pair.Value;
            this.UniqueID=$"{pair.Key}{(this.Value?On:Off)}";
        }
    }
}
