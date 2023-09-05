using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Kurisu.GOAP.Runner;
using Kurisu.GOAP.Resolver;
using Unity.Burst;
namespace Kurisu.GOAP
{
    /// <summary>
    /// class GOAPPlannerPro
    /// Pro version using job system and burst compiler to support multi-thread
    /// </summary>
    [RequireComponent(typeof(GOAPWorldState))]
    public class GOAPPlannerPro : MonoBehaviour, IPlanner
    {
        [BurstCompile]
        private struct GoalSorter : IComparer<IGoal>
        {
            public readonly int Compare(IGoal x, IGoal y)
            {
                return y.GetPriority().CompareTo(x.GetPriority());
            }
        }
        protected GOAPWorldState worldState;
        public GOAPWorldState WorldState => worldState;
        protected readonly List<IGoal> goals = new();
        protected readonly List<IAction> actions = new();
        public IGoal ActivateGoal { get; private set; }
        int IPlanner.ActiveActionIndex => 0;
        private IAction candidateAction;
        private IAction activateAction;
        private readonly List<IAction> candidatePlan = new();
        public IAction ActivateAction => activateAction;
        public List<IAction> ActivatePlan { get; private set; } = new();
        private IGoal candidateGoal;
        private readonly List<IGoal> candidateGoals = new();
        internal List<IGoal> CandidateGoals => candidateGoals;
        // Loggers
        [SerializeField, Tooltip("Control the log message of planner. Never: No log; OnlyActive: Only logging active plan message; IncludeSearch: Include " +
        "searching detail like action select information; IncludeFail: Include logging all fail message like fail to find path or fail to find a goal. " +
        "Always: Log all message")]
        private LogType logType;
        private bool LogActive => logType.HasFlag(LogType.OnlyActive);
        private bool LogSearch => logType.HasFlag(LogType.IncludeSearch);
        private bool LogFail => logType.HasFlag(LogType.IncludeFail);
        [SerializeField, Tooltip("Nothing: Automatically update planner.\n" +
        "ManualUpdateGoal: Toggle this to disable planner to tick goal automatically.\n" +
        "ManualActivatePlanner: Toggle this to disable planner to tick and search plan automatically," +
        " however when the plan is generated, the planner will focus that plan until the plan is deactivated. So you can't stop plan manually.")]
        private TickType tickType;
        [SerializeField]
        private bool skipSearchWhenActionRunning;
        internal bool SkipSearchWhenActionRunning => skipSearchWhenActionRunning;
        public List<GOAPBehavior> Behaviors => Enumerable.Empty<GOAPBehavior>().Concat(actions.OfType<GOAPBehavior>()).Concat(goals.OfType<GOAPBehavior>()).ToList();
        public Object Object => gameObject;
        public event System.Action<IPlanner> OnUpdatePlanEvent;
        private GOAPJobRunner jobRunner;
        private bool isDirty = false;
        [SerializeField]
        private bool isActive = true;
        private bool activateFlag = false;
        private void Awake()
        {
            worldState = GetComponent<GOAPWorldState>();
            isActive &= !tickType.HasFlag(TickType.ManualActivatePlanner);
        }
        private void Update()
        {
            if (activateFlag)
            {
                isActive = true;
                activateFlag = false;
            }
            if (isDirty)
            {
                isDirty = false;
                jobRunner?.Dispose();
                jobRunner = new GOAPJobRunner(this, new GraphResolver(actions.Cast<INode>().Concat(goals)));
            }
            if (isActive) Tick();
        }
        public void ManualActivate()
        {
            //Ensure jobRunner activate in Update()
            activateFlag = true;
        }
        private void LateUpdate()
        {
            if (!isActive) return;
            //Ensure job is completed
            jobRunner?.Complete();
            bool debugUpdate = false;
            if ((NoActiveGoal() && CandidateGoalAvailable()) || BetterGoalAvailable())
            {
                StartCurrentBestGoal();
                debugUpdate = true;
            }
            else if (HaveNextAction())
            {
                StartCurrentBestAction();
                debugUpdate = true;
            }
            if (debugUpdate)
                OnUpdatePlanEvent?.Invoke(this);
            else
            {
                if (tickType.HasFlag(TickType.ManualActivatePlanner))
                {
                    isActive = false;
                    if (LogSearch) PlannerLog("Manual plan updating ends, need to be activated manully again.", bold: true);
                }
            }
        }
        private void OnDestroy()
        {
            jobRunner?.Dispose();
        }
        void IPlanner.InjectGoals(IEnumerable<IGoal> source)
        {
            goals.Clear();
            foreach (var goal in source)
            {
                goals.Add(goal);
                goal.Init(worldState);
            }
            isDirty = true;
        }
        void IPlanner.InjectActions(IEnumerable<IAction> source)
        {
            actions.Clear();
            foreach (var action in source)
            {
                actions.Add(action);
                action.Init(worldState);
            }
            isDirty = true;
        }
        internal void SetCandidate(List<IAction> path, IGoal goal)
        {
            if (goal == null || path.Count == 0)
            {
                candidatePlan.Clear();
                candidateAction = null;
                candidateGoal = null;
                if (LogFail) PlannerLog("No candiate goal or path was found.");
                return;
            }
            var action = path[0];
            candidatePlan.Clear();
            candidatePlan.AddRange(path);
            if (candidateAction != action && LogSearch) PlannerLog($"Search candidate action:{action.Name}");
            candidateAction = action;
            if (candidateGoal != goal && LogSearch) PlannerLog($"Search candidate goal:{goal.Name}");
            candidateGoal = goal;
        }
        List<IAction> IPlanner.GetAllActions() => actions;
        public void Tick()
        {
            if (!tickType.HasFlag(TickType.ManualUpdateGoal)) TickGoals();
            OnTickActivePlan();
            GetHighestPriorityGoals(candidateGoals);
            jobRunner?.Run();
        }

