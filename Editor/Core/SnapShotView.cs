using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
/// <summary>
/// This code is modified from https://github.com/toastisme/OpenGOAP
/// </summary>
namespace Kurisu.GOAP.Editor
{
    /// <summary>
    //  Displays information from the GOAPPlanner
    //  1.1.1 Changed: Covert dedicated editor window to IMGUI container that can be added in graph editor
    /// </summary>
    public class SnapshotView : VisualElement
    {
        private readonly IPlanner planner; // The planner being visualised
        private Rect activePlanPanel;
        private Rect goalPrioritiesPanel;

        // Active plan node params
        private Vector2 nodeSpacing;
        private Vector2 nodeSize;
        private Vector2 taskNodeSize;

        // Styles
        private GUIStyle guiNodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle activeNodeStyle;
        private GUIStyle panelStyle;
        private GUIStyle goalLabelStyle;
        private GUIStyle disabledGoalLabelStyle;

        // GUIContent
        private GUIContent actionContent;
        private GUIContent goalContent;

        // Positions
        private readonly float activePlanHeight = 20f;
        private readonly float priorityRectHeight = 30f;
        private readonly float prioritySpacing = 20f;

        // Colors
        private Color backgroundNodeColor;
        private Color actionColor;
        private Color goalColor;
        private Color runningTint;
        private Color defaultTint;
        private Color linkColor;
        private Color panelColor;
        private readonly List<PlanCache> planeCaches = new(50);
        private int currentIndex;
        private List<GoalData> GoalData => planeCaches[currentIndex].goalData;
        private List<IAction> ActivePlan => planeCaches[currentIndex].activePlan;
        private IGoal ActiveGoal => planeCaches[currentIndex].activeGoal;
        private int ActiveActionIdx => planeCaches[currentIndex].activeActionIdx;
        private const int contentHeight = 250;
        private float maxPriority = 1f;
        private class PlanCache
        {
            public IGoal activeGoal;
            public List<GoalData> goalData;
            public List<IAction> activePlan;
            public int activeActionIdx;
        }
        public SnapshotView(IPlanner planner)
        {
            this.planner = planner;
            planner.OnUpdate += CachePlan;
            //Re-Register if editor window changed
            RegisterCallback<AttachToPanelEvent>((evt) => { planner.OnUpdate -= CachePlan; planner.OnUpdate += CachePlan; });
            RegisterCallback<DetachFromPanelEvent>((evt) => planner.OnUpdate -= CachePlan);
            Add(GetIMGUIContainer());
            SetupPanels();
            SetupActivePlanNodeParams();
            SetupGUIStyles();
            SetupColors();
            SetupGUIContent();
        }
        private IMGUIContainer GetIMGUIContainer()
        {
            var container = new IMGUIContainer(() =>
            {
                DrawActivePlanPanel();
                DrawGoalPrioritiesPanel();
                DrawToolBar();
            });
            container.style.height = contentHeight;
            return container;
        }
        private void SetupPanels()
        {
            activePlanPanel = new Rect(
                0,
                0,
                contentRect.width,
                contentHeight * .4f
            );
            goalPrioritiesPanel = new Rect(
                0,
                contentHeight * .4f,
                contentRect.width,
                contentHeight * .6f
            );
        }

        private void SetupActivePlanNodeParams()
        {
            nodeSpacing = GUIProperties.NodeSpacing();
            nodeSize = GUIProperties.NodeSize();
            taskNodeSize = GUIProperties.TaskNodeSize();
        }

        private void SetupGUIStyles()
        {
            guiNodeStyle = GUIProperties.GUINodeStyle();
            selectedNodeStyle = GUIProperties.SelectedGUINodeStyle();
            activeNodeStyle = guiNodeStyle;
            panelStyle = GUIProperties.GUIPlannerStyle();
            goalLabelStyle = GUIProperties.GoalLabelStyle();
            disabledGoalLabelStyle = GUIProperties.DisabledGoalLabelStyle();
        }

