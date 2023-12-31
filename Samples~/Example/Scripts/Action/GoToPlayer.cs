namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Go To Player 移动至玩家")]
    public class GoToPlayer : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            Preconditions["HaveEnergy"] = true;
            //Set this precondition to let action automatically cancel
            Preconditions["InDistance"] = false;
        }
        protected sealed override void SetupEffects()
        {
            Effects["InDistance"] = true;
        }
        public sealed override float GetCost()
        {
            return 10;
        }
        public sealed override void OnTick()
        {
            agent.NavMeshAgent.SetDestination(agent.Player.position);
        }
    }
}
