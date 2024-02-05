using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Reflection;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(WorldState), true)]
    public class WorldStateEditor : UnityEditor.Editor
    {
        private const string LabelText = "AkiGOAP WorldState";
        private const string WorldState = "Runtime States:";
        private const string True = "<color=#5BDB14>True</color>";
        private const string False = "<color=#ff2f2f>False</color>";
        private const string Global = "Global";
        private const string Local = "Local";
        private VisualElement statesGroup;
        private VisualElement myInspector;
        private static readonly FieldInfo StatesInfo = typeof(GOAPStateSet)
                                                        .GetField("states", BindingFlags.NonPublic | BindingFlags.Instance);
        public override VisualElement CreateInspectorGUI()
        {
            myInspector = new VisualElement();
            var state = target as WorldState;
            myInspector.Add(UIElementUtility.GetLabel(LabelText, 20));
            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:m_Script"));
            if (Application.isPlaying)
            {
                var localState = state != null ? state.LocalState : null;
                var stateLabel = new Label(WorldState);
                stateLabel.style.fontSize = 15;
                myInspector.Add(stateLabel);
                statesGroup = new VisualElement();
                myInspector.Add(statesGroup);
                if (localState == null) return myInspector;
                if (StatesInfo.GetValue(localState) is not Dictionary<string, bool> states || states.Count == 0) return myInspector;
                RefreshStates();
            }
            return myInspector;
        }
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            var state = target as WorldState;
            state.OnUpdate += RefreshStates;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            var state = target as WorldState;
            state.OnUpdate -= RefreshStates;
        }
        private void RefreshStates()
        {
            if (statesGroup == null)
            {
                Repaint();
                return;
            }
            statesGroup.Clear();
            var state = target as WorldState;
            var localState = state != null ? state.LocalState : null;
            if (localState != null)
            {
                var states = StatesInfo.GetValue(localState) as Dictionary<string, bool>;
                AddStates(states, false);
            }
            var globalState = state != null ? state.GlobalState : null;
            if (globalState != null)
            {
                var states = StatesInfo.GetValue(globalState) as Dictionary<string, bool>;
                AddStates(states, true);
            }
        }
        private void AddStates(Dictionary<string, bool> states, bool global)
        {
            if (states == null || states.Count == 0) return;
            foreach (var pair in states)
            {
                var group = GetGroup();
                group.Add(UIElementUtility.GetLabel(global ? Global : Local, 12, 33, UIElementUtility.AkiBlue));
                group.Add(UIElementUtility.GetLabel(pair.Key, 12, 33));
                group.Add(UIElementUtility.GetLabel(pair.Value ? True : False, 12, 33));
                statesGroup.Add(group);
            }
        }
        internal static VisualElement GetGroup()
        {
            var group = new VisualElement();
            group.style.flexDirection = FlexDirection.Row;
            return group;
        }
    }
}
