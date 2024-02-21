using System.Collections.Generic;
using Kurisu.GOAP.Resolver;
namespace Kurisu.GOAP
{
    /// <summary>
    /// interface GOAP.IAction
    /// Basic interface for all GOAPActions
    /// </summary>
    public interface IAction : INode
    {
        float GetCost();
        void Init(WorldState worldState);
        /// <summary>
        /// Returns true if effects are a superset for conditions
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        bool SatisfiesConditions(IDictionary<string, bool> conditions);
        /// <summary>
        /// True if worldState is a superset of preconditions
        /// </summary>
        /// <param name="worldState"></param>
        /// <returns></returns>
        bool PreconditionsSatisfied(IStateCollection worldState);
        /// <summary>
        /// Called every frame by GOAPPlanner
        /// </summary>
        void OnTick();
        /// <summary>
        /// Called when selected by GOAPPlanner
        /// </summary>
        void OnActivate();
        /// <summary>
        /// Called by GOAPPlanner when action effects achieved or plan cancelled
        /// </summary>
        void OnDeactivate();
        public Dictionary<string, bool> Preconditions { get; }
        // What will be in worldState when action completed
        public Dictionary<string, bool> Effects { get; }
        public string Name { get; }

    }
}