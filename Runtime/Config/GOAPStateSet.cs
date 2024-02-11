using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.GOAP
{
    /// <summary>
    /// GOAP.StateSet 
    /// A series of string,value dictionaries to describe a state
    /// </summary>
    [CreateAssetMenu(fileName = "GOAPStateSet", menuName = "AkiGOAP/GOAPStateSet")]
    public class GOAPStateSet : ScriptableObject
    {
        private Dictionary<string, bool> states;
        [SerializeField, Tooltip("Absent key in boolStates treated the same as key = false")]
        internal bool defaultFalse = true;
        internal void Init()
        {
            states = new Dictionary<string, bool>();
        }
        public virtual void AddState(string name, bool value)
        {
            states[name] = value;
        }

        public void RemoveState(string name)
        {
            states.Remove(name);
        }

        public bool GetState(string name)
        {
            if (defaultFalse && !states.ContainsKey(name))
            {
                return false;
            }
            return states[name];
        }
        public bool InStates(string name)
        {
            return states.ContainsKey(name);
        }
        public bool InSet(string name, bool value)
        {
            if (!InStates(name))
            {
                return defaultFalse && value == false;
            }
            if (states[name] != value)
            {
                return false;
            }
            return true;
        }
        public StateCache GetCache()
        {
            return StateCache.Get(states);
        }
        public IReadOnlyDictionary<string, bool> GetStates()
        {
            return states;
        }
#if UNITY_EDITOR
        /// <summary>
        /// In editor, we use OnValidate to init dictionary.
        /// If you want to serialize the value, try implement 'ISerializationCallbackReceiver'.
        /// </summary>
        private void OnValidate()
        {
            Init();
        }
#endif
        /// <summary>
        /// Awake of ScriptableObject will be called when the file is unsearialized at first, useful in build game
        /// </summary>
        private void Awake()
        {
            Init();
        }
    }
}