        private bool NoActiveGoal()
        {
            return ActivateGoal == null;
        }
        private bool HaveNextAction()
        {
            return candidateAction != null && candidateAction != ActivateAction;
        }
        private bool BetterGoalAvailable()
        {
            return candidateGoal != null && candidateGoal != ActivateGoal;
        }

        private bool CandidateGoalAvailable()
        {
            return candidateAction != null && candidateGoal != null;
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
        public void TickGoals()
        {
            if (goals != null)
            {
                for (int i = 0; i < goals.Count; i++)
                {
                    goals[i].OnTick();
                }
            }
        }
        private void OnTickActivePlan()
        {
            // Nothing to run
            if (ActivateGoal == null || ActivateAction == null) { return; }
            // Goal no longer viable
            if (!ActivateGoal.PreconditionsSatisfied(worldState))
            {
                if (LogActive) ActivePlanLog(
                    $"{ActivateGoal.Name} failed as preconditions are no longer satisfied",
                    bold: true
                );
                OnCompleteOrFailActivePlan();
                return;
            }

            // Plan no longer viable
            if (!(ActivateAction.PreconditionsSatisfied(worldState)))
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
            if (ActivateGoal.ConditionsSatisfied(worldState))
            {
                if (LogActive) ActivePlanLog($"{ActivateGoal.Name} completed", bold: true);
                OnCompleteOrFailActivePlan();
                return;
            }
        }
        private void OnCompleteOrFailActivePlan()
        {
            ActivateAction?.OnDeactivate();
            ActivateGoal?.OnDeactivate();
            ActivateGoal = null;
            SetCurrentAction(null);
        }
        private void SetCurrentAction(IAction action)
        {
            ActivatePlan.Clear();
            activateAction = action;
            if (activateAction != null)
                ActivatePlan.AddRange(candidatePlan);
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
            if (goals == null || goals.Count == 0)
            {
                if (LogFail) PlannerLog("No goals found");
                return;
            }
            for (int i = 0; i < goals.Count; i++)
            {

                if (!goals[i].PreconditionsSatisfied(worldState))
                {
                    if (LogFail) PlannerLog($"{goals[i].Name} not valid as preconditions not satisfied");
                    continue;
                }
                chosenGoals.Add(goals[i]);
            }
            chosenGoals.Sort(new GoalSorter());
        }
        // <summary>
        /// Returns list of GoalData reverse sorted by priority (Editor Debug Method)
        /// </summary>
        /// <returns></returns>
        List<GoalData> IPlanner.GetSortedGoalData()
        {
            List<GoalData> goalData = new();
            for (int i = 0; i < goals.Count; i++)
            {
                goalData.Add(
                    new GoalData(goals[i].Name, goals[i].GetPriority(), goals[i].PreconditionsSatisfied(worldState))
                );
            }
            goalData.Sort((x, y) => x.priority.CompareTo(y.priority));
            goalData.Reverse();
            return goalData;
        }
        private void ActivePlanLog(object message, bool bold = false)
        {
            string s = $"<color=#5BDB14>ActivePlan: {message}</color>";
            if (bold)
            {
                s = "<b>" + s + "</b>";
            }
            Debug.Log(s, this);
        }

        private void PlannerLog(object message, bool bold = false)
        {
            string s = $"<color=#00C2FF>Planner: {message}</color>";
            if (bold)
            {
                s = "<b>" + s + "</b>";
            }
            Debug.Log(s, this);
        }
    }
}