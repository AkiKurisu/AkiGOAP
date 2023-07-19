using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class CostBuilder:ICostBuilder
    {
        private readonly List<INode> nodeIndexList;
        private float[] costList;

        public CostBuilder(List<INode> nodeIndexList)
        {
            this.nodeIndexList = nodeIndexList;
            this.costList = this.nodeIndexList.Select(x => 1f).ToArray();
        }
        
        public ICostBuilder SetCost(INode node, float cost)
        {
            var index = this.nodeIndexList.IndexOf(node);

            if (index == -1)
                return this;
            
            this.costList[index] = cost;

            return this;
        }
        
        public float[] Build()
        {
            return this.costList;
        }
    }
}