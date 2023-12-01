namespace Kurisu.GOAP.Resolver
{
    public interface IGraphResolver
    {
        IResolveHandle StartResolve(RunData runData);
        IExecutableBuilder GetExecutableBuilder();
        IPositionBuilder GetPositionBuilder();
        ICostBuilder GetCostBuilder();
        int GetIndex(INode node);
        INode GetNode(int index);
        void Dispose();
        IConditionBuilder GetConditionBuilder();
    }
}