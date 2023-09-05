using System;
using System.Collections.Generic;
using Kurisu.GOAP.Resolver;
using UnityEngine;
namespace Kurisu.GOAP
{
    /// <summary>
    //  The state of the world represented as a local StateSet and a global StateSet.
    //  The local StateSet is specific to the GameObject, whereas the global StateSet
    //  can be shared between GameObjects.
    /// </summary>
    public class GOAPWorldState : MonoBehaviour
    {
        //Attached and specific to the GameObject 
        private GOAPStateSet localState;
        public GOAPStateSet LocalState => localState;
        [SerializeField, Tooltip("Absent key in boolStates treated the same as key = false")]
        private bool defaultFalse = true;
        /// <summary>
        /// Shared between objects.
        /// All states assumed to have g_ prefix
        /// </summary>
        [SerializeField]
        private GOAPStateSet globalState;
        public GOAPStateSet GlobalState => globalState;
        private readonly Dictionary<INode, Transform> nodeTargets = new();
#if UNITY_EDITOR
        //Editor hook for UIElement update
        internal Action OnUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }
#endif
        protected void Awake()
        {
            localState = ScriptableObject.CreateInstance<GOAPStateSet>();
            localState.defaultFalse = defaultFalse;
            //Since we create instance manually, we need to init it
            localState.Init();
        }
        public bool IsSubset(Dictionary<string, bool> state)
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
        public void RegisterNodeTarget(INode node, Transform target)
        {
            nodeTargets[node] = target;
        }
        public Transform ResolveNodeTarget(INode node)
        {
            if (!nodeTargets.TryGetValue(node, out Transform target))
            {
                return null;
            }
            return target;
        }

        public void SetState(string name, bool value, bool global = false)
        {
            if (global)
            {
                globalState.AddState(name, value);
            }
            else
            {
                localState.AddState(name, value);
            }
        }

        public void RemoveState(string name, bool includeGlobal = true)
        {
            if (includeGlobal && globalState != null && globalState.InStates(name))
            {
                globalState.RemoveState(name);
            }
            else
            {
                localState.RemoveState(name);
            }
        }

        public bool GetState(string name, bool includeGlobal = true)
        {
            if (includeGlobal && globalState != null && globalState.InStates(name))
            {
                return globalState.GetState(name);
            }
            return localState.GetState(name);
        }

        public bool InSet(string name, bool value, bool includeGlobal = true)
        {
            if (includeGlobal && globalState != null && globalState.InStates(name))
            {
                return globalState.InSet(name, value);
            }
            return localState.InSet(name, value);
        }
    }
}