using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IBackend
    {
        List<IAction> Actions { get; }
        List<IGoal> Goals { get; }
    }
}