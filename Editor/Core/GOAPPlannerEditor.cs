using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(GOAPPlanner), true)]
    public class GOAPPlannerEditor : UnityEditor.Editor
    {
        private const string LabelText = "AkiGOAP <size=12>V1.1.1</size> Planner";
        private const string GraphButtonText = "Open GOAP Editor";
        private const string IsActiveTooltip = "Whether current planner is active, will be disbled automatically" +
        " when skipSearchWhenActionRunning is on";
        private const string SkilSearchTooltip = "Enabled to skip search plan when already have an action, enable this will need you to set correct precondition" +
        " for each action to let it quit by itself";
        private const string BackendTooltip = "Select planner path execution backend, " +
        "recommand using Normal Backend for simple job and using JobSystem Backend for including distance caculation and complex job";
        private VisualElement backendView;
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            //Title
            myInspector.Add(UIUtility.GetLabel(LabelText, 20));
            //Backend
            myInspector.AddSpace();
            var backendType = serializedObject.FindProperty("backendType");
            var enumValue = Enum.GetValues(typeof(PlannerBackend)).Cast<Enum>().Select(v => v).ToList();
            var backend = new EnumField("Backend", enumValue, (PlannerBackend)backendType.enumValueIndex)
            {
                tooltip = BackendTooltip
            };
            if (Application.isPlaying)
            {
                backend.SetEnabled(false);
            }
            else
            {
                backend.RegisterValueChangedCallback((e) => OnBackendChanged((PlannerBackend)e.newValue));
            }
            myInspector.Add(backend);
            //Default
            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:m_Script"));
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:skipSearchWhenActionRunning"));
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:isActive"));
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:backendType"));
            //Setting
            UIUtility.GetLabel("Normal Setting", 14, color: UIUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            myInspector.Q<PropertyField>("PropertyField:logType").MoveToEnd(myInspector);
            myInspector.Q<PropertyField>("PropertyField:tickType").MoveToEnd(myInspector);
            backendView = new VisualElement();
            //Pro Setting
            if ((PlannerBackend)backendType.enumValueIndex == PlannerBackend.JobSystem)
            {
                DrawJobSystemsBackend(backendView);
            }
            myInspector.Add(backendView);
            //Editor Buttpm
            UIUtility.GetButton(GraphButtonText, UIUtility.AkiBlue, ShowGOAPEditor, 100)
                .Enabled(Application.isPlaying)
                .AddTo(myInspector);
            return myInspector;
        }
        private void DrawJobSystemsBackend(VisualElement myInspector)
        {
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
        }
        private void OnBackendChanged(PlannerBackend newBackend)
        {
            serializedObject.FindProperty("backendType").enumValueIndex = (int)newBackend;
            serializedObject.ApplyModifiedProperties();
            //Repaint Editor
            backendView.Clear();
            if (newBackend == PlannerBackend.JobSystem)
            {
                DrawJobSystemsBackend(backendView);
            }
        }
        private void ShowGOAPEditor()
        {
            GOAPEditorWindow.ShowEditorWindow(target as IGOAPSet);
        }
    }
}
