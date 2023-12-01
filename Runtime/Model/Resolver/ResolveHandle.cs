using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public struct ResolveHandle : IResolveHandle
    {
        private readonly GraphResolver graphResolver;
        private readonly JobHandle handle;
        private readonly GraphResolverJob job;
        private bool IsCompleted { get; set; }
#if UNITY_COLLECTIONS_1_3
        public ResolveHandle(GraphResolver graphResolver, NativeParallelMultiHashMap<int, int> nodeConditions, NativeParallelMultiHashMap<int, int> conditionConnections, RunData runData)
#else
        public ResolveHandle(GraphResolver graphResolver, NativeMultiHashMap<int, int> nodeConditions, NativeMultiHashMap<int, int> conditionConnections, RunData runData)
#endif
        {
            this.graphResolver = graphResolver;
            IsCompleted = false;
            job = new GraphResolverJob
            {
                NodeConditions = nodeConditions,
                ConditionConnections = conditionConnections,
                RunData = runData,
                Result = new NativeList<NodeData>(Allocator.TempJob)
            };

            handle = job.Schedule();
        }
        public void CompleteNonAlloc(ref List<IAction> resultCache)
        {
            if (IsCompleted) return;
            resultCache.Clear();
            IsCompleted = true;
            handle.Complete();
            foreach (var data in job.Result)
            {
                var node = graphResolver.GetNode(data.Index);
                if (node is IAction)
                    resultCache.Add(node as IAction);
            }
            job.Result.Dispose();
            job.RunData.IsExecutable.Dispose();
            job.RunData.Positions.Dispose();
            job.RunData.Costs.Dispose();
            job.RunData.ConditionsMet.Dispose();
        }
    }
}