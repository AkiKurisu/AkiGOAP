using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Follow Player 跟随玩家")]
    public class FollowPlayerGoal : ExampleGoal
    {
        [SerializeField]
        private float distance = 4;
        protected sealed override void SetupDerived()
        {
            //This precondition works as a state key to let planner transfer to other goal
            Preconditions["HaveEnergy"] = true;
            //Set this precondition to let goal automatically cancel
            Preconditions["InDistance"] = false;
            Conditions["InDistance"] = true;
        }
        protected sealed override float SetupPriority()
        {
            return 1f;
        }
        public sealed override void OnTick()
        {
            //Following condition works as world sensor
            //If distance * distance is smaller than 4, set 'InDistance' to true
            worldState.SetState("InDistance", Vector3.SqrMagnitude(agent.Transform.position - agent.Player.position) < distance);
        }
    }
}
