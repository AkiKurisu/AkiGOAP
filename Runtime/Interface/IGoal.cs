using System.Collections.Generic;
using Kurisu.GOAP.Resolver;
namespace Kurisu.GOAP
{
    /// <summary>
    /// Basic interface for all GOAPGoals
    /// </summary>
    public interface IGoal : INode
    {
        /// <summary>
        /// Set static or dynamic priority of this goal
        /// </summary>
        /// <returns></returns>
        float GetPriority();
        void Init(WorldState worldState);
        bool ConditionsSatisfied(WorldState worldState);
        bool PreconditionsSatisfied(WorldState worldState);
        /// <summary>
        /// Goal OnTick for collecting data or sensoring state or sending event
        /// </summary>
        void OnTick();
        void OnActivate();
        void OnDeactivate();
        Dictionary<string, bool> Conditions { get; }

        // What must be in worldState for the goal to be considered
        Dictionary<string, bool> Preconditions { get; }
        string Name { get; }
    }
}