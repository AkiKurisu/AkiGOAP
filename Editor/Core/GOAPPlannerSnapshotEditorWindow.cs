using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/// <summary>
/// This code is modified from https://github.com/toastisme/OpenGOAP
/// </summary>
namespace Kurisu.GOAP.Editor
{
    /// <summary>
    /// class PlannerSnapshot
    //  Displays information from the GOAPPlanner as a custom editor window
    /// </summary>
    public class GOAPPlannerSnapshotEditorWindow : EditorWindow
    {
        private IPlanner planner; // The planner being visualised

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
        private float activePlanHeight = 30f;
        private float maxPriorityRectWidth;
        private float priorityRectHeight = 40f;
        private float prioritySpacing = 25f;

        // Colors
        private Color backgroundNodeColor;
        private Color actionColor;
        private Color goalColor;
        private Color runningTint;
        private Color defaultTint;
        private Color linkColor;
        private Color panelColor;

        private readonly List<PlanCache> planeCaches = new List<PlanCache>();
        private int currentIndex;
        private List<GoalData> goalData => planeCaches[currentIndex].goalData;
        private List<IAction> activePlan => planeCaches[currentIndex].activePlan;
        private IGoal activeGoal => planeCaches[currentIndex].activeGoal;
        private int activeActionIdx => planeCaches[currentIndex].activeActionIdx;
        private class PlanCache
        {
            public IGoal activeGoal;
            public List<GoalData> goalData;
            public List<IAction> activePlan;
            public int activeActionIdx;
        }
        public static void Show(IPlanner planner)
        {
            var window = GOAPPlannerSnapshotEditorWindow.GetWindow<GOAPPlannerSnapshotEditorWindow>("GOAP Planner Snapshot");
            window.SetUp(planner);
            window.Show();
        }
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Close();
                    break;
            }
        }
        public void SetUp(IPlanner planner)
        {
            this.planner = planner;
            planner.OnUpdatePlanEvent += CachePlan;
        }
        private void OnDestroy()
        {
            planner.OnUpdatePlanEvent -= CachePlan;
        }
        private void SetupPanels()
        {
            activePlanPanel = new Rect(
                0,
                0,
                position.width,
                position.height * .4f
            );
            goalPrioritiesPanel = new Rect(
                0,
                position.height * .4f,
                position.width,
                position.height
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
        private void SetupGoalPriorities()
        {
            maxPriorityRectWidth = position.width - 10f;
        }
        private void OnEnable()
        {
            SetupPanels();
            SetupActivePlanNodeParams();
            SetupGUIStyles();
            SetupColors();
            SetupGUIContent();
            SetupGoalPriorities();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        private void CachePlan(IPlanner planner)
        {
            if (IsActive())
            {
                planeCaches.Add(new PlanCache
                {
                    activeGoal = planner.ActivateGoal,
                    goalData = (planner as IPlanner).GetSortedGoalData(),
                    activePlan = new List<IAction>(planner.ActivatePlan),
                    activeActionIdx = (planner as IPlanner).ActiveActionIndex
                });
                if (planeCaches.Count > 50) planeCaches.RemoveAt(0);
            }
            Repaint();
        }
        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawActivePlanPanel();
            DrawGoalPrioritiesPanel();
            DrawToolBar();
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
                    Repaint();
                }
                int total = Mathf.Max(1, planeCaches.Count);
                GUILayout.TextField($"Cache:{currentIndex + 1}/{total}");
                if (GUILayout.Button("Next", GUILayout.MinWidth(100)))
                {
                    currentIndex++;
                    if (currentIndex >= planeCaches.Count) currentIndex = 0;
                    Repaint();
                }
                if (GUILayout.Button("Clear", GUILayout.MinWidth(100)))
                {
                    currentIndex = 0;
                    planeCaches.Clear();
                    Repaint();
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
                position.width,
                position.height * .4f
            );
            BeginWindows();
            activePlanPanel = GUILayout.Window(
                1,
                activePlanPanel,
                DrawActivePlan,
                "Active Plan",
                panelStyle
            );
            EndWindows();
        }

        private void DrawGoalPrioritiesPanel()
        {
            GUI.color = runningTint;
            GUI.backgroundColor = panelColor;
            goalPrioritiesPanel = new Rect(
                0,
                position.height * .3f,
                position.width,
                position.height
            );
            BeginWindows();
            goalPrioritiesPanel = GUILayout.Window(
                2,
                goalPrioritiesPanel,
                DrawGoalPriorities,
                "Goal Priorities",
                panelStyle
            );
            EndWindows();
        }


        private bool IsActive()
        {
            return (
                planner != null
                && planner.ActivatePlan != null
                && planner.ActivatePlan.Count > 0
            );
        }

        private void DrawActivePlan(int unusedWindowID)
        {
            DrawActionNodes(unusedWindowID);
        }
        /// <summary>
        /// Draws goals in priority order, where priority is 
        /// visualised as a progress bar 
        /// </summary>
        /// <param name="unusedWindowID"></param>
        private void DrawGoalPriorities(int unusedWindowID)
        {
            if (planeCaches.Count == 0) return;
            GUILayout.Label("\n\n");
            GUI.color = runningTint;
            GUI.backgroundColor = backgroundNodeColor;
            for (int i = 0; i < goalData.Count; i++)
            {
                GUI.Box(
                    new Rect(
                        0,
                        30f + i * prioritySpacing,
                        Mathf.Clamp(goalData[i].priority, 0.05f, 1f) * maxPriorityRectWidth,
                        priorityRectHeight
                    ),
                    goalData[i].goalName,
                    goalData[i].canRun ? goalLabelStyle : disabledGoalLabelStyle
                );
            }

        }
        /// <summary>
        /// Draws an action plan as a series of labelled Rects linked together
        /// </summary>
        /// <param name="unusedWindowID"></param>
        private void DrawActionNodes(int unusedWindowID)
        {
            if (planeCaches.Count == 0) return;
            int count = 0;
            for (int i = 0; i < activePlan.Count; i++)
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
                if (i == activeActionIdx)
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

                actionContent.text = "\nAction\n\n" + activePlan[i].Name;

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
            goalContent.text = "\nGoal\n\n" + activeGoal.Name;
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

            Vector2 startPos = new Vector2(
                startGridPos * nodeSpacing.x + nodeSize.x,
                activePlanHeight + nodeSize.y * .5f
            );

            Vector2 endPos = new Vector2(
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
        /// <summary>
        /// Background grid of the editor
        /// </summary>
        /// <param name="gridSpacing"></param>
        /// <param name="gridOpacity"></param>
        /// <param name="gridColor"></param>
        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(
                    new Vector3(gridSpacing * i, -gridSpacing, 0),
                    new Vector3(gridSpacing * i, position.height, 0f)
                );
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(
                    new Vector3(-gridSpacing, gridSpacing * j, 0),
                    new Vector3(position.width, gridSpacing * j, 0f)
                );
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}
