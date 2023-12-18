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
        public GOAPAction Action => NodeBehavior as GOAPAction;
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
            if (Action.IsSelected)
            {
                stringBuilder.AppendLine("Always Satisfied");
            }
            else
            {
                if (action.ConditionStates != null)
                {
                    foreach (var state in action.ConditionStates)
                    {
                        stringBuilder.Append(state.Key);
                        stringBuilder.Append(":");
                        stringBuilder.AppendLine(state.Value.ToString());
                    }
                }
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
        protected override void OnRestore()
        {
            if (Action.IsSelected)
                AddToClassList("AlwaysSatisfied");
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (Application.isPlaying && Graph.Set is IPlanner planner)
            {
                if (Action.IsSelected)
                    evt.menu.MenuItems().Add(new NodeMenuAction("Disable Always Satisfied", (a) =>
                    {
                        Action.IsSelected = false;
                        RemoveFromClassList("AlwaysSatisfied");
                    }, x => DropdownMenuAction.Status.Normal));
                else
                    evt.menu.MenuItems().Add(new NodeMenuAction("Enable Always Satisfied", (a) =>
                    {
                        Action.IsSelected = true;
                        AddToClassList("AlwaysSatisfied");
                    }, x => DropdownMenuAction.Status.Normal));
            }
        }
    }
}
