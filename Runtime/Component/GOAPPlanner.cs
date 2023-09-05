using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
/// <summary>
/// This code is modified from https://github.com/toastisme/OpenGOAP
/// </summary>
namespace Kurisu.GOAP
{
    /// <summary>
    /// Used to package GOAPGoal data for other classes (e.g. PlannerSnapshot)
    /// </summary>
    public struct GoalData
    {
        public string goalName;
        public float priority;
        public bool canRun;

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
        public ActionNode(ActionNode parent, IAction action)
        {
            this.parent = parent;
            this.action = action;
        }
    }
    [Flags]
    internal enum LogType
    {
        OnlyActive = 2,
        IncludeSearch = 4,
        IncludeFail = 8
    }
    [Flags]
    internal enum TickType
    {
        ManualUpdateGoal = 2,
        ManualActivatePlanner = 4
    }
    /// <summary>
    /// class GOAPPlanner
    /// Identifies the highest priority GOAPGoal with a viable action plan
    /// and carries out that action plan.
    /// </summary>
    [RequireComponent(typeof(GOAPWorldState))]
    public class GOAPPlanner : MonoBehaviour, IPlanner
    {
        protected GOAPWorldState worldState;
        public GOAPWorldState WorldState => worldState;
        protected readonly List<IGoal> goals = new();
        protected readonly List<IAction> actions = new();
        //// Active
        public IGoal ActivateGoal { get; private set; }
        public int ActiveActionIndex { get; private set; }
        public List<IAction> ActivatePlan { get; private set; }

        //// Optimal
        private IGoal optimalGoal;
        private List<IAction> optimalPlan;
        // Loggers
        [SerializeField, Tooltip("Control the log message of planner. Never: No log; OnlyActive: Only logging active plan message; IncludeSearch: Include " +
        "searching detail like action select information; IncludeFail: Include logging all fail message like fail to find path or fail to find a goal. " +
        "Always: Log all message")]
        private LogType logType;
        private bool LogActive => logType.HasFlag(LogType.OnlyActive);
        private bool LogSearch => logType.HasFlag(LogType.IncludeSearch);
        private bool LogFail => logType.HasFlag(LogType.IncludeFail);
        public List<GOAPBehavior> Behaviors => Enumerable.Empty<GOAPBehavior>().Concat(actions.OfType<GOAPBehavior>()).Concat(goals.OfType<GOAPBehavior>()).ToList();
        public UnityEngine.Object Object => gameObject;
        public event System.Action<IPlanner> OnUpdatePlanEvent;
        [SerializeField, Tooltip("Nothing: Automatically update planner.\n" +
        "ManualUpdateGoal: Toggle this to disable planner to tick goal automatically.\n" +
        "ManualActivatePlanner: Toggle this to disable planner to tick and search plan automatically," +
        " however when the plan is generated, the planner will focus that plan until the plan is deactivated. So you can't stop plan manually.")]
        private TickType tickType;
        private readonly Queue<List<IAction>> poolQueue = new();
        public bool IsActive { get; private set; } = true;
        private void Awake()
        {
            worldState = GetComponent<GOAPWorldState>();
            IsActive &= !tickType.HasFlag(TickType.ManualActivatePlanner);
        }
        private void Update()
        {
            //If has ActivePlan and mode is manualStart
            if (IsActive) Tick();
        }
        public void ManualActivate()
        {
            IsActive = true;
        }
        public void InjectGoals(IEnumerable<IGoal> source)
        {
            goals.Clear();
            foreach (var goal in source)
            {
                goals.Add(goal);
                goal.Init(worldState);
            }
        }
        public void InjectActions(IEnumerable<IAction> source)
        {
            actions.Clear();
            foreach (var action in source)
            {
                actions.Add(action);
                action.Init(worldState);
            }
        }
        List<IAction> IPlanner.GetAllActions() => actions;
        public void Tick()
        {
            if (!tickType.HasFlag(TickType.ManualUpdateGoal)) TickGoals();
            OnTickActivePlan();
            GetHighestPriorityGoal(chosenGoal: out optimalGoal, out optimalPlan);
            if ((NoActiveGoal() && GoalAvailable()) || BetterGoalAvailable())
            {
                StartCurrentBestGoal();
                OnUpdatePlanEvent?.Invoke(this);
            }
            else
            {
                if (tickType.HasFlag(TickType.ManualActivatePlanner))
                {
                    IsActive = false;
                    if (LogSearch) PlannerLog("Manual plan updating ends, need to be activated manully again.", bold: true);
                }
            }
        }
        private List<IAction> GetPlan()
        {
            if (poolQueue.Count != 0)
            {
                var plan = poolQueue.Dequeue();
                plan.Clear();
                return plan;
            }
            return new List<IAction>();
        }

