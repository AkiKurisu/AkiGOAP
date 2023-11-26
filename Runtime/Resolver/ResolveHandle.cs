using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public struct ResolveHandle:IResolveHandle
    {
        private readonly GraphResolver graphResolver;
        private readonly JobHandle handle;
        private readonly GraphResolverJob job;
        private bool IsCompleted{get;set;}
        public ResolveHandle(GraphResolver graphResolver, NativeMultiHashMap<int, int> nodeConditions, NativeMultiHashMap<int, int> conditionConnections, RunData runData)
        {
            this.graphResolver = graphResolver;
            this.IsCompleted=false;
            this.job = new GraphResolverJob
            {
                NodeConditions = nodeConditions,
                ConditionConnections = conditionConnections,
                RunData = runData,
                Result = new NativeList<NodeData>(Allocator.TempJob)
            };
        
            this.handle = this.job.Schedule();
        }
        public void CompleteNonAlloc(ref List<IAction> resultCache)
        {
            if(IsCompleted)return;
            resultCache.Clear();
            IsCompleted=true;
            this.handle.Complete();
            foreach (var data in this.job.Result)
            {
                var node=this.graphResolver.GetNode(data.Index);
                if(node is IAction)
                resultCache.Add(node as IAction);
            }
            this.job.Result.Dispose();
            this.job.RunData.IsExecutable.Dispose();
            this.job.RunData.Positions.Dispose();
            this.job.RunData.Costs.Dispose();
            this.job.RunData.ConditionsMet.Dispose();
        }
    }
}