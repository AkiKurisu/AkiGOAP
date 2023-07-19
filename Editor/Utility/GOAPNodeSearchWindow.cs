using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPNodeSearchWindow : ScriptableObject, ISearchWindowProvider 
    {
        private Texture2D _indentationIcon;
        private GOAPView view;
        private EditorWindow editorWindow;
        private readonly NodeResolver nodeResolver = new NodeResolver();
        public void Init(EditorWindow editorWindow,GOAPView view)
        {
            this.editorWindow=editorWindow;
            this.view=view;
            _indentationIcon=new Texture2D(1,1);
            _indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
            _indentationIcon.Apply();
        }
        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            Dictionary<string,List<Type>> attributeDict=new Dictionary<string, List<Type>>();

            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select GOAP Node"),0));
            AddTypeEntry(typeof(GOAPGoal),entries);
            AddTypeEntry(typeof(GOAPAction),entries);
            return entries;
        }
        private void AddTypeEntry(Type addType,List<SearchTreeEntry> entries)
        {
            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {addType.Name}"),1));
            List<Type> nodeTypes =SearchUtility.FindSubClassTypes(addType);
            var groups=nodeTypes.GroupsByGroup();
            nodeTypes=nodeTypes.Except(groups.SelectMany(x=>x)).ToList();
            foreach(var group in groups)
            {
                entries.AddAllEntries(group,_indentationIcon,2);
            }
            foreach(Type type in nodeTypes)
            {
                entries.AddEntry(type,2,_indentationIcon);
            }
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var localMousePosition = view.contentViewContainer.WorldToLocal(worldMousePosition);
            Rect newRect=new Rect(localMousePosition, new Vector2(100, 100));
            var type = searchTreeEntry.userData as Type;
            var node = this.nodeResolver.CreateNodeInstance(type,view);
            node.SetPosition(newRect);
            view.AddElement(node);
            node.onSelectAction=view.onSelectAction;
            return true;
        }    
    }
    public class GOAPNodeSearchWindow<T,K> : ScriptableObject, ISearchWindowProvider where T:class,K
    {
        private Texture2D _indentationIcon;
        private GOAPView view;
        private EditorWindow editorWindow;
        private readonly NodeResolver nodeResolver = new NodeResolver();
        public void Init(EditorWindow editorWindow,GOAPView view)
        {
            this.editorWindow=editorWindow;
            this.view=view;
            _indentationIcon=new Texture2D(1,1);
            _indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
            _indentationIcon.Apply();
        }
        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            Dictionary<string,List<Type>> attributeDict=new Dictionary<string, List<Type>>();

            entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {typeof(T).Name}"),0));
            List<Type> nodeTypes =SearchUtility.FindSubClassTypes(typeof(T));
            var groups=nodeTypes.GroupsByGroup();
            nodeTypes=nodeTypes.Except(groups.SelectMany(x=>x)).ToList();
            foreach(var group in groups)
            {
                entries.AddAllEntries(group,_indentationIcon,1);
            }
            foreach(Type type in nodeTypes)
            {
                entries.AddEntry(type,1,_indentationIcon);
            }
            return entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var localMousePosition = view.contentViewContainer.WorldToLocal(worldMousePosition);
            Rect newRect=new Rect(localMousePosition, new Vector2(100, 100));
            var type = searchTreeEntry.userData as Type;
            var node = this.nodeResolver.CreateNodeInstance(type,view);
            node.SetPosition(newRect);
            view.AddElement(node);
            node.onSelectAction=view.onSelectAction;
            return true;
        }    
    }
    public sealed class GoalSearchWindowProvider : GOAPNodeSearchWindow<GOAPGoal,IGoal>
    {
    }
    public sealed class ActionSearchWindowProvider : GOAPNodeSearchWindow<GOAPAction,IAction>
    {
    }
}
