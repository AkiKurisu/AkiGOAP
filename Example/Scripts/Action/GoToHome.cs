using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Go To Home 前往家")]
    public class GoToHome : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            //Set precondition to let action automatically cancel
            Preconditions["CanRest"] = false;
            worldState.RegisterNodeTarget(this, agent.Home);
        }
        protected sealed override void SetupEffects()
        {
            Effects["CanRest"] = true;
        }
        public sealed override float GetCost()
        {
            return 1;
        }
        public sealed override void OnTick()
        {
            agent.NavMeshAgent.SetDestination(agent.Home.position);
            //You can make a trigger to set state or other method based on unity engine lifetime scope
            worldState.SetState("CanRest", Vector3.SqrMagnitude(agent._Transform.position - agent.Home.position) < 1);
        }
    }
}
