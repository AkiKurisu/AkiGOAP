using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    internal class Graph
    {
        public List<Node> RootNodes { get; set; } = new();
        public List<Node> ChildNodes { get; set; } = new();
        public Node[] AllNodes => this.RootNodes.Union(this.ChildNodes).ToArray();
    }
}