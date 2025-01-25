// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(SetRotationInteraction)), CanEditMultipleObjects]
    public class SetRotationInteractionEditor : InteractionEditor
    {
        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty targetObject = serializedObject.FindProperty("targetObject");
            EditorGUILayout.PropertyField(targetObject);
            SerializedProperty mode = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(mode);

            switch ((SetRotationInteraction.Mode)mode.enumValueIndex)
            {
                case SetRotationInteraction.Mode.Euler:
                    SerializedProperty newRotation = serializedObject.FindProperty("newRotation");
                    EditorGUILayout.PropertyField(newRotation);
                    break;

                case SetRotationInteraction.Mode.AngleAxis:
                    SerializedProperty axis = serializedObject.FindProperty("axis");
                    EditorGUILayout.PropertyField(axis);
                    SerializedProperty angle = serializedObject.FindProperty("angle");
                    EditorGUILayout.PropertyField(angle);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}