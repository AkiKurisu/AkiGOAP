namespace Kurisu.GOAP.Resolver
{
    public interface IExecutableBuilder
    {
        IExecutableBuilder SetExecutable(INode node, bool executable);
        void Clear();
        bool[] Build();
    }
}