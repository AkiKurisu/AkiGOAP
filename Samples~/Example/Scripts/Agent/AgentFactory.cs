using UnityEngine;
namespace Kurisu.GOAP.Example
{
    public class AgentFactory : MonoBehaviour
    {
        [SerializeField]
        private GameObject agentPrefab;
        [SerializeField]
        private int maxAmount = 1000;
        [SerializeField]
        private Transform player;
        [SerializeField]
        private Transform home;
        [SerializeField]
        private Transform tent;
        [SerializeField]
        private GOAPSet dataSet;
        private ExampleAgent[] agents;
        [Header("Runtime Property"), SerializeField]
        private float agentEnergy;
        private void Awake()
        {
            agents = new ExampleAgent[maxAmount];
            for (int i = 0; i < maxAmount; i++)
            {
                var prefab = Instantiate(agentPrefab, GetRandomPosition(), Quaternion.identity);
                agents[i] = new ExampleAgent(dataSet)
                {
                    Transform = prefab.transform,
                    Home = home,
                    Tent = tent,
                    Player = player
                };
                agents[i].Init();
            }
            InvokeRepeating(nameof(LossEnergy), 0.2f, 0.2f);
        }
        private void LossEnergy()
        {
            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].LossEnergy();
            }
            agentEnergy = agents[0].Energy;
        }
        private Vector3 GetRandomPosition()
        {
            return transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        }
    }
}
