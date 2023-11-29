using UnityEngine;
using UnityEditor;
namespace Kurisu.GOAP.Editor
{
    public class GUIProperties
    {
        /**
         * Static methods to store general properties
         * for visualising GOAP components
         */

        //// Sizes

        public static Vector2 NodeSize() { return new Vector2(200, 70); }
        public static Vector2 TaskNodeSize() { return new Vector2(200, 60); }

        //// Positions

        public static Vector2 NodeSpacing()
        {
            Vector2 guiNodeSize = NodeSize();
            int padding = 20;
            return new Vector2(
                guiNodeSize.x + padding,
                guiNodeSize.y + padding
            );
        }

        //// Colors

        public static Color BackgroundNodeColor()
        {
            return new Color(87f / 255.0f, 117f / 255.0f, 144f / 255.0f);
        }

        public static Color GoalColor()
        {
            return new Color(0f, 1f, 1f, 0.9f);
        }
        public static Color ActionColor()
        {
            return new Color(251 / 255.0f, 114 / 255.0f, 153 / 255.0f, 0.9f);
        }

        public static Color PanelColor()
        {
            Color color = BackgroundNodeColor();
            return new Color(color[0] * .5f, color[1] * .5f, color[2] * .5f);
        }

        public static Color RunningTint()
        {
            return new Color(1f, 1f, 1f);
        }

        public static Color DefaultTint()
        {
            return new Color(.8f, .8f, .8f);
        }

        public static Color LinkColor()
        {
            return Color.white;
        }

        //// Styles

        public static GUIStyle GUINodeStyle()
        {
            GUIStyle nodeStyle = new();
            nodeStyle.normal.background = EditorGUIUtility.Load(
                "builtin skins/lightskin/images/node0.png"
                ) as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.alignment = TextAnchor.UpperLeft;
            nodeStyle.fontStyle = FontStyle.Bold;
            nodeStyle.padding = new RectOffset(10, 0, 0, 0);
            nodeStyle.fontSize = 12;
            return nodeStyle;
        }

        public static GUIStyle SelectedGUINodeStyle()
        {
            GUIStyle selectedNodeStyle = GUINodeStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load(
                "builtin skins/lightskin/images/node0 on.png"
                ) as Texture2D;
            return selectedNodeStyle;
        }

        public static GUIStyle GUIPlannerStyle()
        {
            GUIStyle nodeStyle = new();
            nodeStyle.normal.background = EditorGUIUtility.Load(
                "builtin skins/lightskin/images/node0.png"
                ) as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.alignment = TextAnchor.UpperLeft;
            nodeStyle.fontStyle = FontStyle.Bold;
            nodeStyle.padding = new RectOffset(10, 0, 10, 0);
            nodeStyle.fontSize = 10;
            return nodeStyle;
        }

        public static GUIStyle GoalLabelStyle()
        {
            GUIStyle nodeStyle = new();
            nodeStyle.normal.background = EditorGUIUtility.Load(
                "builtin skins/lightskin/images/node0.png"
                ) as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            nodeStyle.alignment = TextAnchor.MiddleLeft;
            nodeStyle.normal.textColor = Color.white;
            nodeStyle.fontStyle = FontStyle.Bold;
            nodeStyle.padding = new RectOffset(10, 0, -5, 0);
            nodeStyle.fontSize = 10;
            return nodeStyle;
        }

        public static GUIStyle DisabledGoalLabelStyle()
        {
            GUIStyle nodeStyle = GoalLabelStyle();
            nodeStyle.normal.textColor = Color.grey;
            return nodeStyle;
        }


        //// GUIContent

        public static GUIContent ActionContent()
        {
            Texture2D icon = Resources.Load("Icons/action_icon", typeof(Texture2D)) as Texture2D;
            string text = "Action";
            string tooltip = "Action for the gameobject to execute.";
            return new GUIContent(text, icon, tooltip);
        }

        public static GUIContent GoalContent()
        {
            Texture2D icon = Resources.Load("Icons/goal_icon", typeof(Texture2D)) as Texture2D;
            string text = "Goal";
            string tooltip = "The goal this plan satisfies.";
            return new GUIContent(text, icon, tooltip);
        }

    }
}