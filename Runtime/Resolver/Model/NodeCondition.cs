using System;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    internal class NodeCondition
    {
        public GOAPState Condition { get; set; }
        public Node[] Connections { get; set; } = Array.Empty<Node>();
    }
}