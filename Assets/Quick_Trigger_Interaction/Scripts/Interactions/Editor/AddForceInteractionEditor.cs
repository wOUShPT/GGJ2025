// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(AddForceInteraction)), CanEditMultipleObjects]
    public class AddForceInteractionEditor : InteractionEditor
    {
        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty body = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(body);
            SerializedProperty forceType = serializedObject.FindProperty("forceType");
            EditorGUILayout.PropertyField(forceType);
            SerializedProperty magnitude = serializedObject.FindProperty("magnitude");

            switch ((AddForceInteraction.Mode)body.enumValueIndex)
            {
                case AddForceInteraction.Mode._3D:

                    SerializedProperty rigidbody = serializedObject.FindProperty("body");
                    EditorGUILayout.PropertyField(rigidbody);
                    if ((AddForceInteraction.ForceType)forceType.enumValueIndex !=
                        AddForceInteraction.ForceType.relative)
                    {
                        SerializedProperty orientation = serializedObject.FindProperty("orientation");
                        EditorGUILayout.PropertyField(orientation);
                    }

                    EditorGUILayout.PropertyField(magnitude);
                    SerializedProperty forceMode = serializedObject.FindProperty("forceMode");
                    EditorGUILayout.PropertyField(forceMode);
                    break;

                case AddForceInteraction.Mode._2D:

                    SerializedProperty rigidbody2D = serializedObject.FindProperty("body2D");
                    EditorGUILayout.PropertyField(rigidbody2D);
                    if ((AddForceInteraction.ForceType)forceType.enumValueIndex !=
                        AddForceInteraction.ForceType.relative)
                    {
                        SerializedProperty angle = serializedObject.FindProperty("angle");
                        EditorGUILayout.PropertyField(angle);
                    }

                    EditorGUILayout.PropertyField(magnitude);
                    SerializedProperty forceMode2D = serializedObject.FindProperty("forceMode2D");
                    EditorGUILayout.PropertyField(forceMode2D);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}