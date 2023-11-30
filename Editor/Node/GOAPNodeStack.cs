using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPNodeStack : StackNode
    {
        public GOAPView GraphView { get; }
        public GOAPNodeStack(GOAPView graphView)
        {
            GraphView = graphView;
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.Center;
        }
    }
    public class GOAPActionStack : GOAPNodeStack
    {
        public GOAPActionStack(GOAPView graphView) : base(graphView)
        {
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new DropdownMenuAction("Add Action", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ActionSearchWindowProvider>();
                provider.Init(GraphView);
                provider.OnNodeCreated += (x) => AddElement(x);
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }, x => DropdownMenuAction.Status.Normal));
        }
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is GOAPActionNode actionNode)
            {
                var behaviorType = actionNode.GetBehavior();
                return contentContainer.Query<GOAPActionNode>().ToList().FirstOrDefault(x => x.GetBehavior() == behaviorType) == null;
            }
            return false;
        }
    }
    public class GOAPGoalStack : GOAPNodeStack
    {
        public GOAPGoalStack(GOAPView graphView) : base(graphView)
        {
        }
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new DropdownMenuAction("Add Goal", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<GoalSearchWindowProvider>();
                provider.Init(GraphView);
                provider.OnNodeCreated += (x) => AddElement(x);
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }, x => DropdownMenuAction.Status.Normal));
        }
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is GOAPGoalNode actionNode)
            {
                var behaviorType = actionNode.GetBehavior();
                return contentContainer.Query<GOAPGoalNode>().ToList().FirstOrDefault(x => x.GetBehavior() == behaviorType) == null;
            }
            return false;
        }
    }
}
