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
        private const string LabelText = "AkiGOAP <size=12>V1.1.2</size> Planner";
        private const string GraphButtonText = "Open GOAP Editor";
        private const string BackendTooltip = "Select planner path execution backend, " +
        "recommend using Normal Backend for simple job and using JobSystem Backend for complex job";
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            //Title
            myInspector.Add(UIElementUtility.GetLabel(LabelText, 20));
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
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:backendType"));
            //Setting
            UIElementUtility.GetLabel("Normal Setting", 14, color: UIElementUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            myInspector.Q<PropertyField>("PropertyField:logType").MoveToEnd(myInspector);
            myInspector.Q<PropertyField>("PropertyField:tickType").MoveToEnd(myInspector);
            myInspector.AddSpace();
            UIElementUtility.GetLabel("Advanced Setting", 14, color: UIElementUtility.AkiBlue, anchor: TextAnchor.MiddleLeft).AddTo(myInspector);
            myInspector.Q<PropertyField>("PropertyField:isActive").MoveToEnd(myInspector);
            myInspector.Q<PropertyField>("PropertyField:searchMode").MoveToEnd(myInspector);
            //Editor Bottom
            UIElementUtility.GetButton(GraphButtonText, UIElementUtility.AkiBlue, ShowGOAPEditor, 100)
                .Enabled(Application.isPlaying)
                .AddTo(myInspector);
            return myInspector;
        }
        private void OnBackendChanged(PlannerBackend newBackend)
        {
            serializedObject.FindProperty("backendType").enumValueIndex = (int)newBackend;
            serializedObject.ApplyModifiedProperties();
        }
        private void ShowGOAPEditor()
        {
            GOAPEditorWindow.ShowEditorWindow(target as IGOAPSet);
        }
    }
}
