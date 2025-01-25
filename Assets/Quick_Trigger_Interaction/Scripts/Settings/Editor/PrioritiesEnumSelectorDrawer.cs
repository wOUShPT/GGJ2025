// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Settings
{
    [CustomPropertyDrawer(typeof(PrioritiesEnumSelector))]
    public class PrioritiesEnumSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty dynamicEnumProperty = property.FindPropertyRelative("dynamicEnum");
            SerializedProperty selectedIndexProperty = property.FindPropertyRelative("selectedIndex");
            SerializedProperty value = property.FindPropertyRelative("value");

            PrioritiesEnum dynamicEnum = dynamicEnumProperty.objectReferenceValue as PrioritiesEnum;

            if (dynamicEnum != null)
            {
                string[] options = dynamicEnum.enumValues.ToArray();
                if (options.Length == 0)
                {
                    GUIContent labelContent = new GUIContent(property.displayName);
                    position.size = new Vector2(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth, position.size.y);
                    EditorGUI.LabelField(position, labelContent);
                    position.x += EditorGUIUtility.labelWidth;
                    EditorGUI.HelpBox(position, "Priorities Enum list is empty", MessageType.Warning);
                    return;
                }
                
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
}