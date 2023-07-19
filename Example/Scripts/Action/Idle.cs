namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Idle 静止")]
    public class Idle : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            preconditions["InDistance"]=true;
            preconditions["HaveEnergy"]=true;
        }
        public sealed override float GetCost()
        {
            return 0;
        }
    }
}
