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
        bool SatisfiesConditions(Dictionary<string, bool> conditions);
        bool PreconditionsSatisfied(WorldState worldState);
        void OnTick();
        void OnActivate();
        void OnDeactivate();
        public Dictionary<string, bool> Preconditions { get; }
        // What will be in worldState when action completed
        public Dictionary<string, bool> Effects { get; }
        public string Name { get; }

    }
}