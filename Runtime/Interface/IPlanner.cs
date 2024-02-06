using System;
using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IPlanner : IGOAPSet
    {
        IGoal ActivateGoal { get; }
        List<IAction> ActivatePlan { get; }
        event Action<IPlanner> OnUpdate;
        event Action<IPlanner> OnReload;
        WorldState WorldState { get; }
        int ActiveActionIndex { get; }
        /// <summary>
        /// Get internal backend
        /// </summary>
        /// <value></value>
        IBackend Backend { get; }
        /// <summary>
        /// Get a list of current goal info
        /// </summary>
        /// <returns></returns>
        List<GoalData> GetSortedGoalData();
        void TickGoals();
        /// <summary>
        /// Activate planner when plan is complete while ManualActivatePlanner is on
        /// </summary>
        void ManualActivate();
        /// <summary>
        /// Abort current running plan to force a replan next update
        /// </summary>
        void AbortActivePlan();
        /// <summary>
        /// Clear all goals and actions for preparing new data injection
        /// </summary>
        void CleanUp();
        void SetGoals(IEnumerable<IGoal> source);
        void SetActions(IEnumerable<IAction> source);
        void SetGoalsAndActions(IEnumerable<IGoal> goals, IEnumerable<IAction> actions);
    }
}
