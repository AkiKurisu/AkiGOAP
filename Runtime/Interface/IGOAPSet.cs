using System.Collections.Generic;
namespace Kurisu.GOAP
{
    public interface IGOAPSet
    {
        List<GOAPBehavior> Behaviors { get; }
        UnityEngine.Object Object { get; }
    }
}
