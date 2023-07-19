namespace Kurisu.GOAP.Example
{
    public abstract class ExampleGoal : GOAPGoal
    {
        protected ExampleAgent agent;
        public void Inject(ExampleAgent agent)
        {
            this.agent=agent;
        }
    }
}
