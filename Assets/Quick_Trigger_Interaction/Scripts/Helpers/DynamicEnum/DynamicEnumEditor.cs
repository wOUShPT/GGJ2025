// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AstralShift.Helpers.DynamicEnum
{
    [CustomEditor(typeof(DynamicEnum))]
    public class DynamicEnumEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DynamicEnum dynamicEnum = (DynamicEnum)target;

            EditorGUILayout.LabelField("Enum Values", EditorStyles.boldLabel);

            for (int i = 0; i < dynamicEnum.enumValues.Count; i++)
            {
                dynamicEnum.enumValues[i] = EditorGUILayout.TextField($"Option {i}", dynamicEnum.enumValues[i]);
            }

            if (GUILayout.Button("Add Value"))
            {
                dynamicEnum.enumValues.Add($"Option {dynamicEnum.enumValues.Count}");
            }

            if (dynamicEnum.enumValues.Count > 0 && GUILayout.Button("Remove Last Value"))
            {
                dynamicEnum.enumValues.RemoveAt(dynamicEnum.enumValues.Count - 1);
            }

            EditorUtility.SetDirty(dynamicEnum);
        }
    }
}
#endif