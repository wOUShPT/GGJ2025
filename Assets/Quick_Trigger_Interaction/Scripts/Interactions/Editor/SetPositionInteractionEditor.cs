// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(SetPositionInteraction)), CanEditMultipleObjects]
    public class SetPositionInteractionEditor : InteractionEditor
    {
        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty mode = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(mode);
            SerializedProperty targetObject = serializedObject.FindProperty("targetObject");
            EditorGUILayout.PropertyField(targetObject);

            switch ((SetPositionInteraction.Mode)mode.enumValueIndex)
            {
                case SetPositionInteraction.Mode.transform:
                    SerializedProperty newPositionTransform = serializedObject.FindProperty("newPositionTransform");
                    EditorGUILayout.PropertyField(newPositionTransform);
                    break;

                case SetPositionInteraction.Mode.position:
                    SerializedProperty newPosition = serializedObject.FindProperty("newPosition");
                    EditorGUILayout.PropertyField(newPosition);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}