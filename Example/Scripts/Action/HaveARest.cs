using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Have A Rest 休息一下")]
    public class HaveARest : ExampleAction
    {
        //We can expose some property to the graph editor
        //Use GOAPLabel to change the label of field in graph editor
        [SerializeField, GOAPLabel("Wait Time 等待时间")]
        private float waitTime = 5;
        private float timer;
        protected sealed override void SetupDerived()
        {
            Preconditions["CanRest"] = true;
            //Set this precondition to let action automatically cancel
            Preconditions["HaveEnergy"] = false;
        }
        protected sealed override void SetupEffects()
        {
            Effects["HaveEnergy"] = true;
        }
        public sealed override void OnTick()
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                timer = 0;
                agent.Energy = 100;
                worldState.SetState("HaveEnergy", true);
            }
        }
        protected sealed override void OnActivateDerived()
        {
            //Reset timer when enter this action
            timer = 0;
            agent.NavMeshAgent.isStopped = true;
        }
        protected sealed override void OnDeactivateDerived()
        {
            agent.NavMeshAgent.isStopped = false;
            //Reset signal state
            worldState.SetState("CanRest", false);
        }
    }
}
