using System;
using UnityEngine;
namespace Kurisu.GOAP
{
    [Serializable]
    public class GOAPBehavior
    {
#if UNITY_EDITOR
        [HideInInspector]
        internal string description = string.Empty;
        [SerializeField, HideInInspector]
        private string guid;
        internal string GUID { get => guid; set => guid = value; }
#endif
    }
}
