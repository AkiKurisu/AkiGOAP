
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(GOAPGoalSet))]
    public class GOAPGoalSetEditor : UnityEditor.Editor
    {
        private const string LabelText="AkiGOAP <size=12>V1.0</size> GoalSet";
        private const string ButtonText="Open GOAP Editor";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            myInspector.styleSheets.Add(UIUtility.GetInspectorStyleSheet());
            myInspector.Add(UIUtility.GetLabel(LabelText,20));
            var description=new PropertyField(serializedObject.FindProperty("Description"),string.Empty);
            myInspector.Add(description);
            //Draw Button
            myInspector.Add(UIUtility.GetButton(ButtonText,UIUtility.AkiBlue,Open,100));   
            return myInspector;
        }
        private void Open()
        {
            GOAPEditorWindow.ShowEditorWindow(target as IGOAPSet);
        }
    }
}
