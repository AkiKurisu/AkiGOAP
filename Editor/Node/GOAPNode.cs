using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public abstract class GOAPNode : Node
    {
        public GOAPBehavior NodeBehavior { private set; get; }
        public string GUID { get; protected set; }
        private Type dirtyNodeBehaviorType;
        private readonly VisualElement container;
        private readonly TextField description;
        private readonly FieldResolverFactory fieldResolverFactory;
        public readonly List<IFieldResolver> resolvers = new();
        public Action<GOAPNode> onSelectAction;
        protected GOAPView Graph { get; private set; }
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
            GUID = Guid.NewGuid().ToString();
            Initialize();
        }

        private void Initialize()
        {
            AddDescription();
            mainContainer.Add(container);
        }

        private void AddDescription()
        {
            description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(description);
        }
        public void Restore(GOAPBehavior behavior)
        {
            NodeBehavior = behavior;
            resolvers.ForEach(e => e.Restore(NodeBehavior));
            description.value = NodeBehavior.description;
            GUID = string.IsNullOrEmpty(behavior.GUID) ? Guid.NewGuid().ToString() : behavior.GUID;
            OnRestore();

        }
        protected virtual void OnRestore() { }
        private GOAPBehavior ReplaceBehavior()
        {
            NodeBehavior = Activator.CreateInstance(GetBehavior()) as GOAPBehavior;
            return NodeBehavior;
        }
        public Type GetBehavior()
        {
            return dirtyNodeBehaviorType;
        }

        public void Commit()
        {
            ReplaceBehavior();
            resolvers.ForEach(r => r.Commit(NodeBehavior));
            NodeBehavior.description = description.value;
            NodeBehavior.GUID = GUID;
        }
        public void SetBehavior(Type nodeBehavior, GOAPView view)
        {
            if (dirtyNodeBehaviorType != null)
            {
                dirtyNodeBehaviorType = null;
                container.Clear();
                resolvers.Clear();
            }
            dirtyNodeBehaviorType = nodeBehavior;

            nodeBehavior
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<HideInInspector>() == null)
                .Concat(GetAllFields(nodeBehavior))
                .Where(field => field.IsInitOnly == false)
                .ToList().ForEach((p) =>
                {
                    var fieldResolver = fieldResolverFactory.Create(p);
                    var defaultValue = Activator.CreateInstance(nodeBehavior) as GOAPBehavior;
                    fieldResolver.Restore(defaultValue);
                    container.Add(fieldResolver.GetEditorField());
                    resolvers.Add(fieldResolver);
                });
            var label = nodeBehavior.GetCustomAttribute(typeof(GOAPLabelAttribute), false) as GOAPLabelAttribute;
            title = label?.Title ?? nodeBehavior.Name;
            Graph = view;
            if (view.Set is IPlanner)
            {
                capabilities &= ~Capabilities.Copiable;
                capabilities &= ~Capabilities.Deletable;
                capabilities &= ~Capabilities.Movable;
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null)
                .Where(field => field.GetCustomAttribute<HideInInspector>() == null).Concat(GetAllFields(t.BaseType));
        }
        public void CleanUp()
        {
            RemoveFromClassList("IsRunning");
            RemoveFromClassList("IsCurrent");
            OnCleanUp();
        }
        protected virtual void OnCleanUp() { }
        protected void SetStyle(bool isCurrent)
        {
            if (isCurrent)
            {
                RemoveFromClassList("IsRunning");
                AddToClassList("IsCurrent");
            }
            else
            {
                RemoveFromClassList("IsCurrent");
                AddToClassList("IsRunning");
            }
        }
    }
}
