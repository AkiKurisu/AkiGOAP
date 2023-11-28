using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;
namespace Kurisu.GOAP.Example
{
    [Serializable]
    public class ExampleAgent
    {
        public ExampleAgent(GOAPSet dataSet)
        {
            this.dataSet = dataSet;
        }
        public Transform Transform { get; set; }
        public NavMeshAgent NavMeshAgent { get; private set; }
        private readonly GOAPSet dataSet;
        public Transform Player { get; internal set; }
        public Transform Home { get; internal set; }
        public Transform Tent { get; internal set; }
        public int Energy = 100;
        public void Init()
        {
            //Inject dependency
            NavMeshAgent = Transform.GetComponent<NavMeshAgent>();
            var goals = dataSet.GetGoals();
            foreach (var goal in goals.OfType<ExampleGoal>())
            {
                goal.Inject(this);
            }
            var actions = dataSet.GetActions();
            foreach (var action in actions.OfType<ExampleAction>())
            {
                action.Inject(this);
            }
            var planner = Transform.GetComponent<IPlanner>();
            planner.InjectGoals(goals);
            planner.InjectActions(actions);
        }
        public void LossEnergy()
        {
            Energy = Mathf.Clamp(Energy - 1, 0, 100);
        }
    }
}
