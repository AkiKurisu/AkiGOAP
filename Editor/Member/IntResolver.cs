using System;
using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#else
using UnityEngine.UIElements;
#endif
namespace Kurisu.GOAP.Editor
{
    public class IntResolver : FieldResolver<IntegerField, int>
    {
        public IntResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override IntegerField CreateEditorField(FieldInfo fieldInfo)
        {
            return new IntegerField(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo info) => infoType == typeof(int);
    }
}