        private bool NoActiveGoal()
        {
            return ActivateGoal == null;
        }

        private bool BetterGoalAvailable()
        {
            return optimalGoal != null && optimalGoal != ActivateGoal;
        }

        private bool GoalAvailable()
        {
            return optimalPlan != null && optimalGoal != null;
        }

        private void StartCurrentBestGoal()
        {
            if (ActivateGoal != null)
            {
                ActivateGoal.OnDeactivate();
            }
            if (ActivatePlan != null && ActiveActionIndex < ActivatePlan.Count)
            {
                ActivatePlan[ActiveActionIndex].OnDeactivate();
            }

            ActiveActionIndex = 0;
            ActivateGoal = optimalGoal;
            if (ActivatePlan != null) poolQueue.Enqueue(ActivatePlan);
            ActivatePlan = optimalPlan;
            if (LogActive) ActivePlanLog($"Starting new plan for {ActivateGoal.Name}", bold: true);
            ActivateGoal.OnActivate();
            if (LogActive) ActivePlanLog($"Starting {ActivatePlan[ActiveActionIndex].Name}");
            ActivatePlan[ActiveActionIndex].OnActivate();
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
            if (ActivateGoal == null || ActivatePlan == null) { return; }

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
            if (!(ActivatePlan[ActiveActionIndex].PreconditionsSatisfied(worldState)))
            {
                if (LogActive) ActivePlanLog(
                    $"{ActivatePlan[ActiveActionIndex].Name} failed as preconditions are no longer satisfied",
                    bold: true
                    );
                OnCompleteOrFailActivePlan();
                return;
            }

            ActivatePlan[ActiveActionIndex].OnTick();
            // Goal complete
            if (ActivateGoal.ConditionsSatisfied(worldState))
            {
                if (LogActive) ActivePlanLog($"{ActivateGoal.Name} completed", bold: true);
                OnCompleteOrFailActivePlan();
                return;
            }

            if (ActiveActionIndex < ActivatePlan.Count - 1)
            {
                // At least one more action after activeAction
                for (int i = ActivatePlan.Count - 1; i > ActiveActionIndex; i--)
                {
                    if (ActivatePlan[i].PreconditionsSatisfied(worldState))
                    {
                        // Can skip to a new action
                        if (LogActive) ActivePlanLog($"Stopping {ActivatePlan[ActiveActionIndex].Name}");
                        ActivatePlan[ActiveActionIndex].OnDeactivate();
                        ActiveActionIndex = i;
                        if (LogActive) ActivePlanLog($"Moving to new action: {ActivatePlan[ActiveActionIndex].Name}");
                        ActivatePlan[ActiveActionIndex].OnActivate();
                    }
                }
            }
        }

        private void OnCompleteOrFailActivePlan()
        {
            if (ActivatePlan != null)
            {
                ActivatePlan[ActiveActionIndex].OnDeactivate();
            }
            ActivateGoal?.OnDeactivate();
            ActivateGoal = null;
            poolQueue.Enqueue(ActivatePlan);
            ActivatePlan = null;
        }

