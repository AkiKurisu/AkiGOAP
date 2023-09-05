using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(GOAPPlanner), true)]
    public class GOAPPlannerEditor : UnityEditor.Editor
    {
        private const string LabelText = "AkiGOAP <size=12>V1.1</size> Planner";
        private const string ButtonText = "Open Planner Snapshot";
        private const string GraphButtonText = "Open GOAP Editor";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            myInspector.Add(UIUtility.GetLabel(LabelText, 20));
            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:m_Script"));
            //Setting
            myInspector.AddSpace();
            UIUtility.GetLabel("Normal Setting", 14, color: UIUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            myInspector.Q<PropertyField>("PropertyField:logType").MoveToEnd(myInspector);
            myInspector.Q<PropertyField>("PropertyField:tickType").MoveToEnd(myInspector);
            //SnapShot
            UIUtility.GetButton(ButtonText, UIUtility.AkiRed, ShowPlannerWindow, 100)
                .Enabled(Application.isPlaying)
                .AddTo(myInspector);
            //Editor
            UIUtility.GetButton(GraphButtonText, UIUtility.AkiBlue, ShowGOAPEditor, 100)
                .Enabled(Application.isPlaying)
                .AddTo(myInspector);
            return myInspector;
        }
        private void ShowPlannerWindow()
        {
            GOAPPlannerSnapshotEditorWindow.Show(target as IPlanner);
        }
        private void ShowGOAPEditor()
        {
            GOAPEditorWindow.ShowEditorWindow(target as IGOAPSet);
        }
    }
}
