using System;
using System.Reflection;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#else
using UnityEngine.UIElements;
#endif
using UnityEngine;
namespace Kurisu.GOAP.Editor
{
    public class Vector3Resolver : FieldResolver<Vector3Field, Vector3>
    {
        public Vector3Resolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override Vector3Field CreateEditorField(FieldInfo fieldInfo)
        {
            return new Vector3Field(fieldInfo.Name);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo info) => infoType == typeof(Vector3);

    }
}