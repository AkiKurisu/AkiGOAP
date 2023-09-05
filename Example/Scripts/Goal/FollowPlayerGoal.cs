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
            Preconditions["HaveEnergy"] = true;
            Preconditions["InDistance"] = false;
            Conditions["InDistance"] = true;
        }
        public sealed override float GetPriority()
        {
            return 1f;
        }
        public sealed override void OnTick()
        {
            //If distance * distance is smaller than 4, set 'InDistance' to true
            worldState.SetState("InDistance", Vector3.SqrMagnitude(agent.Transform.position - agent.Player.position) < distance);
        }
    }
}
