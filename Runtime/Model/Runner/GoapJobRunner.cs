using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Kurisu.GOAP.Resolver;
using UnityEngine;
namespace Kurisu.GOAP.Runner
{
    public class GOAPJobRunner
    {
        private readonly JobSystemBackend backend;
        private readonly IGraphResolver resolver;
        private readonly List<JobRunHandle> resolveHandles = new();
        private readonly IExecutableBuilder executableBuilder;
        private readonly IPositionBuilder positionBuilder;
        private readonly ICostBuilder costBuilder;
        private readonly IConditionBuilder conditionBuilder;
        private List<IAction> resultCache = new();
        private bool isRunning;
        public GOAPJobRunner(JobSystemBackend backend, IGraphResolver graphResolver)
        {
            this.backend = backend;
            resolver = graphResolver;

            executableBuilder = resolver.GetExecutableBuilder();
            positionBuilder = resolver.GetPositionBuilder();
            costBuilder = resolver.GetCostBuilder();
            conditionBuilder = resolver.GetConditionBuilder();
        }

        public void Run()
        {
            resolveHandles.Clear();
            if (backend == null)
                return;
            if (backend.CandidateGoals.Count == 0)
                return;
            if (backend.ActivateAction != null && backend.BackendHost.SearchMode >= SearchMode.OnActionComplete)
                return;
            isRunning = true;
            FillBuilders(backend, backend.BackendHost.Transform);
            //Create job for each candidate goal
            foreach (var goal in backend.CandidateGoals)
            {
                positionBuilder.SetPosition(goal, backend.BackendHost.Transform.position);
                resolveHandles.Add(new JobRunHandle(goal, resolver.StartResolve(new RunData
                {
                    StartIndex = resolver.GetIndex(goal),
                    IsExecutable = new NativeArray<bool>(executableBuilder.Build(), Allocator.TempJob),
                    Positions = new NativeArray<float3>(positionBuilder.Build(), Allocator.TempJob),
                    Costs = new NativeArray<float>(costBuilder.Build(), Allocator.TempJob),
                    ConditionsMet = new NativeArray<bool>(conditionBuilder.Build(), Allocator.TempJob),
                    DistanceMultiplier = 1f
                })));
            }
        }
        /// <summary>
        /// Current executableBuilder only works for current action's conditionStates 
        /// Not consider the Effects brought about by path traversal which can only be set in job execution
        /// MainBackend use StateCache <see cref="StateCache"/> to fix this problem
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="transform"></param>
        private void FillBuilders(JobSystemBackend backend, Transform transform)
        {
            executableBuilder.Clear();
            positionBuilder.Clear();
            conditionBuilder.Clear();

            foreach (var action in backend.Actions)
            {
                var allMet = true;
                if (action.ConditionStates != null)
                {
                    foreach (var condition in action.ConditionStates)
                    {
                        if (!backend.BackendHost.WorldState.InSet(condition.Key, condition.Value))
                        {
                            allMet = false;
                            continue;
                        }
                        conditionBuilder.SetConditionMet(condition, true);
                    }
                }
                //TODO: Fix SetExecutable
                executableBuilder.SetExecutable(action, allMet);
                costBuilder.SetCost(action, action.GetCost());
                Transform target = backend.BackendHost.WorldState.ResolveNodeTarget(action);
                Vector3 position = target != null ? target.position : transform.position;
                positionBuilder.SetPosition(action, position);
            }
        }
        public void Complete()
        {
            if (!isRunning) return;
            isRunning = false;
            bool find = false;
            foreach (var resolveHandle in resolveHandles)
            {
                if (find)
                {
                    //Already search a plan, just complete
                    resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
                    continue;
                }
                resultCache.Clear();
                resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
                if (backend == null)
                    continue;
                if (resultCache.Count != 0)
                {
                    //Get candidate goal and action with highest priority
                    backend.SetCandidate(resultCache, resolveHandle.Goal);
                    //If not find, thus fall back to next handle
                    find = true;
                }
            }
            resolveHandles.Clear();
            if (!find)
                backend.SetCandidate(resultCache, null);
        }

        public void Dispose()
        {
            foreach (var resolveHandle in resolveHandles)
            {
                resolveHandle.Handle.CompleteNonAlloc(ref resultCache);
            }
            resolver.Dispose();
        }

        private struct JobRunHandle
        {
            public IGoal Goal { get; }
            public IResolveHandle Handle { get; set; }
            public JobRunHandle(IGoal goal, IResolveHandle handle)
            {
                Goal = goal;
                Handle = handle;
            }
        }
    }
}