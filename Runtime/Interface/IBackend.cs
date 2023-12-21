using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IBackend
    {
        public List<IAction> Actions { get; }
        public List<IGoal> Goals { get; }
    }
}