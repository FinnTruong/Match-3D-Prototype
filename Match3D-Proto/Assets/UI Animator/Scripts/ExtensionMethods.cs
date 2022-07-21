using UnityEngine;
using System.Reflection;

using UnityEngine.UI;

namespace UIAnimatorCore
{
    public static class ExtensionMethods
    {
        public static bool DrivenByLayoutGroup(this RectTransform rectTransform)
        {
#if CSHARP_7_3_OR_NEWER
            PropertyInfo drivenProp = typeof(RectTransform).GetProperty("drivenByObject", BindingFlags.NonPublic|BindingFlags.Instance);

            if(drivenProp == null)
            {
                return false;
            }

            object drivenByObject = drivenProp.GetValue(rectTransform);

            return drivenByObject != null && !(drivenByObject is ContentSizeFitter);
#else
            return false;
#endif
        }
    }
}