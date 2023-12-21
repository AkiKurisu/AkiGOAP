using System;
using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IPlanner : IGOAPSet
    {
        void InjectGoals(IEnumerable<IGoal> source);
        void InjectActions(IEnumerable<IAction> source);
        IGoal ActivateGoal { get; }
        List<IAction> ActivatePlan { get; }
        event Action<IPlanner> OnUpdate;
        event Action<IPlanner> OnReload;
        WorldState WorldState { get; }
        int ActiveActionIndex { get; }
        List<GoalData> GetSortedGoalData();
        void TickGoals();
        void ManualActivate();
        IBackend Backend { get; }
    }
}
