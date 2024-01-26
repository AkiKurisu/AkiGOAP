using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    internal class UIElementUtility
    {
        internal static Button GetButton(string text, Color? color = null, System.Action callBack = null, float widthPercent = 50, float fontSize = 15)
        {
            var button = new Button();
            if (callBack != null)
                button.clicked += callBack;
            if (color.HasValue)
                button.style.backgroundColor = color.Value;
            button.style.width = Length.Percent(widthPercent);
            button.text = text;
            button.style.fontSize = fontSize;
            return button;
        }
        internal static Label GetLabel(string text, int frontSize, float? widthPercent = null, Color? color = null, TextAnchor? anchor = TextAnchor.MiddleCenter)
        {
            var label = new Label(text);
            label.style.fontSize = frontSize;
            if (widthPercent.HasValue)
                label.style.width = Length.Percent(widthPercent.Value);
            if (color.HasValue)
                label.style.color = color.Value;
            if (anchor.HasValue)
                label.style.unityTextAlign = anchor.Value;
            return label;
        }
        internal static Color AkiBlue = new(140 / 255f, 160 / 255f, 250 / 255f);
        internal static Color AkiRed = new(253 / 255f, 163 / 255f, 255 / 255f);
        private const string InspectorStyleSheetPath = "AkiGOAP/Inspector";
        internal static StyleSheet GetInspectorStyleSheet() => Resources.Load<StyleSheet>(InspectorStyleSheetPath);
    }
    public static class UIElementExtension
    {
        /// <summary>
        /// Add Space to target VisualElement by adding empty element
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="height"></param>
        public static VisualElement AddSpace(this VisualElement parent, int height = 10)
        {
            var space = new VisualElement();
            space.style.height = height;
            space.AddTo(parent);
            return parent;
        }
        public static VisualElement AddTo(this VisualElement child, VisualElement parent)
        {
            parent.Add(child);
            return child;
        }
        public static VisualElement Enabled(this VisualElement child, bool enabled)
        {
            child.SetEnabled(enabled);
            return child;
        }
        public static VisualElement MoveToEnd(this VisualElement child, VisualElement parent)
        {
            parent.Remove(child);
            parent.Add(child);
            return child;
        }
    }
}
