using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class CostBuilder : ICostBuilder
    {
        private readonly List<INode> nodeIndexList;
        private readonly float[] costList;

        public CostBuilder(List<INode> nodeIndexList)
        {
            this.nodeIndexList = nodeIndexList;
            costList = this.nodeIndexList.Select(x => 1f).ToArray();
        }

        public ICostBuilder SetCost(INode node, float cost)
        {
            var index = nodeIndexList.IndexOf(node);

            if (index == -1)
                return this;

            costList[index] = cost;

            return this;
        }

        public float[] Build()
        {
            return costList;
        }
    }
}