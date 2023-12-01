using System.Collections.Generic;
namespace Kurisu.GOAP.Resolver
{
    public interface IResolveHandle
    {
        void CompleteNonAlloc(ref List<IAction> resultCache);
    }
}