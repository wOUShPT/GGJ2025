// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Helpers.Attributes
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = enabled;

            if (enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            if (enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;

            // Get the source field path
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField);

            SerializedProperty sourceProperty = property.serializedObject.FindProperty(conditionPath);

            if (sourceProperty != null)
            {
                enabled = sourceProperty.boolValue;
            }
            else
            {
                Debug.LogWarning("ConditionalHideAttribute: Source field not found: " +
                                 condHAtt.ConditionalSourceField);
            }

            if (!condHAtt.HideIfFalse)
            {
                enabled = !enabled;
            }

            return enabled;
        }
    }
}
#endif