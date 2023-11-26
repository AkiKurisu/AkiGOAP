using System;
using System.Collections.Generic;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#else
using UnityEngine.UIElements;
#endif
namespace Kurisu.GOAP.Editor
{
    public class EnumField : PopupField<Enum>
    {
        public EnumField(string label, List<Enum> choices, Enum defaultValue = null)
            : base(label, choices, defaultValue, null, null)
        {
        }
    }
}