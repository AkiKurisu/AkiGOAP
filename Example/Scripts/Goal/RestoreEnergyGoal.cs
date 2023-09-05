using UnityEngine;
namespace Kurisu.GOAP.Example
{
    [GOAPLabel("Restore Energy 恢复能量")]
    public class RestoreEnergyGoal : ExampleGoal
    {
        [SerializeField]
        private int minAmount = 0;
        protected sealed override void SetupDerived()
        {
            Preconditions["HaveEnergy"] = false;
            Conditions["HaveEnergy"] = true;
        }
        public sealed override float GetPriority()
        {
            return 0.5f;
        }
        public sealed override void OnTick()
        {
            worldState.SetState("HaveEnergy", agent.Energy > minAmount);
        }
    }
}
