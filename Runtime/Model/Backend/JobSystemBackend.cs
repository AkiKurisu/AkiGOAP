using Kurisu.GOAP.Runner;
using Kurisu.GOAP.Resolver;
using Unity.Burst;
using System.Collections.Generic;
using System.Linq;
namespace Kurisu.GOAP
{
    /// <summary>
    /// Execution on multi-thread, fast but may need more care
    /// Dynamic effect will cause allocation which may decrease performance
    /// </summary>
    public class JobSystemBackend : BaseBackend
    {
        [BurstCompile]
        private struct GoalSorter : IComparer<IGoal>
        {
            public readonly int Compare(IGoal x, IGoal y)
            {
                return y.GetPriority().CompareTo(x.GetPriority());
            }
        }
        private GOAPJobRunner jobRunner;
        private bool activateFlag = false;
        private bool isDirty = false;
        private IAction candidateAction;
        private IGoal candidateGoal;
        private readonly List<IAction> candidatePlan = new(Capacity);
        private readonly List<IGoal> candidateGoals = new(Capacity);
        public List<IGoal> CandidateGoals => candidateGoals;
        private IAction activateAction;
        public IAction ActivateAction => activateAction;
        public override int ActiveActionIndex => 0;
        public override List<IAction> ActivatePlan { get; } = new(Capacity);
        public JobSystemBackend(IBackendHost backendHost) : base(backendHost) { }
        public override void ManualActivate()
        {
            //Ensure jobRunner activate in Update()
            activateFlag = true;
        }
        protected override void OnActionInjected()
        {
            isDirty = true;
        }
        protected override void OnGoalInjected()
        {
            isDirty = true;
        }
        public override void Update()
        {
            if (activateFlag)
            {
                //Deferred Activation
                IsActive = true;
                activateFlag = false;
            }
            if (isDirty)
            {
                //Deferred Job Remake
                isDirty = false;
                jobRunner?.Dispose();
                jobRunner = new GOAPJobRunner(this, new GraphResolver(Actions.Cast<INode>().Concat(Goals)));
            }
            if (IsActive) Tick();
        }
        private void Tick()
        {
            if (!TickType.HasFlag(TickType.ManualUpdateGoal)) TickGoals();
            OnTickActivePlan();
            GetHighestPriorityGoals(candidateGoals);
            jobRunner?.Run();
        }
        public override void OnDisable()
        {
            jobRunner?.Complete();
        }
        public override void LateUpdate()
        {
            if (!IsActive) return;
            //Ensure job is completed
            jobRunner?.Complete();
            bool notifyHost = false;
            if ((NoActiveGoal() && CandidateGoalAvailable()) || BetterGoalAvailable())
            {
                StartCurrentBestGoal();
                notifyHost = true;
            }
            else if (HaveNextAction())
            {
                StartCurrentBestAction();
                notifyHost = true;
            }
            if (notifyHost)
                NotifyHostUpdate();
            else
            {
                if (TickType.HasFlag(TickType.ManualActivatePlanner))
                {
                    IsActive = false;
                    if (LogSearch) PlannerLog("Manual plan updating ends, need to be activated manually again.", bold: true);
                }
            }
        }
        /// <summary>
        ///  Updates chosenGoal and chosenPlan with the highest priority goal that 
        ///  has a valid plan
        /// </summary>
        /// <param name="chosenGoal"></param>
        private void GetHighestPriorityGoals(List<IGoal> chosenGoals)
        {
            chosenGoals.Clear();
            //Searching for highest priority goal
            if (Goals == null || Goals.Count == 0)
            {
                if (LogFail) PlannerLog("No goals found");
                return;
            }
            for (int i = 0; i < Goals.Count; i++)
            {
                if (!Goals[i].PreconditionsSatisfied(WorldState))
                {
                    if (LogFail) PlannerLog($"{Goals[i].Name} not valid as preconditions not satisfied");
                    continue;
                }
                if (LogSearch) PlannerLog($"Add candidate goal {Goals[i].Name}", bold: true);
                chosenGoals.Add(Goals[i]);
            }
            chosenGoals.Sort(new GoalSorter());
        }
        internal void SetCandidate(List<IAction> path, IGoal goal)
        {
            if (goal == null || path.Count == 0)
            {
                candidatePlan.Clear();
                candidateAction = null;
                candidateGoal = null;
                if (LogFail) PlannerLog("No candidate goal or path was found.");
                return;
            }
            var action = path[0];
            candidatePlan.Clear();
            candidatePlan.AddRange(path);
            if (candidateAction != action && LogSearch) PlannerLog($"Set candidate action:{action.Name}");
            candidateAction = action;
            if (candidateGoal != goal && LogSearch) PlannerLog($"Set candidate goal:{goal.Name}");
            candidateGoal = goal;
        }

        private void StartCurrentBestGoal()
        {
            //Activate Goal
            ActivateGoal?.OnDeactivate();
            ActivateGoal = candidateGoal;
            if (LogActive) ActivePlanLog($"Starting new plan for {ActivateGoal.Name}", bold: true);
            ActivateGoal.OnActivate();
            //Activate Action
            StartCurrentBestAction();
        }
        private void StartCurrentBestAction()
        {
            ActivateAction?.OnDeactivate();
            SetCurrentAction(candidateAction);
            if (LogActive) ActivePlanLog($"Starting {ActivateAction.Name}");
            ActivateAction.OnActivate();
        }
        private void OnTickActivePlan()
        {
            // Nothing to run
            if (ActivateGoal == null || ActivateAction == null) { return; }
            // Goal no longer viable
            if (!ActivateGoal.PreconditionsSatisfied(WorldState))
            {
                if (LogActive) ActivePlanLog(
                    $"{ActivateGoal.Name} failed as preconditions are no longer satisfied",
                    bold: true
                );
                OnCompleteOrFailActivePlan();
                return;
            }

            // Plan no longer viable
            if (!ActivateAction.PreconditionsSatisfied(WorldState))
            {
                if (LogActive) ActivePlanLog(
                    $"{ActivateAction.Name} failed as preconditions are no longer satisfied",
                    bold: true
                    );
                OnCompleteOrFailActivePlan();
                return;
            }
            ActivateAction.OnTick();
            // Goal complete
            if (ActivateGoal.ConditionsSatisfied(WorldState))
            {
                if (LogActive) ActivePlanLog($"{ActivateGoal.Name} completed", bold: true);
                OnCompleteOrFailActivePlan();
                return;
            }
        }
        private bool CandidateGoalAvailable()
        {
            return candidateAction != null && candidateGoal != null;
        }
        private bool HaveNextAction()
        {
            return candidateAction != null && candidateAction != ActivateAction;
        }
        private bool BetterGoalAvailable()
        {
            return candidateGoal != null && candidateGoal != ActivateGoal;
        }
        private void OnCompleteOrFailActivePlan()
        {
            ActivateAction?.OnDeactivate();
            ActivateGoal?.OnDeactivate();
            bool needNotify = ActivateGoal != null;
            ActivateGoal = null;
            SetCurrentAction(null);
            if (needNotify) NotifyHostUpdate();
        }
        private void SetCurrentAction(IAction action)
        {
            ActivatePlan.Clear();
            activateAction = action;
            if (activateAction != null)
                ActivatePlan.AddRange(candidatePlan);
        }
        public override void AbortActivePlan()
        {
            ActivateAction?.OnDeactivate();
            ActivatePlan.Clear();
            activateAction = null;
        }
        public override void Dispose()
        {
            jobRunner?.Dispose();
        }
    }
}