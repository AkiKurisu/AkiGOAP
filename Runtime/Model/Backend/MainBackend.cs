using System.Collections.Generic;
namespace Kurisu.GOAP
{
    /// <summary>
    /// Execution on main loop, but more controllable, especially for dynamic effects
    /// </summary>
    public class MainBackend : BaseBackend
    {
        #region Activate
        private int activeActionIndex;
        public override int ActiveActionIndex => activeActionIndex;
        private List<IAction> activePlan;
        public override List<IAction> ActivatePlan => activePlan;
        #endregion
        #region Optimal
        private IGoal optimalGoal;
        private List<IAction> optimalPlan;
        #endregion
        private static readonly ObjectPool<List<IAction>> poolQueue = new(() => new(Capacity), Capacity);
        public MainBackend(IBackendHost backendHost) : base(backendHost) { }
        #region List Cache
        private readonly List<ActionNode> openListCache = new(Capacity);
        private readonly List<ActionNode> closedListCache = new(Capacity);
        private readonly List<IAction> validStartActionsCache = new(Capacity);
        private readonly List<IAction> linkNodesCache = new(Capacity);
        private readonly List<IAction> candidatePath = new(Capacity);
        #endregion
        public override void ManualActivate()
        {
            IsActive = true;
        }
        public override void Update()
        {
            if (!IsActive) return;
            if (!TickType.HasFlag(TickType.ManualUpdateGoal)) TickGoals();
            OnTickActivePlan();
            GetHighestPriorityGoal(chosenGoal: out optimalGoal, out optimalPlan);
            if ((NoActiveGoal() && GoalAvailable()) || BetterGoalAvailable())
            {
                StartCurrentBestGoal();
                NotifyHostUpdate();
            }
            else
            {
                if (TickType.HasFlag(TickType.ManualActivatePlanner))
                {
                    IsActive = false;
                    if (LogSearch) PlannerLog("Manual plan updating ends, need to be activated manually again.", bold: true);
                }
            }
        }
        private bool GoalAvailable()
        {
            return optimalPlan != null && optimalGoal != null;
        }

