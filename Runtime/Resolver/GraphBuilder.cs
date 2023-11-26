using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    internal class GraphBuilder
    {
        public Graph Build(IEnumerable<INode> nodesToBuild)
        {
            var (RootNodes, ChildNodes) = nodesToBuild.ToNodes();

            var graph = new Graph
            {
                RootNodes = RootNodes.ToList(),
            };

            var allNodes = RootNodes.Union(ChildNodes).ToArray();

            var effectMap = GetEffectMap(allNodes);
            var conditionMap = GetConditionMap(allNodes);

            foreach (var node in RootNodes)
            {
                ConnectNodes(node, effectMap, conditionMap, graph);
            }

            return graph;
        }

        private void ConnectNodes(Node node, Dictionary<string, List<Node>> effectMap, Dictionary<string, List<Node>> conditionMap, Graph graph)
        {
            if (!graph.ChildNodes.Contains(node) && !node.IsRootNode)
                graph.ChildNodes.Add(node);

            foreach (var actionNodeCondition in node.Conditions)
            {
                if (actionNodeCondition.Connections.Any())
                    continue;

                var key = actionNodeCondition.Condition.UniqueID;

                if (!effectMap.ContainsKey(key))
                    continue;

                actionNodeCondition.Connections = effectMap[key].ToArray();

                foreach (var connection in actionNodeCondition.Connections)
                {
                    connection.Effects.First(x => x.Effect.UniqueID == key).Connections = conditionMap[key].ToArray();
                }

                foreach (var subNode in actionNodeCondition.Connections)
                {
                    ConnectNodes(subNode, effectMap, conditionMap, graph);
                }
            }
        }

        private Dictionary<string, List<Node>> GetEffectMap(Node[] actionNodes)
        {
            var map = new Dictionary<string, List<Node>>();

            foreach (var actionNode in actionNodes)
            {
                foreach (var actionNodeEffect in actionNode.Effects)
                {
                    var key = actionNodeEffect.Effect.UniqueID;

                    if (!map.ContainsKey(key))
                        map[key] = new List<Node>();

                    map[key].Add(actionNode);
                }
            }

            return map;
        }

        private Dictionary<string, List<Node>> GetConditionMap(Node[] actionNodes)
        {
            var map = new Dictionary<string, List<Node>>();

            foreach (var actionNode in actionNodes)
            {
                foreach (var actionNodeConditions in actionNode.Conditions)
                {
                    var key = actionNodeConditions.Condition.UniqueID;

                    if (!map.ContainsKey(key))
                        map[key] = new List<Node>();

                    map[key].Add(actionNode);
                }
            }

            return map;
        }
    }
    internal static class Extensions
    {
        public static (Node[] RootNodes, Node[] ChildNodes) ToNodes(this IEnumerable<INode> nodes)
        {
            var mappedNodes = nodes.Select(ToNode).ToArray();

            return (
                mappedNodes.Where(x => x.IsRootNode).ToArray(),
                mappedNodes.Where(x => !x.IsRootNode).ToArray()
            );
        }

        private static Node ToNode(INode node)
        {
            return new Node
            {
                InternalNode = node,
                Conditions = node.ConditionStates?.Select(y => new NodeCondition
                {
                    Condition = y
                }).ToList() ?? new List<NodeCondition>(),
                Effects = node.EffectStates?.Select(y => new NodeEffect
                {
                    Effect = y
                }).ToList() ?? new List<NodeEffect>()
            };
        }
    }
}