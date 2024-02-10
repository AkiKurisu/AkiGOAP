using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    [BurstCompile]
    public struct NodeData
    {
        public int Index;
        public float G;
        public float H;
        public int ParentIndex;
        public readonly float F => G + H;
    }

    [BurstCompile]
    public struct RunData
    {
        public int StartIndex;
        // Index = NodeIndex
        public NativeArray<bool> IsExecutable;
        // Index = ConditionIndex
        public NativeArray<bool> ConditionsMet;
        public NativeArray<float3> Positions;
        public NativeArray<float> Costs;
        public float DistanceMultiplier;
    }

    [BurstCompile]
    public struct NodeSorter : IComparer<NodeData>
    {
        public readonly int Compare(NodeData x, NodeData y)
        {
            return x.F.CompareTo(y.F);
        }
    }

    [BurstCompile]
    public struct GraphResolverJob : IJob
    {
#if UNITY_COLLECTIONS_1_3
        // Dictionary<ActionIndex, ConditionIndex[]>
        [ReadOnly] public NativeParallelMultiHashMap<int, int> NodeConditions;
        // Dictionary<ConditionIndex, NodeIndex[]>
        [ReadOnly] public NativeParallelMultiHashMap<int, int> ConditionConnections;
#else
        [ReadOnly] public NativeMultiHashMap<int, int> NodeConditions;
        [ReadOnly] public NativeMultiHashMap<int, int> ConditionConnections;
#endif
        // Resolve specific
        [ReadOnly] public RunData RunData;

        // Results
        public NativeList<NodeData> Result;

        public static readonly float3 InvalidPosition = new(float.MaxValue, float.MaxValue, float.MaxValue);
        [BurstCompile]
        public void Execute()
        {
            var nodeCount = NodeConditions.Count();
            var runData = RunData;
#if UNITY_COLLECTIONS_1_3
            var openSet = new NativeParallelHashMap<int, NodeData>(nodeCount, Allocator.Temp);
            var closedSet = new NativeParallelHashMap<int, NodeData>(nodeCount, Allocator.Temp);
#else
            var openSet = new NativeHashMap<int, NodeData>(nodeCount, Allocator.Temp);
            var closedSet = new NativeHashMap<int, NodeData>(nodeCount, Allocator.Temp);
#endif
            var nodeData = new NodeData
            {
                Index = runData.StartIndex,
                G = 0,
                H = int.MaxValue,
                ParentIndex = -1
            };
            openSet.Add(runData.StartIndex, nodeData);
            while (!openSet.IsEmpty)
            {
                var openList = openSet.GetValueArray(Allocator.Temp);
                openList.Sort(new NodeSorter());

                var currentNode = openList[0];

                if (runData.IsExecutable[currentNode.Index])
                {
                    RetracePath(currentNode, closedSet, Result);
                    break;
                }

                closedSet.TryAdd(currentNode.Index, currentNode);
                openSet.Remove(currentNode.Index);

                // If this node has a condition that is false and has no connections, it is unresolvable
                if (HasUnresolvableCondition(currentNode.Index))
                {
                    continue;
                }

                foreach (var conditionIndex in NodeConditions.GetValuesForKey(currentNode.Index))
                {
                    if (runData.ConditionsMet[conditionIndex])
                    {
                        continue;
                    }

                    foreach (var neighborIndex in ConditionConnections.GetValuesForKey(conditionIndex))
                    {
                        if (closedSet.ContainsKey(neighborIndex))
                        {
                            continue;
                        }

                        var newG = currentNode.G + RunData.Costs[neighborIndex];
                        // Current neighbor is not in the open set
                        if (!openSet.TryGetValue(neighborIndex, out NodeData neighbor))
                        {
                            neighbor = new NodeData
                            {
                                Index = neighborIndex,
                                G = newG,
                                H = Heuristic(neighborIndex, currentNode.Index),
                                ParentIndex = currentNode.Index
                            };
                            openSet.Add(neighborIndex, neighbor);
                            continue;
                        }

                        // This neighbor has a lower cost
                        if (newG < neighbor.G)
                        {
                            neighbor.G = newG;
                            neighbor.ParentIndex = currentNode.Index;

                            openSet.Remove(neighborIndex);
                            openSet.Add(neighborIndex, neighbor);
                        }
                    }
                }

                openList.Dispose();
            }

            openSet.Dispose();
            closedSet.Dispose();
        }
        [BurstCompile]
        private float Heuristic(int currentIndex, int previousIndex)
        {
            var previousPosition = RunData.Positions[previousIndex];
            var currentPosition = RunData.Positions[currentIndex];

            if (previousPosition.Equals(InvalidPosition) || currentPosition.Equals(InvalidPosition))
            {
                return 0f;
            }

            return math.distance(previousPosition, currentPosition) * RunData.DistanceMultiplier;
        }
#if UNITY_COLLECTIONS_1_3
        private readonly void RetracePath(NodeData startNode, NativeParallelHashMap<int, NodeData> closedSet, NativeList<NodeData> path)
#else
        private readonly void RetracePath(NodeData startNode, NativeHashMap<int, NodeData> closedSet, NativeList<NodeData> path)
#endif
        {
            var currentNode = startNode;
            while (currentNode.ParentIndex != -1)
            {
                path.Add(currentNode);
                currentNode = closedSet[currentNode.ParentIndex];
            }
        }

        private bool HasUnresolvableCondition(int currentIndex)
        {
            foreach (var conditionIndex in NodeConditions.GetValuesForKey(currentIndex))
            {
                //Already fit current world state
                if (RunData.ConditionsMet[conditionIndex])
                {
                    continue;
                }
                //No node fit condition
                if (!ConditionConnections.GetValuesForKey(conditionIndex).MoveNext())
                {
                    return true;
                }
            }

            return false;
        }
    }
}