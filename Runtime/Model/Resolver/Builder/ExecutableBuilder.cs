using System.Collections.Generic;
using System.Linq;
/// <summary>
/// This code is modified from https://github.com/crashkonijn/GOAP
/// </summary>
namespace Kurisu.GOAP.Resolver
{
    public class ExecutableBuilder : IExecutableBuilder
    {
        private readonly List<INode> nodeIndexList;
        private readonly bool[] executableList;

        public ExecutableBuilder(List<INode> nodeIndexList)
        {
            this.nodeIndexList = nodeIndexList;
            executableList = this.nodeIndexList.Select(x => false).ToArray();
        }

        public IExecutableBuilder SetExecutable(INode node, bool executable)
        {
            var index = nodeIndexList.IndexOf(node);

            if (index == -1)
                return this;

            executableList[index] = executable;

            return this;
        }

        public void Clear()
        {
            for (int i = 0; i < nodeIndexList.Count; i++) executableList[i] = false;
        }

        public bool[] Build()
        {
            return executableList;
        }
    }
}