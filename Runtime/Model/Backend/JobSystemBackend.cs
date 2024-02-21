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
        private IGoal candidateGoal;
        private readonly List<IAction> candidatePlan = new(Capacity);
        private readonly List<IGoal> candidateGoals = new(Capacity);
        public List<IGoal> CandidateGoals => candidateGoals;
        public IAction ActivateAction
        {
            get
            {
                if (activeActionIndex < ActivatePlan.Count) return ActivatePlan[activeActionIndex];
                return null;
            }
        }
        private int activeActionIndex = 0;
        public override int ActiveActionIndex => activeActionIndex;
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
            if (!TickType.HasFlag(TickType.ManualTickGoal)) TickGoals();
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
            else if (HaveBetterPlan())
            {
                StartCurrentBestPlan();
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
                candidateGoal = null;
                if (LogFail) PlannerLog("No candidate goal or path was found.");
                return;
            }
            candidatePlan.Clear();
            candidatePlan.AddRange(path);
            if (candidateGoal != goal && LogSearch) PlannerLog($"Set candidate goal:{goal.Name}");
            candidateGoal = goal;
        }

        private void StartCurrentBestGoal()
        {
            ActivateGoal?.OnDeactivate();
            ActivateGoal = candidateGoal;
            if (LogActive) ActivePlanLog($"Starting new plan for {ActivateGoal.Name}", bold: true);
            ActivateGoal.OnActivate();
            StartCurrentBestPlan();
        }
        private void StartCurrentBestPlan()
        {
            if (CanTickPlan)
                ActivateAction?.OnDeactivate();
            activeActionIndex = 0;
            ActivatePlan.Clear();
            ActivatePlan.AddRange(candidatePlan);
            if (LogActive) ActivePlanLog($"Starting {ActivateAction.Name}");
            if (CanTickPlan)
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
                if (SearchMode != SearchMode.OnPlanComplete)
                {
                    OnCompleteOrFailActivePlan();
                    return;
                }
                else
                {
                    //Use original plan cache to activate next
                    //If search mode use OnActionComplete, it is not necessary
                    if (TryActivateNext())
                    {
                        NotifyHostUpdate();
                    }
                    else
                    {
                        OnCompleteOrFailActivePlan();
                        return;
                    }
                }
            }
            if (CanTickPlan)
                ActivateAction.OnTick();
            // Goal complete
            if (ActivateGoal.ConditionsSatisfied(WorldState))
            {
                if (LogActive) ActivePlanLog($"{ActivateGoal.Name} completed", bold: true);
                OnCompleteOrFailActivePlan();
                return;
            }
        }
        private bool TryActivateNext()
        {
            // At least one more action after activeAction
            for (int i = activeActionIndex + 1; i < ActivatePlan.Count; ++i)
            {
                if (ActivatePlan[i].PreconditionsSatisfied(WorldState))
                {
                    // Can skip to a new action
                    if (LogActive) ActivePlanLog($"Stopping {ActivatePlan[activeActionIndex]}");
                    if (CanTickPlan)
                        ActivatePlan[activeActionIndex].OnDeactivate();
                    activeActionIndex = i;
                    if (LogActive) ActivePlanLog($"Moving to new action: {ActivatePlan[activeActionIndex]}");
                    if (CanTickPlan)
                        ActivatePlan[activeActionIndex].OnActivate();
                    return true;
                }
            }
            return false;
        }
        private bool CandidateGoalAvailable()
        {
            return candidatePlan.Count > 0 && candidateGoal != null;
        }
        private bool HaveBetterPlan()
        {
            if (candidatePlan.Count != ActivatePlan.Count) return true;
            for (int i = 0; i < candidatePlan.Count; ++i)
            {
                if (candidatePlan[i] != ActivatePlan[i]) return true;
            }
            return false;
        }
        private bool BetterGoalAvailable()
        {
            return candidateGoal != null && candidateGoal != ActivateGoal;
        }
        private void OnCompleteOrFailActivePlan()
        {
            if (CanTickPlan)
                ActivateAction?.OnDeactivate();
            ActivateGoal?.OnDeactivate();
            bool needNotify = ActivateGoal != null;
            ActivateGoal = null;
            ActivatePlan.Clear();
            activeActionIndex = 0;
            if (needNotify) NotifyHostUpdate();
        }
        public override void AbortActivePlan()
        {
            if (CanTickPlan)
                ActivateAction?.OnDeactivate();
            ActivatePlan.Clear();
            activeActionIndex = 0;
        }
        public override void CleanUp()
        {
            if (CanTickPlan)
                ActivateAction?.OnDeactivate();
            ActivatePlan.Clear();
            activeActionIndex = 0;
            ActivateGoal?.OnDeactivate();
            ActivateGoal = null;
            candidatePlan.Clear();
            candidateGoal = null;
        }
        public override void Dispose()
        {
            jobRunner?.Dispose();
        }
    }
}