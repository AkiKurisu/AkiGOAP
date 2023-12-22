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
        public Dictionary<string, bool> Conditions { get; protected set; } = new Dictionary<string, bool>();
        // What must be in worldState for the goal to be considered
        public Dictionary<string, bool> Preconditions { get; protected set; } = new Dictionary<string, bool>();
        GOAPState[] INode.EffectStates => null;
        public GOAPState[] ConditionStates { get; private set; }
        void IGoal.Init(WorldState worldState)
        {
            this.worldState = worldState;
            SetupDerived();
            ConditionStates = Conditions.Select(x => GOAPState.Get(x)).ToArray();
        }
        /// <summary>
        /// Set the complete condition of this goal
        /// </summary>
        protected virtual void SetupDerived() { }
#if UNITY_EDITOR
        internal bool IsSelected { get; set; }
        internal bool IsBanned { get; set; }
#endif
        public float GetPriority()
        {
#if UNITY_EDITOR
            //Get priority in editor, can be jumped to highest in graph editor
            if (IsSelected) return float.MaxValue;
#endif
            return SetupPriority();
        }
        /// <summary>
        /// Set static or dynamic priority of this goal
        /// </summary>
        /// <returns></returns>
        protected virtual float SetupPriority()
        {
            return 0f;
        }

        public virtual bool PreconditionsSatisfied(WorldState worldState)
        {
#if UNITY_EDITOR
            if (IsBanned)
            {
                return false;
            }
#endif
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