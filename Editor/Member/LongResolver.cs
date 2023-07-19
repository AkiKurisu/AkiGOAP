using System;
using System.Reflection;
using UnityEditor.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class LongResolver : FieldResolver<LongField,long>
    {
        public LongResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override LongField CreateEditorField(FieldInfo fieldInfo)
        {
            return new LongField(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType,FieldInfo info)=>infoType == typeof(long);
    }
}