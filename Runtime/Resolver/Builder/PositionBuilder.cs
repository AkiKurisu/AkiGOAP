using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class PositionBuilder:IPositionBuilder
    {
        private readonly List<INode> nodeIndexList;
        private float3[] executableList;

        public PositionBuilder(List<INode> nodeIndexList)
        {
            this.nodeIndexList = nodeIndexList;
            this.executableList = this.nodeIndexList.Select(x => GraphResolverJob.InvalidPosition).ToArray();
        }
        
        public IPositionBuilder SetPosition(INode node, Vector3 position)
        {
            var index = this.nodeIndexList.IndexOf(node);

            if (index == -1)
                return this;
            
            this.executableList[index] = position;

            return this;
        }
        
        public float3[] Build()
        {
            return this.executableList;
        }

        public void Clear()
        {
            for(int i=0;i<nodeIndexList.Count;i++)this.executableList[i]=GraphResolverJob.InvalidPosition;
        }
    }
}