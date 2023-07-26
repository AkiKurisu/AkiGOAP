using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IPlanner : IGOAPSet
    {
        void InjectGoals(IEnumerable<IGoal> source);
        void InjectActions(IEnumerable<IAction> source);
        IGoal ActivateGoal { get; }
        List<IAction> ActivatePlan { get; }
        List<IAction> GetAllActions();
        event System.Action<IPlanner> OnUpdatePlanEvent;
        GOAPWorldState WorldState { get; }
        int activeActionIndex { get; }
        List<GoalData> GetSortedGoalData();
        void TickGoals();
        void ManualActivate();
    }
}
