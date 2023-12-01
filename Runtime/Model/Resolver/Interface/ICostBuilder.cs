namespace Kurisu.GOAP.Resolver
{
    public interface ICostBuilder
    {
        ICostBuilder SetCost(INode node, float cost);
        float[] Build();
    }
}