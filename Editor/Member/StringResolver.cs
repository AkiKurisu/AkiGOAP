using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class StringResolver : FieldResolver<TextField, string>
    {
        public StringResolver(FieldInfo fieldInfo) : base(fieldInfo) { }
        protected override TextField CreateEditorField(FieldInfo fieldInfo)
        {
            bool multiline = fieldInfo.GetCustomAttribute<MultilineAttribute>() != null;
            var field = new TextField(fieldInfo.Name);
            field.style.minWidth = 200;
            if (multiline)
            {
                field.multiline = true;
                field.style.maxWidth = 250;
                field.style.whiteSpace = WhiteSpace.Normal;
            }
            return field;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(string);
    }
}