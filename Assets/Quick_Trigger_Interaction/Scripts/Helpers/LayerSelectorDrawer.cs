// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AstralShift.Helpers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerSelector))]
    public class LayerSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use LayerSelector with int.");
            }
        }
    }

#endif
    public class LayerSelector : PropertyAttribute
    {
    }
}