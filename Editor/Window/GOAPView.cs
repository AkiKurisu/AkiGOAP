using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
namespace Kurisu.GOAP.Editor
{
    public class GOAPView : GraphView
    {
        private const string GraphStyleSheetPath = "AkiGOAP/Graph";
        private const string GoalIconPath = "Icons/goal_icon";
        private const string ActionIconPath = "Icons/action_icon";
        private readonly EditorWindow editorWindow;
        internal System.Action<GOAPNode> onSelectAction;
        private readonly IGOAPSet set;
        public IGOAPSet Set => set;
        private readonly NodeResolver nodeResolver = new NodeResolver();
        private GOAPNodeStack goalStack;
        private GOAPNodeStack actionStack;
        public GOAPView(EditorWindow editor, IGOAPSet set)
        {
            this.set = set;
            editorWindow = editor;
            style.flexGrow = 1;
            style.flexShrink = 1;
            styleSheets.Add(Resources.Load<StyleSheet>(GraphStyleSheetPath));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Insert(0, new GridBackground());

            var contentDragger = new ContentDragger();
            //鼠标中键移动
            contentDragger.activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.MiddleMouse,
            });
            // 添加选框
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(contentDragger);
            if (set is GOAPActionSet)
            {
                var searchWindow = ScriptableObject.CreateInstance<ActionSearchWindowProvider>();
                searchWindow.Init(editorWindow, this);
                nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }
            else if (set is GOAPGoalSet)
            {
                var searchWindow = ScriptableObject.CreateInstance<GoalSearchWindowProvider>();
                searchWindow.Init(editorWindow, this);
                nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }
            else
            {
                var searchWindow = ScriptableObject.CreateInstance<GOAPNodeSearchWindow>();
                searchWindow.Init(editorWindow, this);
                nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }

        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                switch (e)
                {
                    case DropdownMenuAction a: return a.name == "Create Node" || a.name == "Delete";
                    default: return false;
                }
            });
            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
        }
        internal void Restore()
        {
            goalStack = new GOAPGoalStack();
            goalStack.SetPosition(new Rect(100, 300, 100, 100));
            goalStack.headerContainer.Add(new Image() { image = Resources.Load<Texture2D>(GoalIconPath) });
            goalStack.headerContainer.Add(new Label("   GOAP Goal Stack"));
            actionStack = new GOAPActionStack();
            actionStack.SetPosition(new Rect(600, 300, 100, 100));
            actionStack.headerContainer.Add(new Image() { image = Resources.Load<Texture2D>(ActionIconPath) });
            actionStack.headerContainer.Add(new Label("   GOAP Action Stack"));
            AddElement(goalStack);
            AddElement(actionStack);
            if (set is GOAPActionSet)
            {
                goalStack.SetEnabled(false);
            }
            if (set is GOAPGoalSet)
            {
                actionStack.SetEnabled(false);
            }
            foreach (var behavior in set.Behaviors)
            {
                if (behavior == null) continue;
                var node = nodeResolver.CreateNodeInstance(behavior.GetType(), this);
                node.Restore(behavior);
                if (node is GOAPActionNode) actionStack.AddElement(node);
                else goalStack.AddElement(node);
                node.onSelectAction = onSelectAction;
            }
            if (set is IPlanner)
            {
                IPlanner planner = set as IPlanner;
                planner.OnUpdatePlanEvent += UpdateView;
            }
        }
        private void UpdateView(IPlanner planner)
        {
            var actions = actionStack.Query<GOAPNode>().ToList();
            actions.ForEach(x => x.CleanUp());
            var goals = goalStack.Query<GOAPNode>().ToList();
            goals.ForEach(x => x.CleanUp());
            var activePlans = planner.ActivatePlan;
            var activeGoal = planner.ActivateGoal;
            foreach (var action in activePlans)
            {
                if (action is not GOAPAction goapAction) continue;
                var t_Action = actions.First(x => x.GUID == goapAction.GUID);
                (t_Action as GOAPActionNode).SetUp(goapAction.GetCost());
            }
            if (activeGoal is not GOAPGoal goapGoal) return;
            var t_Goal = goals.First(x => x.GUID == goapGoal.GUID);
            (t_Goal as GOAPGoalNode).SetUp(goapGoal.GetPriority(), goapGoal.PreconditionsSatisfied(planner.WorldState), true);
            foreach (var goal in goals)
            {
                if (goal == t_Goal) continue;
                var goalBehavior = planner.Behaviors.First(x => x.GUID == goal.GUID) as GOAPGoal;
                (goal as GOAPGoalNode).SetUp(goalBehavior.GetPriority(), goalBehavior.PreconditionsSatisfied(planner.WorldState), false);
            }
        }
        internal void Save()
        {
            if (Application.isPlaying) return;
            set.Behaviors.Clear();
            IEnumerable<GOAPNode> list;
            if (set is GOAPActionSet)
            {
                list = actionStack.Query<GOAPNode>().ToList();
            }
            else if (set is GOAPGoalSet)
            {
                list = goalStack.Query<GOAPNode>().ToList();
            }
            else list = goalStack.Query<GOAPNode>().ToList().Concat(actionStack.Query<GOAPNode>().ToList());
            foreach (var node in list)
            {
                node.Commit();
                set.Behaviors.Add(node.NodeBehavior);
            }
            EditorUtility.SetDirty(set.Object);
            AssetDatabase.SaveAssets();
        }
    }
}
