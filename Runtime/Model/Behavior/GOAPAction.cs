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
        // What must be in worldState for the action to run
        public Dictionary<string, bool> Preconditions { get; protected set; } = new Dictionary<string, bool>();
        // What will be in worldState when action completed
        public Dictionary<string, bool> Effects { get; protected set; } = new Dictionary<string, bool>();
        private GOAPState[] _effects;
        public GOAPState[] EffectStates
        {
            get
            {
                if (DynamicSetEffect)
                {
                    if (_effects != null)
                    {
                        foreach (var effect in _effects)
                        {
                            effect.Pooled();
                        }
                    }
                    Effects.Clear();
                    SetupEffects();
                    _effects = Effects.Select(x => GOAPState.Get(x)).ToArray();
                }
                return _effects;
            }
        }
        private GOAPState[] conditionStates;
        public GOAPState[] ConditionStates
        {
            get
            {
#if UNITY_EDITOR
                if (IsSelected) return null;
#endif
                return conditionStates;
            }
        }
        /// <summary>
        /// Whether effect of this action can be set dynamically at runtime
        /// </summary>
        protected virtual bool DynamicSetEffect => false;
        void IAction.Init(WorldState worldState)
        {
            this.worldState = worldState;
            SetupDerived();
            SetupEffects();
            if (!DynamicSetEffect) _effects = Effects.Select(x => GOAPState.Get(x)).ToArray();
            conditionStates = Preconditions.Select(x => GOAPState.Get(x)).ToArray();
        }


        public virtual float GetCost()
        {
            return 0f;
        }
        public bool SatisfiesConditions(IDictionary<string, bool> conditions)
        {
            if (DynamicSetEffect)
            {
                Effects.Clear();
                SetupEffects();
            }
            int satisfyCounter = 0;
            foreach (var i in conditions)
            {
                //To find a entry point, we only need to satisfy one condition
                if (!Effects.TryGetValue(i.Key, out bool effectValue))
                {
                    continue;
                }
                if (effectValue != i.Value)
                {
                    return false;
                }
                else
                {
                    ++satisfyCounter;
                }
            }
            if (satisfyCounter == 0) return false;
            //Should check the precondition will not create status collision
            //There are some possible results
            //Goal needs precondition: [a:1,b:1,c:0]
            //Result 1:
            //Node A meet [a:1], also requires [b:1]
            //So requirement now becomes [b:1,c:0]
            //Result 2:
            //Node A meet [a:1], but requires [c:1]
            //So requirement now becomes [b:1,c:0,c:1], which is not possible
            //So it means we meet a collision
            //Only if Node A fulfill c:0 can it add new requirement
            foreach (var preCondition in Preconditions)
            {
                if (conditions.TryGetValue(preCondition.Key, out bool value) && preCondition.Value != value)
                {
                    //Check this condition is fulfilled
                    if (!Effects.TryGetValue(preCondition.Key, out bool effect) || effect != value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public void OnActivate()
        {
            OnActivateDerived();
        }

        protected virtual void OnActivateDerived() { }
        public void OnDeactivate()
        {
            OnDeactivateDerived();
        }

        protected virtual void OnDeactivateDerived() { }
        public virtual void OnTick() { }
#if UNITY_EDITOR
        /// <summary>
        /// Set to enable action always satisfy preconditions
        /// </summary>
        /// <value></value>
        internal bool IsSelected { get; set; }
#endif
        public bool PreconditionsSatisfied(IStateCollection worldState)
        {
#if UNITY_EDITOR
            //Always satisfied when being selected
            if (IsSelected) return true;
#endif
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
    }
}
