using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(GOAPPlannerPro), true)]
    public class GOAPPlannerProEditor : UnityEditor.Editor
    {
        private const string LabelText = "AkiGOAP <size=12>V1.1.1</size> Planner Pro";
        private const string ButtonText = "Open Planner Snapshot";
        private const string GraphButtonText = "Open GOAP Editor";
        private const string IsActiveTooltip = "Whether current planner is active, will be disbled automatically" +
        " when skipSearchWhenActionRunning is on";
        private const string SkilSearchTooltip = "Enabled to skip search plan when already have an action, enable this will need you to set correct precondition" +
        " for each action to let it quit by itself";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            myInspector.Add(UIUtility.GetLabel(LabelText, 20));
            //Default
            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:m_Script"));
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:skipSearchWhenActionRunning"));
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:isActive"));
            //Setting
            myInspector.AddSpace();
            UIUtility.GetLabel("Normal Setting", 14, color: UIUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            myInspector.Q<PropertyField>("PropertyField:logType").MoveToEnd(myInspector);
            myInspector.Q<PropertyField>("PropertyField:tickType").MoveToEnd(myInspector);
            var isActive = new Toggle("Is Active")
            {
                tooltip = IsActiveTooltip
            };
            isActive.BindProperty(serializedObject.FindProperty("isActive"));
            isActive.AddTo(myInspector);
            myInspector.AddSpace();
            UIUtility.GetLabel("Pro Setting", 14, color: UIUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            var skipSearchProperty = serializedObject.FindProperty("skipSearchWhenActionRunning");
            var skipSearchToggle = new Toggle("Skip Search When Action Running")
            {
                tooltip = SkilSearchTooltip
            };
            skipSearchToggle.BindProperty(skipSearchProperty);
            skipSearchToggle.AddTo(myInspector);
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
