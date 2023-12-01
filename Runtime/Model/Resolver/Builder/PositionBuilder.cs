using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class PositionBuilder : IPositionBuilder
    {
        private readonly List<INode> nodeIndexList;
        private readonly float3[] executableList;

        public PositionBuilder(List<INode> nodeIndexList)
        {
            this.nodeIndexList = nodeIndexList;
            executableList = this.nodeIndexList.Select(x => GraphResolverJob.InvalidPosition).ToArray();
        }

        public IPositionBuilder SetPosition(INode node, Vector3 position)
        {
            var index = nodeIndexList.IndexOf(node);

            if (index == -1)
                return this;

            executableList[index] = position;

            return this;
        }

        public float3[] Build()
        {
            return executableList;
        }

        public void Clear()
        {
            for (int i = 0; i < nodeIndexList.Count; i++) executableList[i] = GraphResolverJob.InvalidPosition;
        }
    }
}