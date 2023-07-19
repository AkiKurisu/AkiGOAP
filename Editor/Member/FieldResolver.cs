using System;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.GOAP.Editor
{
    sealed class Ordered : Attribute
    {
        public int Order = 100;
    }

    public interface IFieldResolver
    {
        /// <summary>
        /// 获取ValueField
        /// </summary>
        /// <param name="ownerTreeView"></param>
        /// <returns></returns>
        VisualElement GetEditorField();
        /// <summary>
        /// 只创建ValueField,不进行任何绑定
        /// </summary>
        /// <returns></returns>
        VisualElement CreateField();
        void Restore(object behavior);
        void Commit(object behavior);   
    }

    public abstract class FieldResolver<T, K> :IFieldResolver where T: BaseField<K>
    {
        private readonly FieldInfo fieldInfo;
        private T editorField;
        public object Value=>editorField.value;
        protected FieldResolver(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            SetEditorField();
        }

        private void SetEditorField()
        {
            this.editorField = this.CreateEditorField(this.fieldInfo);
            GOAPLabelAttribute label=this.fieldInfo.GetCustomAttribute<GOAPLabelAttribute>();
            if(label!=null)this.editorField.label=label.Title;
            TooltipAttribute tooltip=this.fieldInfo.GetCustomAttribute<TooltipAttribute>();
            if(tooltip!=null)this.editorField.tooltip=tooltip.tooltip;
            
        }
        protected abstract T CreateEditorField(FieldInfo fieldInfo);
        public VisualElement CreateField()=>CreateEditorField(this.fieldInfo);
        public VisualElement GetEditorField()
        {
            return this.editorField;
        }
        public void Restore(object behavior)
        {
            editorField.value = (K)fieldInfo.GetValue(behavior);
        }
        public void Commit(object behavior)
        {
           fieldInfo.SetValue(behavior, editorField.value);
        }
    }
}