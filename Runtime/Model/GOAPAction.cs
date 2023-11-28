using System.Collections.Generic;
using System.Linq;
namespace Kurisu.GOAP
{
    /// <summary>
    /// A behavior that requires preconditions to run and has known 
    /// effects upon completion.
    /// </summary>
    public abstract class GOAPAction : GOAPBehavior, IAction
    {
        protected WorldState worldState;
        // All of these states are removed from worldState when OnDeactivate is called
        private Dictionary<string, bool> temporaryState;

        // What must be in worldState for the action to run
        public Dictionary<string, bool> Preconditions { get; protected set; } = new Dictionary<string, bool>();
        // What will be in worldState when action completed
        public Dictionary<string, bool> Effects { get; protected set; } = new Dictionary<string, bool>();

        // Absent key treated the same as key = false in preconditions and effects
        protected virtual bool DefaultFalse => true;
        public virtual string Name => GetType().Name;
        private GOAPState[] _effects;
        public GOAPState[] EffectStates
        {
            get
            {
                if (DynamicSetEffect)
                {
                    Effects.Clear();
                    SetupEffects();
                    _effects = Effects.Select(x => new GOAPState(x)).ToArray();
                }
                return _effects;
            }
            set
            {
                _effects = value;
            }
        }
        public GOAPState[] ConditionStates { get; private set; }
        /// <summary>
        /// Whether effect of this action can be set dynamically at runtime
        /// </summary>
        protected virtual bool DynamicSetEffect => false;
        void IAction.Init(WorldState worldState)
        {
            this.worldState = worldState;
            SetupDerived();
            SetupEffects();
            if (!DynamicSetEffect) EffectStates = Effects.Select(x => new GOAPState(x)).ToArray();
            ConditionStates = Preconditions.Select(x => new GOAPState(x)).ToArray();
            ResetTemporaryState();
        }

        private void ResetTemporaryState()
        {
            temporaryState = new Dictionary<string, bool>();
        }

        public virtual float GetCost()
        {
            return 0f;
        }
        /// <summary>
        /// Returns true if effects are a superset for conditions
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool SatisfiesConditions(Dictionary<string, bool> conditions)
        {
            if (DynamicSetEffect)
            {
                Effects.Clear();
                SetupEffects();
            }
            foreach (var i in conditions)
            {
                //If condition not in the effects
                if (!Effects.ContainsKey(i.Key))
                {
                    if (DefaultFalse && i.Value == false) continue;
                    else return false;
                }
                if (Effects[i.Key] != i.Value)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Called when selected by GOAPPlanner
        /// </summary>
        public void OnActivate()
        {
            OnActivateDerived();
        }

        protected virtual void OnActivateDerived() { }
        /// <summary>
        /// Called by GOAPPlanner when action effects achieved or plan cancelled
        /// </summary>
        public void OnDeactivate()
        {
            OnDeactivateDerived();
            ClearTemporaryStates();
        }

        protected virtual void OnDeactivateDerived() { }
        /// <summary>
        /// Called every frame by GOAPPlanner
        /// </summary>
        public virtual void OnTick() { }
        /// <summary>
        /// True if worldState is a superset of preconditions
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        public bool PreconditionsSatisfied(WorldState worldState)
        {
            return worldState.IsSubset(Preconditions);
        }
        /// <summary>
        /// Effects can Setup at runtime to decrease actions since you can make brunch actions into one using dynamic effects setting
        /// </summary>
        protected virtual void SetupEffects() { }
        /// <summary>
        /// Transform target and conditions can SetUp at runtime
        /// </summary>
        protected virtual void SetupDerived() { }
        protected void AddTemporaryState(string name, bool val)
        {
            worldState.SetState(name, val);
            temporaryState[name] = val;
        }
        protected void ClearTemporaryStates()
        {
            foreach (var i in temporaryState)
            {
                worldState.RemoveState(i.Key);
            }
            ResetTemporaryState();
        }
    }
}
