// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Settings;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics2D
{
    [CustomEditor(typeof(Input2DTrigger))]
    public class Input2DTriggerEditor : Physics2DTriggerEditor
    {
        public override void OnInspectorGUI()
        {
            GetPrioritySettings();
            OnInspectorGUIHeader();
            DrawTagSelector();
            DrawProperties();
            DrawFooter();
        }

        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty interactionProp = serializedObject.FindProperty("interaction");
            SerializedProperty priorityProperty = serializedObject.FindProperty("priority");
            EditorGUILayout.PropertyField(priorityProperty);
            SerializedProperty isFixedAngleProp = serializedObject.FindProperty("isFixedAngle");
            SerializedProperty angleProperty = serializedObject.FindProperty("interactionAngle");
            EditorGUILayout.PropertyField(interactionProp);
            EditorGUILayout.PropertyField(isFixedAngleProp, new GUIContent("Fixed Angle"));
            GUIStyle labelStyle = new GUIStyle("Label");
            labelStyle.alignment = TextAnchor.MiddleLeft;
            if (isFixedAngleProp.boolValue)
            {
                SerializedProperty directionProperty = serializedObject.FindProperty("interactionDirection");
                EditorGUILayout.PropertyField(directionProperty, new GUIContent("Direction"));
            }

            EditorGUILayout.PropertyField(angleProperty, new GUIContent("Angle"));

            SerializedProperty interactionVisualProperty = serializedObject.FindProperty("interactionVisual");
            EditorGUILayout.PropertyField(interactionVisualProperty);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void GetPrioritySettings()
        {
            Input2DTrigger targetScript = target as Input2DTrigger;
            if (targetScript.priority.dynamicEnum == null)
            {
                targetScript.priority = InteractionsSettings.Instance.GetPriorities();
            }
        }
    }
}

