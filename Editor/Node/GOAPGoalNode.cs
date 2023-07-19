using UnityEngine;
using UnityEngine.UIElements;

namespace Kurisu.GOAP.Editor
{
    public class GOAPGoalNode : GOAPNode
    {
        public GOAPGoalNode():base()
        {
            label.style.color=Color.black;
        }
        private readonly Label label=new Label();
        
        protected sealed override void OnCleanUp()
        {
            titleContainer.Remove(label);
        }
        public void SetUp(float priority,bool canRun,bool isCurrent)
        {
            if(canRun)style.backgroundColor=isCurrent?Color.green:Color.yellow;
            label.text=$"Priority : {priority}";
            titleContainer.Add(label);
        }
    }
}
