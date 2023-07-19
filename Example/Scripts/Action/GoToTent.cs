using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Go To Tent 移动至Tent")]
    public class GoToTent : ExampleAction
    {
        protected sealed override void SetupDerived()
        {
            //Set precondition to let action automatically cancel
            preconditions["CanRest"]=false;
            worldState.RegisterNodeTarget(this,agent.Tent);
        }
        protected sealed override void SetupEffects()
        {
            effects["CanRest"]=true;
        }
        public sealed override float GetCost()
        {
            return 1;
        }
        public sealed override void OnTick()
        {
            agent.NavMeshAgent.SetDestination(agent.Tent.position);
            //You can make a trigger to set state or other method based on unity engine lifetime scope
            worldState.SetState("CanRest",Vector3.SqrMagnitude(agent._Transform.position-agent.Tent.position)<1);
        }
    }
}
