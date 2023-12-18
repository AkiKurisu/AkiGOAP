using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Idle Close to Player 在玩家附近待机")]
    public class IdleCloseToPlayer : ExampleGoal
    {
        [SerializeField]
        private float distance = 4;
        [SerializeField]
        private int minAmount = 0;
        protected sealed override void SetupDerived()
        {
            Conditions["Idle"] = true;
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
            worldState.SetState("HaveEnergy", agent.Energy > minAmount);
        }
    }
}