        private bool BetterGoalAvailable()
        {
            return optimalGoal != null && optimalGoal != ActivateGoal;
        }
        private List<IAction> GetPlan()
        {
            var pool = poolQueue.Get();
            pool.Clear();
            return pool;
        }
        private void StartCurrentBestGoal()
        {
            ActivateGoal?.OnDeactivate();
            if (ActivatePlan != null && ActiveActionIndex < ActivatePlan.Count)
            {
                ActivatePlan[ActiveActionIndex].OnDeactivate();
            }

            activeActionIndex = 0;
            ActivateGoal = optimalGoal;
            if (ActivatePlan != null) poolQueue.Push(ActivatePlan);
            activePlan = optimalPlan;
            if (LogActive) ActivePlanLog($"Starting new plan for {ActivateGoal.Name}", bold: true);
            ActivateGoal.OnActivate();
            if (LogActive) ActivePlanLog($"Starting {ActivatePlan[ActiveActionIndex].Name}");
            ActivatePlan[ActiveActionIndex].OnActivate();
        }
        private void OnTickActivePlan()
        {

            // Nothing to run
            if (ActivateGoal == null || ActivatePlan == null) return;

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
            if (!ActivatePlan[ActiveActionIndex].PreconditionsSatisfied(WorldState))
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
            if (ActivateGoal.ConditionsSatisfied(WorldState))
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
                    if (ActivatePlan[i].PreconditionsSatisfied(WorldState))
                    {
                        // Can skip to a new action
                        if (LogActive) ActivePlanLog($"Stopping {ActivatePlan[ActiveActionIndex].Name}");
                        ActivatePlan[ActiveActionIndex].OnDeactivate();
                        activeActionIndex = i;
                        if (LogActive) ActivePlanLog($"Moving to new action: {ActivatePlan[ActiveActionIndex].Name}");
                        ActivatePlan[ActiveActionIndex].OnActivate();
                    }
                }
            }
        }

        private void OnCompleteOrFailActivePlan()
        {
            ActivatePlan?[ActiveActionIndex].OnDeactivate();
            ActivateGoal?.OnDeactivate();
            ActivateGoal = null;
            poolQueue.Push(ActivatePlan);
            activePlan = null;
        }

        /// <summary>
        ///  Updates chosenGoal and chosenPlan with the highest priory goal that 
        ///  has a valid plan
        /// </summary>
        /// <param name="chosenGoal"></param>
        /// <param name="chosenPlan"></param>
        private void GetHighestPriorityGoal(out IGoal chosenGoal, out List<IAction> chosenPlan)
        {
            chosenGoal = null;
            chosenPlan = null;
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

                if (chosenGoal != null && !HasHigherPriority(Goals[i], chosenGoal))
                {
                    continue;
                }
                if (chosenGoal != null && LogSearch)
                    PlannerLog($"{Goals[i].Name} has higher priority than {chosenGoal.Name}");

                List<IAction> candidatePath = GetOptimalPath(goal: Goals[i]);

                if (candidatePath != null)
                {
                    chosenGoal = Goals[i];
                    //Use ObjectPool to recycle Plan List
                    chosenPlan = GetPlan();
                    chosenPlan.AddRange(candidatePath);
                    if (LogSearch) PlannerLog($"Path found. Chosen goal is now {Goals[i].Name}", bold: true);
                }
            }
        }

        private bool HasHigherPriority(IGoal goal, IGoal other)
        {
            return goal.GetPriority() > other.GetPriority();
        }
        //// A*
        private List<IAction> GetOptimalPath(IGoal goal)
        {
            if (LogSearch) PlannerLog($"Searching for plan for {goal.Name}", bold: true);
            validStartActionsCache.Clear();
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].SatisfiesConditions(goal.Conditions))
                {
                    validStartActionsCache.Add(Actions[i]);
                    if (LogSearch) PlannerLog($"{Actions[i].Name} satisfies goal conditions");
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
            foreach (var node in openListCache)
            {
                node.Pooled();
            }
            openListCache.Clear();
            foreach (var node in closedListCache)
            {
                node.Pooled();
            }
            closedListCache.Clear();
            foreach (var action in validStartActionsCache)
            {
                openListCache.Add(ActionNode.Get(null, action));
            }
            //Create state cache for first node
            var stateCache = StateCache.Get(goal.Conditions);
            path = SearchPath(stateCache, openListCache, closedListCache);
            //Don't forget to pool cache
            stateCache.Pooled();
            if (path != null) return path;
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
        /// <param name="stateCache"></param>
        /// <param name="openList"></param>
        /// <param name="closedList"></param>
        /// <returns></returns>
        private List<IAction> SearchPath(
            StateCache stateCache,
            List<ActionNode> openList,
            List<ActionNode> closedList)
        {
            static bool InList(IAction action, List<ActionNode> nodeList)
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
            while (openList.Count != 0)
            {

                ActionNode currentNode;
                if (closedList.Count > 0)
                {
                    currentNode = GetNextNode(
                        stateCache,
                        closedList: closedList,
                        openList: openList
                    );
                }
                else
                {
                    currentNode = GetNextNode(
                        //Equal to goal.Conditions for first node
                        requiredState: stateCache.ToDictionary(),
                        openList: openList,
                        nodeCost: out float nodeCost
                    );
                    if (LogSearch && currentNode != null)
                        PlannerLog($"{currentNode.action.Name} satisfies with min cost {nodeCost}");
                }
                if (currentNode == null)
                {
                    if (LogFail) PlannerLog("No path found");
                    return null;
                }
                //Copy a duplicated version
                var copy = StateCache.Copy(stateCache);
                AppendState(copy, currentNode);
                //The one found with the smallest cost and that meets the conditions of the previous node is added to CloseList
                closedList.Add(currentNode);
                openList.Remove(currentNode);
                //If currentNode can satisfy state cache (which is managed backward through finding) then return the path
                if (WorldState.IsSubset(copy.ToDictionary()))
                {
                    //Pool duplicated version
                    copy.Pooled();
                    return GeneratePath(closedList);
                }
                //Add adjacent nodes that meet the node requirements to OpenList. 
                //This search phase does not require that all requirements be met.
                List<IAction> linkedActions = GetLinkedActions(copy.ToDictionary(), currentNode.action);
                copy.Pooled();
                for (int i = 0; i < linkedActions.Count; i++)
                {
                    if (!InList(linkedActions[i], closedList))
                    {
                        if (!InList(linkedActions[i], openList))
                        {
                            openList.Add(ActionNode.Get(currentNode, linkedActions[i]));
                        }
                    }
                }
            }
            return null;
        }
        private static void AppendState(StateCache stateCache, ActionNode actionNode)
        {
            if (actionNode.parent != null) AppendState(stateCache, actionNode.parent);
            //Remove effects from cache which means requirement is fulfilled by current node
            //The node connect in front of currentNode not need to meet this requirement anymore
            stateCache.DeleteIntersection(actionNode.action.Effects);
            //Add currentNode preconditions which means there should has one node connected
            //in front of currentNode can fulfill this requirement
            stateCache.TryJoin(actionNode.action.Preconditions);
        }
        /// <summary>
        /// Iterates through parents starting with the last ActionNode in closedList
        /// to return a list of GOAPActions.
        /// </summary>
        /// <param name="closedList"></param>
        /// <returns></returns>
        private List<IAction> GeneratePath(List<ActionNode> closedList)
        {
            candidatePath.Clear();
            ActionNode currentNode = closedList[^1];
            while (currentNode.parent != null)
            {
                candidatePath.Add(currentNode.action);
                currentNode = currentNode.parent;
            }
            candidatePath.Add(currentNode.action);
            return candidatePath;
        }
        /// <summary>
        /// Finds the ActionNode in openList that satisfies
        /// conditions of an ActionNode in closedList with the lowest cost.
        /// </summary>
        /// <param name="stateCache"></param>
        /// <param name="closedList"></param>
        /// <param name="openList"></param>
        /// <returns></returns>
        private ActionNode GetNextNode(StateCache stateCache, List<ActionNode> closedList, List<ActionNode> openList)
        {
            float minCost = -1f;
            ActionNode nextNode = null;
            ActionNode currentNode;
            for (int i = 0; i < closedList.Count; i++)
            {
                //There are some possible results
                //Goal needs precondition: [a:1,b:1,c:0]
                //Path 1:
                //Node A meet [a:1], also requires [b:1]
                //So requirement now becomes [b:1,c:0]
                //Path 2:
                //Node A meet [a:1], but requires [c:1\
                //So requirement now becomes [b:1,c:0,c:1], which is not possible
                //So it means we meet a collision
                //Only if Node A fulfill c:0 can it add new requirement
                var cache = StateCache.Copy(stateCache);
                AppendState(cache, closedList[i]);
                currentNode = GetNextNode(
                    cache.ToDictionary(),
                    openList,
                    out float nodeCost
                );
                cache.Pooled();
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
        /// <returns></returns>
        private ActionNode GetNextNode(
            Dictionary<string, bool> requiredState,
            List<ActionNode> openList,
            out float nodeCost)
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
        private List<IAction> GetLinkedActions(Dictionary<string, bool> requiredState, IAction node)
        {
            if (LogSearch) PlannerLog($"Finding actions linked to {node.Name}");
            linkNodesCache.Clear();
            for (int i = 0; i < Actions.Count; i++)
            {

                if (Actions[i].SatisfiesConditions(requiredState))
                {
                    if (LogSearch) PlannerLog($"{Actions[i].Name} is linked to {node.Name}");
                    linkNodesCache.Add(Actions[i]);
                }
                else
                {
                    if (LogFail) PlannerLog($"{Actions[i].Name} not satisfy {node.Name}");
                }
            }
            return linkNodesCache;
        }
    }
}