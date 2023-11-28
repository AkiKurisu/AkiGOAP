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
    public abstract class GOAPGoal : GOAPBehavior, IGoal
    {
        protected WorldState worldState;
        // What must be in worldState for the goal to be complete
        public Dictionary<string, bool> Conditions { get; protected set; }
        // What must be in worldState for the goal to be considered
        public Dictionary<string, bool> Preconditions { get; protected set; }
        GOAPState[] INode.EffectStates => null;
        public GOAPState[] ConditionStates { get; private set; }
        public virtual string Name => GetType().Name;
        void IGoal.Init(WorldState worldState)
        {
            Conditions = new Dictionary<string, bool>();
            Preconditions = new Dictionary<string, bool>();
            this.worldState = worldState;
            SetupDerived();
            ConditionStates = Conditions.Select(x => new GOAPState(x)).ToArray();
        }
        /// <summary>
        /// Set the complete condition of this goal
        /// </summary>
        protected virtual void SetupDerived() { }
        public virtual float GetPriority()
        {
            return 0f;
        }

        public virtual bool PreconditionsSatisfied(WorldState worldState)
        {
            // Will return true if preconditions are empty
            return worldState.IsSubset(Preconditions);
        }

        public virtual bool ConditionsSatisfied(WorldState worldState)
        {
            return worldState.IsSubset(Conditions);
        }
        /// <summary>
        /// Called every frame by GOAPPlanner
        /// </summary>
        public virtual void OnTick() { }
        /// <summary>
        /// Called when selected by GOAPPlanner
        /// </summary>
        public virtual void OnActivate() { }
        /// <summary>
        /// Called by GOAPPlanner when goal achieved or plan cancelled
        /// </summary>
        public virtual void OnDeactivate() { }
    }
}