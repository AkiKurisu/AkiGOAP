using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class GraphResolver:IGraphResolver
    {
        private readonly List<Node> indexList;
        private readonly List<INode> nodeIndexList;

        private readonly List<NodeCondition> conditionList;
        private readonly List<GOAPState> conditionIndexList;
        // Dictionary<ActionIndex, ConditionIndex[]>
        private NativeMultiHashMap<int, int> nodeConditions;
        // Dictionary<ConditionIndex, NodeIndex[]>
        private NativeMultiHashMap<int, int> conditionConnections;
        private Graph graph;
        public GraphResolver(IEnumerable<INode> nodes)
        {
            this.graph = new GraphBuilder().Build(nodes);
            this.indexList = this.graph.AllNodes.ToList();
            this.nodeIndexList = this.indexList.Select(x => x.InternalNode).ToList();
            
            this.conditionList = this.indexList.SelectMany(x => x.Conditions).ToList();
            this.conditionIndexList = this.conditionList.Select(x => x.Condition).ToList();
            
            this.CreateNodeConditions();
            this.CreateConditionConnections();
        }
        public int GetIndex(INode node) => this.nodeIndexList.IndexOf(node);
        public INode GetNode(int index) => this.nodeIndexList[index];
        private void CreateNodeConditions()
        {
            var map = new NativeMultiHashMap<int, int>(this.indexList.Count, Allocator.Persistent);            
            for (var i = 0; i < this.indexList.Count; i++)
            {
                var conditions = this.indexList[i].Conditions
                    .Select(x => this.conditionIndexList.IndexOf(x.Condition));

                foreach (var condition in conditions)
                {
                    map.Add(i, condition);
                }
            }
            
            this.nodeConditions = map;
        }

        private void CreateConditionConnections()
        {
            var map = new NativeMultiHashMap<int, int>(this.conditionIndexList.Count, Allocator.Persistent);
            for (var i = 0; i < this.conditionIndexList.Count; i++)
            {
                var connections = this.conditionList[i].Connections
                    .Select(x => this.indexList.IndexOf(x));

                foreach (var connection in connections)
                {
                    map.Add(i, connection);
                }
            }
            
            this.conditionConnections = map;
        }

        public IResolveHandle StartResolve(RunData runData)
        {
            return new ResolveHandle(this, this.nodeConditions, this.conditionConnections, runData);
        }
        public IExecutableBuilder GetExecutableBuilder()
        {
            return new ExecutableBuilder(this.nodeIndexList);
        }
        
        public IPositionBuilder GetPositionBuilder()
        {
            return new PositionBuilder(this.nodeIndexList);
        }

        public ICostBuilder GetCostBuilder()
        {
            return new CostBuilder(this.nodeIndexList);
        }
        
        public IConditionBuilder GetConditionBuilder()
        {
            return new ConditionBuilder(this.conditionIndexList);
        }
        public void Dispose()
        {
            this.nodeConditions.Dispose();
            this.conditionConnections.Dispose();
        }
    }
}