        private void SetupColors()
        {
            backgroundNodeColor = GUIProperties.BackgroundNodeColor();
            actionColor = GUIProperties.ActionColor();
            goalColor = GUIProperties.GoalColor();
            runningTint = GUIProperties.RunningTint();
            defaultTint = GUIProperties.DefaultTint();
            linkColor = GUIProperties.LinkColor();
            panelColor = GUIProperties.PanelColor();
        }
        private void SetupGUIContent()
        {
            actionContent = GUIProperties.ActionContent();
            goalContent = GUIProperties.GoalContent();
        }
        private void CachePlan(IPlanner planner)
        {
            if (IsActive())
            {
                if (planeCaches.Count >= 50) planeCaches.RemoveAt(0);
                planeCaches.Add(new PlanCache
                {
                    activeGoal = planner.ActivateGoal,
                    goalData = planner.GetSortedGoalData(),
                    activePlan = new List<IAction>(planner.ActivatePlan),
                    activeActionIdx = planner.ActiveActionIndex
                });
            }
        }
        private void DrawToolBar()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = planeCaches.Count != 0;
                if (GUILayout.Button("Last", GUILayout.MinWidth(100)))
                {
                    currentIndex--;
                    if (currentIndex < 0) currentIndex = planeCaches.Count - 1;
                }
                int total = Mathf.Max(1, planeCaches.Count);
                GUILayout.TextField($"Cache:{currentIndex + 1}/{total}");
                if (GUILayout.Button("Next", GUILayout.MinWidth(100)))
                {
                    currentIndex++;
                    if (currentIndex >= planeCaches.Count) currentIndex = 0;
                }
                if (GUILayout.Button("Clear", GUILayout.MinWidth(100)))
                {
                    currentIndex = 0;
                    planeCaches.Clear();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        private void DrawActivePlanPanel()
        {
            GUI.color = runningTint;
            GUI.backgroundColor = panelColor;
            activePlanPanel = new Rect(
                0,
                0,
                contentRect.width,
                contentHeight * .43f
            );
            GUILayout.BeginArea(activePlanPanel, "Active Plan", panelStyle);
            DrawActionNodes();
            GUILayout.EndArea();
        }

        private void DrawGoalPrioritiesPanel()
        {
            GUI.color = runningTint;
            GUI.backgroundColor = panelColor;
            goalPrioritiesPanel = new Rect(
                0,
                contentHeight * .4f,
                contentRect.width,
                contentHeight * .6f
            );
            GUILayout.BeginArea(goalPrioritiesPanel, "Goal Priorities", panelStyle);
            DrawGoalPriorities();
            GUILayout.EndArea();
        }


        private bool IsActive()
        {
            return (
                planner != null
                && planner.ActivatePlan != null
                && planner.ActivatePlan.Count > 0
            );
        }

        /// <summary>
        /// Draws goals in priority order, where priority is 
        /// visualized as a progress bar 
        /// </summary>
        private void DrawGoalPriorities()
        {
            if (planeCaches.Count == 0) return;
            float maxPriorityRectWidth = contentRect.width * 0.8f;
            GUILayout.Label("\n\n");
            GUI.color = runningTint;
            GUI.backgroundColor = backgroundNodeColor;
            float max = maxPriority;
            for (int i = 0; i < GoalData.Count; i++)
            {
                if (GoalData[i].priority > max) max = GoalData[i].priority;
                GUI.Box(
                    new Rect(
                        0,
                        30f + i * prioritySpacing,
                        Mathf.Clamp(GoalData[i].priority / maxPriority, 0.05f, 1f) * maxPriorityRectWidth,
                        priorityRectHeight
                    ),
                    GoalData[i].goalName,
                    GoalData[i].canRun ? goalLabelStyle : disabledGoalLabelStyle
                );
            }
            maxPriority = max;
        }
        /// <summary>
        /// Draws an action plan as a series of labelled Rects linked together
        /// </summary>
        private void DrawActionNodes()
        {
            if (planeCaches.Count == 0) return;
            int count = 0;
            for (int i = 0; i < ActivePlan.Count; i++)
            {

                // Draw link
                if (count > 0)
                {
                    DrawLink(
                        count - 1,
                        count,
                        linkColor
                    );
                }

                GUI.color = defaultTint;
                GUI.backgroundColor = backgroundNodeColor;
                if (i == ActiveActionIdx)
                {
                    GUI.color = runningTint;
                    activeNodeStyle = selectedNodeStyle;
                }
                else
                {
                    activeNodeStyle = guiNodeStyle;
                }
                // Draw node rect
                GUI.Box(
                    GetNodeRect(count),
                    "",
                    activeNodeStyle);

                actionContent.text = "\nAction\n" + ActivePlan[i].Name;

                // Draw task rect
                GUI.backgroundColor = actionColor;
                GUI.Box(
                    GetTaskRect(count),
                    actionContent,
                    activeNodeStyle);

                count++;
            }

            // Draw goal
            DrawLink(
                count - 1,
                count,
                linkColor
            );
            GUI.color = runningTint;
            GUI.backgroundColor = backgroundNodeColor;
            GUI.Box(
                GetNodeRect(count),
                "",
                selectedNodeStyle);
            GUI.backgroundColor = goalColor;
            goalContent.text = "\nGoal\n" + ActiveGoal.Name;
            GUI.Box(
                GetTaskRect(count),
                goalContent,
                selectedNodeStyle);
        }


        private Rect GetNodeRect(int gridPos)
        {
            return new Rect(
                gridPos * nodeSpacing.x,
                activePlanHeight,
                nodeSize.x,
                nodeSize.y
            );
        }

        private Rect GetTaskRect(int gridPos)
        {
            return new Rect(
                gridPos * nodeSpacing.x,
                activePlanHeight + taskNodeSize.y * .05f,
                taskNodeSize.x,
                taskNodeSize.y
            );
        }
        /// <summary>
        /// Links between nodes in the active plan
        /// </summary>
        /// <param name="startGridPos"></param>
        /// <param name="endGridPos"></param>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        private void DrawLink(
            int startGridPos,
            int endGridPos,
            Color color,
            float thickness = 4f)
        {

            Vector2 startPos = new(
                startGridPos * nodeSpacing.x + nodeSize.x,
                activePlanHeight + nodeSize.y * .5f
            );

            Vector2 endPos = new(
                endGridPos * nodeSpacing.x,
                activePlanHeight + nodeSize.y * .5f
            );

            Handles.DrawBezier(
                startPos,
                endPos,
                startPos,
                endPos,
                color,
                null,
                thickness
            );
        }
    }
}
