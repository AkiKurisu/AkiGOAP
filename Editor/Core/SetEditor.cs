using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public abstract class SetEditor : UnityEditor.Editor
    {
        protected abstract string LabelText { get; }
        private const string ButtonText = "Open GOAP Editor";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            myInspector.styleSheets.Add(UIElementUtility.GetInspectorStyleSheet());
            myInspector.Add(UIElementUtility.GetLabel(LabelText, 20));
            var description = new TextField(string.Empty);
            description.BindProperty(serializedObject.FindProperty("Description"));
            description.multiline = true;
            myInspector.Add(description);
            //Draw Button
            myInspector.Add(UIElementUtility.GetButton(ButtonText, UIElementUtility.AkiBlue, Open, 100));
            return myInspector;
        }
        private void Open()
        {
            GOAPEditorWindow.ShowEditorWindow(target as IGOAPSet);
        }
    }
    [CustomEditor(typeof(GOAPActionSet), true)]
    public class GOAPActionSetEditor : SetEditor
    {
        protected override string LabelText => "AkiGOAP ActionSet";
    }
    [CustomEditor(typeof(GOAPGoalSet), true)]
    public class GOAPGoalSetEditor : SetEditor
    {
        protected override string LabelText => "AkiGOAP GoalSet";
    }
    [CustomEditor(typeof(GOAPSet), true)]
    public class GOAPSetEditor : SetEditor
    {
        protected override string LabelText => "AkiGOAP GOAPSet";
    }
}
