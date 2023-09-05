using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPActionNode : GOAPNode
    {
        public GOAPActionNode() : base()
        {
            label.style.color = Color.black;
        }
        private readonly Label label = new();
        protected sealed override void OnCleanUp()
        {
            titleContainer.Remove(label);
        }
        public void SetUp(float cost)
        {
            style.backgroundColor = Color.green;
            label.text = $"Cost : {cost}";
            titleContainer.Add(label);
        }
    }
}
