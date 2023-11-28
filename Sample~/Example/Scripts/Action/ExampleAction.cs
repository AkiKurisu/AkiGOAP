namespace Kurisu.GOAP.Example
{
    public abstract class ExampleAction : GOAPAction
    {
        protected ExampleAgent agent;
        public void Inject(ExampleAgent agent)
        {
            this.agent=agent;
        }
    }
}
