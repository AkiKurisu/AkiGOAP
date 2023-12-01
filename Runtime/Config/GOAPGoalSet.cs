using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.GOAP
{
    [CreateAssetMenu(fileName = "GOAPGoalSet", menuName = "AkiGOAP/GOAPGoalSet")]
    public class GOAPGoalSet : ScriptableObject, IGOAPSet
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
    }
}
