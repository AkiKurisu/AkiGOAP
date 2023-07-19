using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public abstract class GOAPNode : UnityEditor.Experimental.GraphView.Node
    {
        private GOAPView bindView;
        public GOAPBehavior NodeBehavior {private set;get; }
        public string GUID{get;protected set;}
        private Type dirtyNodeBehaviorType;        
        private readonly VisualElement container;
        private readonly TextField description;
        public string Description=>description.value;
        private readonly FieldResolverFactory fieldResolverFactory;
        public readonly List<IFieldResolver> resolvers = new List<IFieldResolver>();
        public Action<GOAPNode> onSelectAction;
        public sealed override void OnSelected()
        {
            base.OnSelected();
            onSelectAction?.Invoke(this);
        }
        
        protected GOAPNode()
        {
            fieldResolverFactory = FieldResolverFactory.Instance;
            container = new VisualElement();
            description = new TextField();
            GUID=Guid.NewGuid().ToString();
            Initialize();
        }

        private void Initialize()
        {
            AddDescription();
            mainContainer.Add(this.container);
        }

        private void AddDescription()
        {
            description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(description);
        }
        public void Restore(GOAPBehavior action)
        {
            NodeBehavior = action;
            resolvers.ForEach(e => e.Restore(NodeBehavior));
            description.value = NodeBehavior.description;
            GUID=string.IsNullOrEmpty(action.GUID)?Guid.NewGuid().ToString():action.GUID;
        }
        private GOAPBehavior ReplaceBehavior()
        {
            this.NodeBehavior = Activator.CreateInstance(GetBehavior()) as GOAPBehavior;
            return NodeBehavior;
        }
        public Type GetBehavior()
        {
            return dirtyNodeBehaviorType;
        }
            
        public void Commit()
        {
            ReplaceBehavior();
            resolvers.ForEach( r => r.Commit(NodeBehavior));
            NodeBehavior.description = this.description.value;
            NodeBehavior.GUID=this.GUID;
        }
        public void SetBehavior(System.Type nodeBehavior,GOAPView view)
        {
            this.bindView=view;
            if (dirtyNodeBehaviorType != null)
            {
                dirtyNodeBehaviorType = null;
                container.Clear();
                resolvers.Clear();
            }
            dirtyNodeBehaviorType = nodeBehavior;

            nodeBehavior
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<HideInInspector>() == null)//根据Atrribute判断是否需要隐藏
                .Concat(GetAllFields(nodeBehavior))//Concat合并列表
                .Where(field => field.IsInitOnly == false)
                .ToList().ForEach((p) =>
                {
                    var fieldResolver = fieldResolverFactory.Create(p);//工厂创建暴露引用
                    var defaultValue = Activator.CreateInstance(nodeBehavior) as GOAPBehavior;
                    fieldResolver.Restore(defaultValue);
                    container.Add(fieldResolver.GetEditorField());
                    resolvers.Add(fieldResolver);
                });
            var label=nodeBehavior.GetCustomAttribute(typeof(GOAPLabelAttribute), false) as GOAPLabelAttribute;
            title = label?.Title??nodeBehavior.Name;
            if(view.Set is IPlanner)
            {
                capabilities&=~Capabilities.Copiable;
                capabilities &=~Capabilities.Deletable;
                capabilities &=~Capabilities.Movable;
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => field.GetCustomAttribute<HideInInspector>() == null).Concat(GetAllFields(t.BaseType));//Concat合并列表
        }
        public void CleanUp()
        {
            style.backgroundColor = new StyleColor(StyleKeyword.Null);
        }
        protected virtual void OnCleanUp(){}

    }
}
