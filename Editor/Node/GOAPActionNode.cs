using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPActionNode : GOAPNode
    {
        public GOAPActionNode() : base()
        {
            costLabel = new();
            costLabel.style.color = Color.black;
            costLabel.style.fontSize = 14;
            stateLabel = new()
            {
                enableRichText = true
            };
            stateLabel.style.color = Color.white;
            stateLabel.style.fontSize = 12;
        }
        private readonly Label costLabel;
        private readonly Label stateLabel;
        protected sealed override void OnCleanUp()
        {
            costLabel.RemoveFromHierarchy();
            stateLabel.RemoveFromHierarchy();
        }
        public void SetUp(GOAPAction action)
        {
            SetStyle(true);
            costLabel.text = $"Cost : {action.GetCost()}";
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("<b>Conditions</b>:");
            foreach (var state in action.ConditionStates)
            {
                stringBuilder.Append(state.Key);
                stringBuilder.Append(":");
                stringBuilder.AppendLine(state.Value.ToString());
            }
            stringBuilder.AppendLine("\n<b>Effects</b>:");
            //Do not use `EffectStates` for preventing dynamic effects trigger
            foreach (var state in action.Effects)
            {
                stringBuilder.Append(state.Key);
                stringBuilder.Append(":");
                stringBuilder.AppendLine(state.Value.ToString());
            }
            if (stringBuilder[^1] == '\n') stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stateLabel.text = stringBuilder.ToString();
            titleContainer.Add(costLabel);
            mainContainer.Add(stateLabel);
        }
    }
}
