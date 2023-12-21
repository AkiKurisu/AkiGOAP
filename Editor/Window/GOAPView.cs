using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System;
namespace Kurisu.GOAP.Editor
{
    public class GOAPView : GraphView
    {
        private const string GraphStyleSheetPath = "AkiGOAP/Graph";
        private readonly EditorWindow editorWindow;
        public EditorWindow EditorWindow => editorWindow;
        internal Action<GOAPNode> onSelectAction;
        private readonly IGOAPSet set;
        public IGOAPSet Set => set;
        private readonly NodeResolver nodeResolver = new();
        private GOAPNodeStack goalStack;
        private GOAPNodeStack actionStack;
        private GOAPPlanStack planStack;
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
            contentDragger.activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.MiddleMouse,
            });
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(contentDragger);
            if (set is GOAPActionSet)
            {
                var searchWindow = ScriptableObject.CreateInstance<ActionSearchWindowProvider>();
                searchWindow.Init(this);
                nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }
            else if (set is GOAPGoalSet)
            {
                var searchWindow = ScriptableObject.CreateInstance<GoalSearchWindowProvider>();
                searchWindow.Init(this);
                nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }
            else
            {
                var searchWindow = ScriptableObject.CreateInstance<GOAPNodeSearchWindow>();
                searchWindow.Init(this);
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
                return e switch
                {
                    NodeMenuAction n => true,
                    DropdownMenuAction a => a.name == "Create Node" || a.name == "Delete",
                    _ => false,
                };
            });
            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
        }
        public void Restore()
        {
            goalStack = new GOAPGoalStack(this);
            actionStack = new GOAPActionStack(this);
            if (set is IPlanner planner)
            {
                //Add plan stack
                planStack = new GOAPPlanStack(this);
                planStack.SetPosition(new Rect(400, 300, 100, 100));
                AddElement(planStack);
                planner.OnUpdate += UpdateView;
                goalStack.SetPosition(new Rect(-100, 300, 100, 100));
                actionStack.SetPosition(new Rect(900, 300, 100, 100));
            }
            else
            {
                goalStack.SetPosition(new Rect(100, 300, 100, 100));
                actionStack.SetPosition(new Rect(600, 300, 100, 100));
            }
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
            if (Application.isPlaying) UpdateView(set as IPlanner);
        }
        private void UpdateView(IPlanner planner)
        {
            planStack.Query<GOAPActionNode>().ForEach(x => actionStack.AddElement(x));
            planStack.Query<GOAPGoalNode>().ForEach(x => goalStack.AddElement(x));
            var actions = actionStack.Query<GOAPActionNode>().ToList();
            actions.ForEach(x => x.CleanUp());
            var goals = goalStack.Query<GOAPGoalNode>().ToList();
            goals.ForEach(x => x.CleanUp());
            var activePlans = planner.ActivatePlan;
            var activeGoal = planner.ActivateGoal;
            if (activePlans != null && activePlans.Count != 0)
            {
                foreach (var action in activePlans)
                {
                    if (action is not GOAPAction goapAction) continue;
                    var t_Action = actions.First(x => x.GUID == goapAction.GUID);
                    t_Action.SetUp(goapAction);
                    planStack.AddElement(t_Action);
                }
            }
            if (activeGoal is not GOAPGoal goapGoal) return;
            var t_Goal = goals.First(x => x.GUID == goapGoal.GUID);
            t_Goal.SetUp(goapGoal, goapGoal.PreconditionsSatisfied(planner.WorldState), true);
            planStack.AddElement(t_Goal);
            foreach (var goal in goals)
            {
                if (goal == t_Goal) continue;
                var goalBehavior = planner.Behaviors.First(x => x.GUID == goal.GUID) as GOAPGoal;
                goal.SetUp(goalBehavior, goalBehavior.PreconditionsSatisfied(planner.WorldState), false);
            }
        }
        public void Save()
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
        public void HeaveGoal(GOAPGoalNode targetGoalNode)
        {
            var goals = goalStack.Query<GOAPGoalNode>().ToList();
            foreach (var goal in goals)
            {
                if (goal == targetGoalNode)
                {
                    goal.Goal.IsSelected = true;
                }
                else
                {
                    goal.Goal.IsSelected = false;
                }
            }
        }
    }
}
