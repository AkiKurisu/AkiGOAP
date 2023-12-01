using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.GOAP
{
    [CreateAssetMenu(fileName = "GOAPSet", menuName = "AkiGOAP/GOAPSet")]
    public class GOAPSet : ScriptableObject, IGOAPSet
    {
        [Serializable]
        private class GoalInternalSet
        {
            [SerializeReference]
            internal List<IGoal> goals = new();
            internal GoalInternalSet(List<GOAPBehavior> behaviors)
            {
                foreach (var behavior in behaviors)
                {
                    if (behavior is IGoal) goals.Add(behavior as IGoal);
                }
            }
        }
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

        public List<IGoal> GetGoals()
        {
            return JsonUtility.FromJson<GoalInternalSet>(JsonUtility.ToJson(new GoalInternalSet(behaviors))).goals;
        }
        public List<IAction> GetActions()
        {
            return JsonUtility.FromJson<ActionInternalSet>(JsonUtility.ToJson(new ActionInternalSet(behaviors))).actions;
        }
    }
}
