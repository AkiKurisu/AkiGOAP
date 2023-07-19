using System.Collections.Generic;
using System.Linq;
using Kurisu.GOAP.Resolver;
namespace Kurisu.GOAP
{
    /// <summary>
    /// A goal that can be achieved via a sequence of GOAPActions.
    //  Defined in terms of a priority, conditions required to achieve the goal,
    //  preconditions required to attempt the goal, and an actionLayer that determines
    //  what layer of GOAPActions will be considered for valid action plans.
    /// </summary>
    public abstract class GOAPGoal : GOAPBehavior,IGoal
    {
        protected GOAPWorldState worldState;    
        // What must be in worldState for the goal to be complete
        public Dictionary<string, bool> conditions{get; protected set;}
        // What must be in worldState for the goal to be considered
        public Dictionary<string, bool> preconditions{get; protected set;}
        GOAPState[] INode.Effects => null;
        public GOAPState[] Conditions {get;private set;}
        public virtual string Name=>GetType().Name;
        void IGoal.Init(GOAPWorldState worldState)
        {
            conditions = new Dictionary<string, bool>();
            preconditions = new Dictionary<string, bool>();
            this.worldState = worldState;
            SetupDerived();
            Conditions=conditions.Select(x=>new GOAPState(x)).ToArray();
        }
        /// <summary>
        /// Set the complete condition of this goal
        /// </summary>
        protected virtual void SetupDerived(){}
        public virtual float GetPriority(){
            return 0f;
        }

        public virtual bool PreconditionsSatisfied(GOAPWorldState worldState){
            // Will return true if preconditions are empty
            return worldState.IsSubset(preconditions);
        }

        public virtual bool ConditionsSatisfied(GOAPWorldState worldState){
            return worldState.IsSubset(conditions);
        }
        /// <summary>
        /// Called every frame by GOAPPlanner
        /// </summary>
        public virtual void OnTick(){}
        /// <summary>
        /// Called when selected by GOAPPlanner
        /// </summary>
        public virtual void OnActivate(){}
        /// <summary>
        /// Called by GOAPPlanner when goal achieved or plan cancelled
        /// </summary>
        public virtual void OnDeactivate(){}
    }
}