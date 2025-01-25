// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;

namespace AstralShift.QTI.Triggers
{
    [CustomEditor(typeof(OnDistanceTrigger)), CanEditMultipleObjects]
    public class OnDistanceTriggerEditor : InteractionTriggerEditor
    {
        public override void DrawProperties()
        {
            serializedObject.Update();

            // Fetch the current target object
            OnDistanceTrigger script = (OnDistanceTrigger)target;
            SerializedProperty interactionProp = serializedObject.FindProperty("interaction");
            EditorGUILayout.PropertyField(interactionProp);

            // Get all tags
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

            // Get the index of the current tag selected
            int currentIndex = System.Array.IndexOf(tags, script.selectedTag);

            EditorGUI.BeginChangeCheck();
            // Show a dropdown for the tags
            int selectedIndex = EditorGUILayout.Popup("Select Tag", currentIndex, tags);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Select Tag modified");
                // Set the selected tag
                script.selectedTag = tags[selectedIndex];
            }

            SerializedProperty distanceProperty = serializedObject.FindProperty("distance");
            EditorGUILayout.PropertyField(distanceProperty);

            SerializedProperty modeProperty = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(modeProperty);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}