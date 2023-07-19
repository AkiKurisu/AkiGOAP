using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Reflection;
namespace Kurisu.GOAP.Editor
{
    [CustomEditor(typeof(GOAPWorldState),true)]
    public class GOAPWorldStateEditor : UnityEditor.Editor
    {
        private const string LabelText="AkiGOAP <size=12>V1.0</size> WorldState";
        private const string WorldState="Runtime States:";
        private const string True="<color=#5BDB14>True</color>";
        private const string False="<color=#ff2f2f>False</color>";
        private const string ButtonText="Refresh States";
        private const string Global="Global";
        private const string Local="Local";
        private VisualElement statesGroup;
        private static FieldInfo StatesInfo=typeof(GOAPStateSet).GetField("states",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Public);
        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            var state = target as GOAPWorldState;
            myInspector.Add(UIUtility.GetLabel(LabelText,20));
            InspectorElement.FillDefaultInspector(myInspector, serializedObject, this);  
            myInspector.Remove(myInspector.Q<PropertyField>("PropertyField:m_Script"));
            var localState=state?.LocalState;
            if(localState==null)return myInspector;
            var states = StatesInfo.GetValue(localState) as Dictionary<string, bool>;
            if(states==null||states.Count==0)return myInspector;
            var stateLabel=new Label(WorldState);
            stateLabel.style.fontSize=15;
            myInspector.Add(stateLabel);
            statesGroup=new VisualElement();
            myInspector.Add(statesGroup);
            var button=UIUtility.GetButton(ButtonText,UIUtility.AkiBlue,RefreshStates,100);
            button.SetEnabled(Application.isPlaying);
            myInspector.Add(button);   
            RefreshStates();
            return myInspector;
        }
        private void RefreshStates() {
            statesGroup.Clear();
            var state = target as GOAPWorldState;
            var localState=state?.LocalState;
            if(localState!=null)
            {
                var states = StatesInfo.GetValue(localState) as Dictionary<string, bool>;
                AddStates(states,false);
            }
            var globalState=state?.GlobalState;
            if(globalState!=null)
            {
                var states = StatesInfo.GetValue(globalState) as Dictionary<string, bool>;
                AddStates(states,true);
            }
        }
        private void AddStates(Dictionary<string,bool>states,bool global)
        {
            if(states==null||states.Count==0)return;
            foreach(var pair in states)
            {
                var group=GetGroup();
                group.Add(UIUtility.GetLabel(global?Global:Local,12,33,UIUtility.AkiBlue));
                group.Add(UIUtility.GetLabel(pair.Key,12,33));
                group.Add(UIUtility.GetLabel(pair.Value?True:False,12,33));
                statesGroup.Add(group);
            }
        }
        internal static VisualElement GetGroup()
        {
            var group=new VisualElement();
            group.style.flexDirection=FlexDirection.Row;
            return group;
        }
    }
}
