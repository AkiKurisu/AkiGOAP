using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPGoalNode : GOAPNode
    {
        public GOAPGoalNode() : base()
        {
            priorityLabel = new();
            priorityLabel.style.color = Color.black;
            priorityLabel.style.fontSize = 14;
            stateLabel = new()
            {
                enableRichText = true
            };
            stateLabel.style.color = Color.white;
            stateLabel.style.fontSize = 12;
        }
        private readonly Label priorityLabel;
        private readonly Label stateLabel;
        public GOAPGoal Goal => NodeBehavior as GOAPGoal;
        protected sealed override void OnCleanUp()
        {
            priorityLabel.RemoveFromHierarchy();
            stateLabel.RemoveFromHierarchy();
        }
        public void SetUp(GOAPGoal goal, bool canRun, bool isCurrent)
        {
            if (canRun) SetStyle(isCurrent);
            if (goal.IsSelected) priorityLabel.text = $"Priority : Highest";
            else priorityLabel.text = $"Priority : {goal.GetPriority()}";
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("<b>Conditions</b>:");
            foreach (var state in goal.ConditionStates)
            {
                stringBuilder.Append(state.Key);
                stringBuilder.Append(":");
                stringBuilder.AppendLine(state.Value.ToString());
            }
            if (stringBuilder[^1] == '\n') stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stateLabel.text = stringBuilder.ToString();
            titleContainer.Add(priorityLabel);
            mainContainer.Add(stateLabel);
        }
        protected override void OnRestore()
        {
            if (Goal.IsBanned)
                AddToClassList("AlwaysBanned");
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (Application.isPlaying && Graph.Set is IPlanner planner)
            {
                if (Goal.IsSelected)
                    evt.menu.MenuItems().Add(new NodeMenuAction("Dropping this Goal", (a) =>
                    {
                        Graph.HeaveGoal(null);
                    }, x => DropdownMenuAction.Status.Normal));
                else
                    evt.menu.MenuItems().Add(new NodeMenuAction("Heaving this Goal", (a) =>
                    {
                        Graph.HeaveGoal(this);
                    }, x => Goal.PreconditionsSatisfied(planner.WorldState) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                if (Goal.IsBanned)
                    evt.menu.MenuItems().Add(new NodeMenuAction("Disable Always Banned", (a) =>
                    {
                        Goal.IsBanned = false;
                        RemoveFromClassList("AlwaysBanned");
                    }, x => DropdownMenuAction.Status.Normal));
                else
                    evt.menu.MenuItems().Add(new NodeMenuAction("Enable Always Banned", (a) =>
                    {
                        Goal.IsBanned = true;
                        AddToClassList("AlwaysBanned");
                    }, x => DropdownMenuAction.Status.Normal));
            }
        }
    }
}
