using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Object = UnityEngine.Object;
namespace Kurisu.GOAP
{
    /// <summary>
    /// Used to package GOAPGoal data for other classes (e.g. Snapshot)
    /// </summary>
    public readonly struct GoalData
    {
        public readonly string goalName;
        public readonly float priority;
        public readonly bool canRun;

        public GoalData(string goalName, float priority, bool canRun)
        {
            this.priority = priority;
            this.goalName = goalName;
            this.canRun = canRun;
        }
    }
    /// <summary>
    /// Used to create a linked list of GOAPActions
    /// </summary>
    public class ActionNode
    {
        public ActionNode parent;
        public IAction action;
        private bool pooled = false;
        public static ActionNode Get(ActionNode parent, IAction action)
        {
            var node = pool.Get();
            node.parent = parent;
            node.action = action;
            node.pooled = false;
            return node;
        }

        public void Pooled()
        {
            if (pooled) return;
            pool.Push(this);
            pooled = true;
        }

        private static readonly ObjectPool<ActionNode> pool = new(() => new(), 10);
    }
    [Flags]
    public enum LogType
    {
        OnlyActive = 2,
        IncludeSearch = 4,
        IncludeFail = 8
    }
    [Flags]
    public enum TickType
    {
        ManualUpdateGoal = 2,
        ManualActivatePlanner = 4
    }
    public enum PlannerBackend
    {
        /// <summary>
        /// Use main loop to calculate anything, but less memory cost.
        /// Can perform well if designed well.
        /// </summary>
        Main,
        /// <summary>
        /// Use job system to calculate path, can execute complex action graph but more memory cost.
        /// Manual tick for heavy calculation is recommended.
        /// </summary>
        JobSystem
    }
    public class GOAPPlanner : MonoBehaviour, IPlanner, IBackendHost
    {
        [SerializeField]
        private PlannerBackend backendType;
        private BaseBackend backend;
        public IGoal ActivateGoal => backend.ActivateGoal;
        public List<IAction> ActivatePlan => backend.ActivatePlan;
        public WorldState WorldState { get; private set; }
        public int ActiveActionIndex => backend.ActiveActionIndex;
        public List<GOAPBehavior> Behaviors => backend != null ? Enumerable.Empty<GOAPBehavior>()
                                                .Concat(backend.Actions.OfType<GOAPBehavior>())
                                                .Concat(backend.Goals.OfType<GOAPBehavior>())
                                                .ToList() : new();
        public Object Object => gameObject;
        public Transform Transform => transform;
        public event Action<IPlanner> OnUpdate;
        public event Action<IPlanner> OnReload;
        [SerializeField, Tooltip("Control the log message of planner. Never: No log; OnlyActive: Only logging active plan message; IncludeSearch: Include " +
       "searching detail like action select information; IncludeFail: Include logging all fail message like fail to find path or fail to find a goal. " +
       "Always: Log all message")]
        private LogType logType;
        [SerializeField, Tooltip("Nothing: Automatically update planner.\n" +
       "ManualUpdateGoal: Toggle this to disable planner to tick goal automatically.\n" +
       "ManualActivatePlanner: Toggle this to disable planner to tick and search plan automatically," +
       " however when the plan is generated, the planner will focus that plan until the plan is deactivated. So you can't stop plan manually.")]
        private TickType tickType;
        [SerializeField]
        private bool skipSearchWhenActionRunning;
        [SerializeField]
        private bool isActive = true;
        #region Host Status
        bool IBackendHost.SkipSearchWhenActionRunning => skipSearchWhenActionRunning;
        bool IBackendHost.IsActive { get => isActive; set => isActive = value; }
        LogType IBackendHost.LogType => logType;
        TickType IBackendHost.TickType => tickType;
        #endregion
        private void Awake()
        {
            WorldState = GetComponent<WorldState>();
            if (backendType == PlannerBackend.Main)
            {
                backend = new MainBackend(this);
            }
            else
            {
                backend = new JobSystemBackend(this);
            }
            isActive &= !tickType.HasFlag(TickType.ManualActivatePlanner);
        }
        private void Update()
        {
            backend.Update();
        }
        private void LateUpdate()
        {
            backend.LateUpdate();
        }
        private void OnDisable()
        {
            backend.OnDisable();
        }
        private void OnDestroy()
        {
            backend.Dispose();
        }
        public List<GoalData> GetSortedGoalData()
        {
            List<GoalData> goalData = new();
            var goals = backend.Goals;
            for (int i = 0; i < goals.Count; i++)
            {
                goalData.Add(
                    new GoalData(goals[i].Name, goals[i].GetPriority(), goals[i].PreconditionsSatisfied(WorldState))
                );
            }
            goalData.Sort((x, y) => x.priority.CompareTo(y.priority));
            goalData.Reverse();
            return goalData;
        }

        public void InjectGoals(IEnumerable<IGoal> source)
        {
            backend.InjectGoals(source);
            OnReload?.Invoke(this);
        }
        public void InjectActions(IEnumerable<IAction> source)
        {
            backend.InjectActions(source);
            OnReload?.Invoke(this);
        }

        public void ManualActivate()
        {
            backend.ManualActivate();
        }

        public void TickGoals()
        {
            backend.TickGoals();
        }

        public void NotifyUpdate()
        {
            OnUpdate?.Invoke(this);
        }
    }
}
