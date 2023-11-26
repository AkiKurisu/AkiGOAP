using System;
using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#else
using UnityEngine.UIElements;
#endif
namespace Kurisu.GOAP.Editor
{
    public class DoubleResolver : FieldResolver<DoubleField, double>
    {
        public DoubleResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override DoubleField CreateEditorField(FieldInfo fieldInfo)
        {
            return new DoubleField(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo info) => infoType == typeof(double);
    }
}