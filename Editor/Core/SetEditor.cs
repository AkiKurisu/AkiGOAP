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
            myInspector.styleSheets.Add(UIUtility.GetInspectorStyleSheet());
            myInspector.Add(UIUtility.GetLabel(LabelText, 20));
            var description = new TextField(string.Empty);
            description.BindProperty(serializedObject.FindProperty("Description"));
            description.multiline = true;
            myInspector.Add(description);
            //Draw Button
            myInspector.Add(UIUtility.GetButton(ButtonText, UIUtility.AkiBlue, Open, 100));
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
        protected override string LabelText => "AkiGOAP <size=12>V1.1.1</size> ActionSet";
    }
    [CustomEditor(typeof(GOAPGoalSet), true)]
    public class GOAPGoalSetEditor : SetEditor
    {
        protected override string LabelText => "AkiGOAP <size=12>V1.1.1</size> GoalSet";
    }
    [CustomEditor(typeof(GOAPSet), true)]
    public class GOAPSetEditor : SetEditor
    {
        protected override string LabelText => "AkiGOAP <size=12>V1.1.1</size> GOAPSet";
    }
}
