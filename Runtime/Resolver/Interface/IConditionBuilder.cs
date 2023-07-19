namespace Kurisu.GOAP.Resolver
{
    public interface IConditionBuilder
    {
        IConditionBuilder SetConditionMet(GOAPState condition, bool met);
        bool[] Build();
        void Clear();
    }
}