using UnityEngine;
namespace Kurisu.GOAP.Example
{
    public class AgentFactory : MonoBehaviour
    {
        [SerializeField]
        private GameObject agentPrefab;
        [SerializeField]
        private int maxAmount=1000;
        [SerializeField]
        private Transform player;
        [SerializeField]
        private Transform home;
        [SerializeField]
        private Transform tent;
        [SerializeField]
        private GOAPSet dataSet;
        [SerializeField]
        private ExampleAgent[] agents;
        private void Awake() {
            agents=new ExampleAgent[maxAmount];
            for(int i=0;i<maxAmount;i++)
            {
                var prefab= Instantiate(agentPrefab,GetRandomPosition(),Quaternion.identity);
                agents[i]=new ExampleAgent(dataSet){
                    _Transform=prefab.transform,
                    Home=home,
                    Tent=tent,
                    Player=player
                };
                agents[i].Init();
            }
            InvokeRepeating("LossEnergy",0.2f,0.2f);
        }
        private void LossEnergy()
        {
            for(int i=0;i<agents.Length;i++)
            {
                agents[i].LossEnergy();
            }
        }
        private Vector3 GetRandomPosition()
        {
            return transform.position+new Vector3(Random.Range(-10f,10f),0,Random.Range(-10f,10f));
        }
    }
}
