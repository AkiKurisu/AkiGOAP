using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class GraphResolver : IGraphResolver
    {
        private readonly List<Node> indexList;
        private readonly List<INode> nodeIndexList;

        private readonly List<NodeCondition> conditionList;
        private readonly List<GOAPState> conditionIndexList;
#if UNITY_COLLECTIONS_1_3
        // Dictionary<ActionIndex, ConditionIndex[]>
        private NativeParallelMultiHashMap<int, int> nodeConditions;
        // Dictionary<ConditionIndex, NodeIndex[]>
        private NativeParallelMultiHashMap<int, int> conditionConnections;
#else
        private NativeMultiHashMap<int, int> nodeConditions;
        private NativeMultiHashMap<int, int> conditionConnections;
#endif
        private readonly Graph graph;
        public GraphResolver(IEnumerable<INode> nodes)
        {
            graph = new GraphBuilder().Build(nodes);
            indexList = graph.AllNodes.ToList();
            nodeIndexList = indexList.Select(x => x.InternalNode).ToList();

            conditionList = indexList.SelectMany(x => x.Conditions).ToList();
            conditionIndexList = conditionList.Select(x => x.Condition).ToList();

            CreateNodeConditions();
            CreateConditionConnections();
        }
        public int GetIndex(INode node) => nodeIndexList.IndexOf(node);
        public INode GetNode(int index) => nodeIndexList[index];
        private void CreateNodeConditions()
        {
#if UNITY_COLLECTIONS_1_3
            var map = new NativeParallelMultiHashMap<int, int>(indexList.Count, Allocator.Persistent);
#else
            var map = new NativeMultiHashMap<int, int>(indexList.Count, Allocator.Persistent);
#endif
            for (var i = 0; i < indexList.Count; i++)
            {
                var conditions = indexList[i].Conditions
                    .Select(x => conditionIndexList.IndexOf(x.Condition));

                foreach (var condition in conditions)
                {
                    map.Add(i, condition);
                }
            }

            nodeConditions = map;
        }

        private void CreateConditionConnections()
        {
#if UNITY_COLLECTIONS_1_3
            var map = new NativeParallelMultiHashMap<int, int>(conditionIndexList.Count, Allocator.Persistent);
#else
            var map = new NativeMultiHashMap<int, int>(conditionIndexList.Count, Allocator.Persistent);
#endif
            for (var i = 0; i < conditionIndexList.Count; i++)
            {
                var connections = conditionList[i].Connections
                    .Select(x => indexList.IndexOf(x));

                foreach (var connection in connections)
                {
                    map.Add(i, connection);
                }
            }

            conditionConnections = map;
        }

        public IResolveHandle StartResolve(RunData runData)
        {
            return new ResolveHandle(this, nodeConditions, conditionConnections, runData);
        }
        public IExecutableBuilder GetExecutableBuilder()
        {
            return new ExecutableBuilder(nodeIndexList);
        }

        public IPositionBuilder GetPositionBuilder()
        {
            return new PositionBuilder(nodeIndexList);
        }

        public ICostBuilder GetCostBuilder()
        {
            return new CostBuilder(nodeIndexList);
        }

        public IConditionBuilder GetConditionBuilder()
        {
            return new ConditionBuilder(conditionIndexList);
        }
        public void Dispose()
        {
            nodeConditions.Dispose();
            conditionConnections.Dispose();
        }
    }
}
