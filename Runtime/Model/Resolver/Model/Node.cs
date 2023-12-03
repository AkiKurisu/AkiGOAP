using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    internal class Node
    {
        public INode InternalNode { get; set; }
        public List<NodeEffect> Effects { get; set; } = new List<NodeEffect>();
        public List<NodeCondition> Conditions { get; set; } = new List<NodeCondition>();
        public bool IsRootNode => InternalNode.EffectStates == null || !InternalNode.EffectStates.Any();
    }
}
