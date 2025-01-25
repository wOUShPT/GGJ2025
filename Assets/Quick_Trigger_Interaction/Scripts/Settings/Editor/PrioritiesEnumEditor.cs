// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Settings
{
    [CustomEditor(typeof(PrioritiesEnum))]
    public class PrioritiesEnumEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PrioritiesEnum priorityEnum = (PrioritiesEnum)target;

            EditorGUILayout.LabelField("Priority Values", EditorStyles.boldLabel);

            for (int i = 0; i < priorityEnum.enumValues.Count; i++)
            {
                priorityEnum.enumValues[i] = EditorGUILayout.TextField($"Priority {i}", priorityEnum.enumValues[i]);
            }

            if (GUILayout.Button("Add Value"))
            {
                Undo.RecordObject(target, "Added a new priority value!");
                priorityEnum.enumValues.Add($"Priority {priorityEnum.enumValues.Count}");
            }

            if (priorityEnum.enumValues.Count > 0 && GUILayout.Button("Remove the last priority value!"))
            {
                Undo.RecordObject(target, "Remove the last priority value!");
                priorityEnum.enumValues.RemoveAt(priorityEnum.enumValues.Count - 1);
            }

            EditorUtility.SetDirty(priorityEnum);
        }
    }
}