        /// <summary>
        ///  Updates chosenGoal and chosenPlan with the highest priorty goal that 
        ///  has a valid plan
        /// </summary>
        /// <param name="chosenGoal"></param>
        /// <param name="chosenPlan"></param>
        private void GetHighestPriorityGoal(out IGoal chosenGoal, out List<IAction> chosenPlan)
        {
            chosenGoal = null;
            chosenPlan = null;
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

                if (chosenGoal != null && !HasHigherPriority(goals[i], chosenGoal))
                {
                    continue;
                }
                if (chosenGoal != null && LogSearch)
                    PlannerLog($"{goals[i].Name} has higher priority than {chosenGoal.Name}");

                List<IAction> candidatePath = GetOptimalPath(
                    currentState: worldState,
                    goal: goals[i]
                    );

                if (candidatePath != null)
                {
                    chosenGoal = goals[i];
                    //Use ObjectPool to recycle Plan List
                    chosenPlan = GetPlan();
                    chosenPlan.AddRange(candidatePath);
                    if (LogSearch) PlannerLog($"Path found. Chosen goal is now {goals[i].Name}", bold: true);
                }
            }
        }

        private bool HasHigherPriority(IGoal goal, IGoal other)
        {
            return goal.GetPriority() > other.GetPriority();
        }
        private static bool InList(IAction action, List<ActionNode> nodeList)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i].action == action)
                {
                    return true;
                }
            }
            return false;
        }
        #region List Cache
        private List<ActionNode> openListCache = new List<ActionNode>();
        private List<ActionNode> closedListCache = new List<ActionNode>();
        private List<IAction> validStartActionsCache = new List<IAction>();
        private List<IAction> linkNodesCache = new List<IAction>();
        private List<IAction> candicatePath = new List<IAction>();
        #endregion
        //// A*
        private List<IAction> GetOptimalPath(GOAPWorldState currentState, IGoal goal)
        {
            if (LogSearch) PlannerLog($"Searching for plan for {goal.Name}", bold: true);
            validStartActionsCache.Clear();
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].SatisfiesConditions(goal.Conditions))
                {
                    validStartActionsCache.Add(actions[i]);
                    if (LogSearch) PlannerLog($"{actions[i].Name} satisfies goal conditions");
                }
            }
            // No path found
            if (validStartActionsCache.Count == 0)
            {
                if (LogSearch) PlannerLog($"No actions found to satisfy {goal.Name} conditions");
                return null;
            }
            validStartActionsCache.Sort(CompareCost);
            List<IAction> path = null;
            for (int i = 0; i < validStartActionsCache.Count; i++)
            {
                openListCache.Clear();
                closedListCache.Clear();
                IAction startAction = validStartActionsCache[i];
                openListCache.Add(new ActionNode(null, startAction));
                if (LogSearch) PlannerLog($"Selected {startAction.Name}, cost:{startAction.GetCost()}", bold: true);
                path = SearchPath(worldState, openListCache, closedListCache, goal);
                if (path != null) return path;
                //Since we don't know the start node is valid,we need to fall back to the next start action
                if (LogSearch) PlannerLog($"Path fall back time:{i}", bold: true);
            }
            if (LogFail) PlannerLog("No path found.");
            return null;
        }
        private static int CompareCost(IAction a, IAction b)
        {
            float costA = a.GetCost();
            float costB = b.GetCost();
            if (costA < costB) return -1;
            else if (costA > costB) return 1;
            else return 0;
        }
        /// <summary>
        /// Search path from a start action node
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="openList"></param>
        /// <param name="closedList"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        private List<IAction> SearchPath(
            GOAPWorldState currentState,
            List<ActionNode> openList,
            List<ActionNode> closedList,
            IGoal goal)
        {

            while (openList.Count != 0)
            {

                ActionNode currentNode = null;
                if (closedList.Count > 0)
                {
                    currentNode = GetNextNode(
                        closedList: closedList,
                        openList: openList
                    );
                }
                else
                {
                    currentNode = GetNextNode(
                        requiredState: goal.Conditions,
                        openList: openList,
                        nodeCost: out float nodeCost,
                        isStartNode: true
                    );
                }

                if (currentNode == null)
                {
                    if (LogFail) PlannerLog("No path found");
                    return null;
                }
                //找到的代价最小且满足前一结点条件的加入CloseList
                closedList.Add(currentNode);
                openList.Remove(currentNode);
                //If CurrentNode can satisfy current state(world State)
                //如果已经满足当前条件则直接返回
                if (currentState.IsSubset(currentNode.action.Preconditions))
                {
                    return GeneratePath(closedList);
                }
                //将满足该结点要求且相邻结点加入OpenList
                List<IAction> linkedActions = GetLinkedActions(
                    node: currentNode.action
                );

                for (int i = 0; i < linkedActions.Count; i++)
                {
                    if (!InList(linkedActions[i], closedList))
                    {
                        if (!InList(linkedActions[i], openList))
                        {
                            openList.Add(new ActionNode(currentNode, linkedActions[i]));
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Iterates through parents starting with the last ActionNode in closedList
        /// to return a list of GOAPActions.
        /// </summary>
        /// <param name="closedList"></param>
        /// <returns></returns>
        private List<IAction> GeneratePath(List<ActionNode> closedList)
        {
            candicatePath.Clear();
            ActionNode currentNode = closedList[closedList.Count - 1];
            while (currentNode.parent != null)
            {
                candicatePath.Add(currentNode.action);
                currentNode = currentNode.parent;
            }
            candicatePath.Add(currentNode.action);
            return candicatePath;
        }
        /// <summary>
        /// Finds the ActionNode in openList that satisfies
        /// conditions of an ActionNode in closedList with the lowest cost.
        /// </summary>
        /// <param name="closedList"></param>
        /// <param name="openList"></param>
        /// <returns></returns>
        private ActionNode GetNextNode(List<ActionNode> closedList, List<ActionNode> openList)
        {
            float minCost = -1f;
            ActionNode nextNode = null;
            ActionNode currentNode;
            for (int i = 0; i < closedList.Count; i++)
            {
                float nodeCost;
                currentNode = GetNextNode(
                    closedList[i].action.Preconditions,
                    openList,
                    out nodeCost
                );
                if ((minCost < 0 || nodeCost < minCost) && currentNode != null)
                {
                    nextNode = currentNode;
                    minCost = nodeCost;
                }
            }
            if (nextNode != null)
            {
                if (LogSearch) PlannerLog($"Selected {nextNode.action.Name}", bold: true);
            }
            else
            {
                if (LogFail) PlannerLog("Could not find next action");
            }
            return nextNode;
        }
        /// <summary>
        /// Searches for the node in openList with the smallest
        /// cost that satisfies requiredState
        /// </summary>
        /// <param name="requiredState"></param>
        /// <param name="openList"></param>
        /// <param name="nodeCost"></param>
        /// <param name="isStartNode"></param>
        /// <returns></returns>
        private ActionNode GetNextNode(Dictionary<string, bool> requiredState, List<ActionNode> openList, out float nodeCost, bool isStartNode = false)
        {
            float minCost = -1f;
            ActionNode nextNode = null;
            for (int i = 0; i < openList.Count; i++)
            {
                if (!openList[i].action.SatisfiesConditions(requiredState))
                {
                    if (LogFail) PlannerLog($"{openList[i].action.Name} does not satisfy conditions");
                    continue;
                }
                if (!isStartNode)
                {
                    //开始结点肯定满足要求(因为已经进行过condition检测)
                    if (LogSearch) PlannerLog($"{openList[i].action.Name} satisfies conditions");
                }
                float cost = openList[i].action.GetCost();
                if (minCost < 0 || cost < minCost)
                {
                    nextNode = openList[i];
                    minCost = cost;
                }
            }
            nodeCost = minCost;
            return nextNode;
        }
        /// <summary>
        /// Searches availableNodes for all those that satisfy node.preconditions 
        /// and that are not in path. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private List<IAction> GetLinkedActions(IAction node)
        {
            if (LogSearch) PlannerLog($"Finding actions linked to {node.Name}");
            linkNodesCache.Clear();
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].SatisfiesConditions(node.Preconditions))
                {
                    if (LogSearch) PlannerLog($"{actions[i].Name} is linked to {node.Name}");
                    linkNodesCache.Add(actions[i]);
                }
            }
            return linkNodesCache;
        }
        /// <summary>
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