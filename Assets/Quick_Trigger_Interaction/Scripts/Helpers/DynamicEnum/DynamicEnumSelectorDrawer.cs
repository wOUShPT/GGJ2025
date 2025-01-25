// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using System;

namespace AstralShift.Helpers.DynamicEnum
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DynamicEnumSelector))]
    public class DynamicEnumSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty dynamicEnumProperty = property.FindPropertyRelative("dynamicEnum");
            SerializedProperty selectedIndexProperty = property.FindPropertyRelative("selectedIndex");
            SerializedProperty value = property.FindPropertyRelative("value");

            DynamicEnum dynamicEnum = dynamicEnumProperty.objectReferenceValue as DynamicEnum;

            if (dynamicEnum != null)
            {
                string[] options = dynamicEnum.enumValues.ToArray();
                string[] filteredOptions = options.Where(o => o != "").ToArray();
                selectedIndexProperty.intValue = EditorGUI.Popup(position, label.text,
                    Array.IndexOf(filteredOptions, options[value.intValue]), filteredOptions);
                //Catch outOfRange
                if (selectedIndexProperty.intValue < 0 || selectedIndexProperty.intValue >= filteredOptions.Length)
                {
                    value.intValue = 0;
                }
                else if (selectedIndexProperty.intValue >= filteredOptions.Length)
                {
                    value.intValue = filteredOptions.Length - 1;
                }
                else
                {
                    value.intValue = Array.IndexOf(options, filteredOptions[selectedIndexProperty.intValue]);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, dynamicEnumProperty, label);
            }

            EditorGUI.EndProperty();
        }
    }
#endif
    [System.Serializable]
    public class DynamicEnumSelector
    {
        public DynamicEnum dynamicEnum;
        public int selectedIndex;
        public int value;
    }
}