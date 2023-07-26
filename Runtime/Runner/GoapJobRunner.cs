using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Kurisu.GOAP.Resolver;
using UnityEngine;
namespace Kurisu.GOAP.Runner
{
    public class GOAPJobRunner
    {
        private readonly GOAPPlannerPro planner;
        private readonly IGraphResolver resolver;
        private readonly List<JobRunHandle> resolveHandles = new();
        private readonly IExecutableBuilder executableBuilder;
        private readonly IPositionBuilder positionBuilder;
        private readonly ICostBuilder costBuilder;
        private readonly IConditionBuilder conditionBuilder;
        private List<IAction> resultCache = new();
        public GOAPJobRunner(GOAPPlannerPro planner, IGraphResolver graphResolver)
        {
            this.planner = planner;
            this.resolver = graphResolver;

            this.executableBuilder = this.resolver.GetExecutableBuilder();
            this.positionBuilder = this.resolver.GetPositionBuilder();
            this.costBuilder = this.resolver.GetCostBuilder();
            this.conditionBuilder = this.resolver.GetConditionBuilder();
        }

        public void Run()
        {
            this.resolveHandles.Clear();
            RunInternal(planner);
        }

        private void RunInternal(GOAPPlannerPro planner)
        {
            if (planner == null)
                return;
            if (planner.CandidateGoals.Count == 0)
                return;
            if (planner.ActivateAction != null && planner.SkipSearchWhenActionRunning)
                return;
            this.FillBuilders(planner, planner.transform);
            //Create job for each candidate goal
            foreach (var goal in planner.CandidateGoals)
                this.resolveHandles.Add(new JobRunHandle(goal, this.resolver.StartResolve(new RunData
                {
                    StartIndex = this.resolver.GetIndex(goal),
                    IsExecutable = new NativeArray<bool>(this.executableBuilder.Build(), Allocator.TempJob),
                    Positions = new NativeArray<float3>(this.positionBuilder.Build(), Allocator.TempJob),
                    Costs = new NativeArray<float>(this.costBuilder.Build(), Allocator.TempJob),
                    ConditionsMet = new NativeArray<bool>(this.conditionBuilder.Build(), Allocator.TempJob),
                    DistanceMultiplier = 1f
                })));
        }

        private void FillBuilders(IPlanner agent, Transform transform)
        {
            this.executableBuilder.Clear();
            this.positionBuilder.Clear();
            this.conditionBuilder.Clear();

            foreach (var node in agent.GetAllActions())
            {
                var allMet = true;
                foreach (var condition in node.Conditions)
                {
                    if (!agent.WorldState.InSet(condition.Key, condition.Value))
                    {
                        allMet = false;
                        continue;
                    }
                    this.conditionBuilder.SetConditionMet(condition, true);
                }
                this.executableBuilder.SetExecutable(node, allMet);
                this.costBuilder.SetCost(node, node.GetCost());
                this.positionBuilder.SetPosition(node, agent.WorldState.ResolveNodeTarget(node)?.position ?? transform.position);
            }
        }
        public void Complete()
        {
            bool find = false;
            foreach (var resolveHandle in this.resolveHandles)
            {
                if (find)
                {
                    //Already search a plan, just complete
                    resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
                    continue;
                }
                resultCache.Clear();
                resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
                if (planner == null)
                    continue;
                if (resultCache.Count != 0)
                {
                    //Get candidate goal and action with highest priority
                    planner.SetCandidate(resultCache, resolveHandle.Goal);
                    //If not find, thus fall back to next handle
                    find = true;
                }
            }
            this.resolveHandles.Clear();
            if (!find)
                planner.SetCandidate(resultCache, null);
        }

        public void Dispose()
        {
            foreach (var resolveHandle in this.resolveHandles)
            {
                resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
            }

            this.resolver.Dispose();
        }

        private struct JobRunHandle
        {
            public IGoal Goal { get; }
            public IResolveHandle Handle { get; set; }
            public JobRunHandle(IGoal goal, IResolveHandle handle)
            {
                this.Goal = goal;
                this.Handle = handle;
            }
        }
    }
}