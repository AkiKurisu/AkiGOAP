namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Idle 静止")]
    public class Idle : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            Preconditions["Idle"] = false;
            Preconditions["InDistance"] = true;
            Preconditions["HaveEnergy"] = true;
        }
        protected sealed override void SetupEffects()
        {
            Effects["Idle"] = true;
        }
        public sealed override float GetCost()
        {
            return 0;
        }
    }
}
