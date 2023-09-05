namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Idle 静止")]
    public class Idle : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            Preconditions["InDistance"] = true;
            Preconditions["HaveEnergy"] = true;
        }
        public sealed override float GetCost()
        {
            return 0;
        }
    }
}
