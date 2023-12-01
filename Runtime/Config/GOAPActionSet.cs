using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.GOAP
{
    [CreateAssetMenu(fileName = "GOAPActionSet", menuName = "AkiGOAP/GOAPActionSet")]
    public class GOAPActionSet : ScriptableObject, IGOAPSet
    {
        [Serializable]
        private class ActionInternalSet
        {
            [SerializeReference]
            internal List<IAction> actions = new();
            internal ActionInternalSet(List<GOAPBehavior> behaviors)
            {
                foreach (var behavior in behaviors)
                {
                    if (behavior is IAction) actions.Add(behavior as IAction);
                }
            }
        }
        [SerializeReference]
        private List<GOAPBehavior> behaviors = new();
#if UNITY_EDITOR
        [Multiline(6)]
        public string Description;
#endif
        public List<GOAPBehavior> Behaviors => behaviors;
        public UnityEngine.Object Object => this;

        public List<IAction> GetActions()
        {
            return JsonUtility.FromJson<ActionInternalSet>(JsonUtility.ToJson(new ActionInternalSet(behaviors))).actions;
        }
    }
}
