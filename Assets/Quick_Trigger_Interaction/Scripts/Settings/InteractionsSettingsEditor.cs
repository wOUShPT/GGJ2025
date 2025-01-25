// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;

namespace AstralShift.QTI.Settings
{
    [CustomEditor(typeof(InteractionsSettings), true), CanEditMultipleObjects]
    public class InteractionsSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Get the target ScriptableObject
            InteractionsSettings interactionSettings = (InteractionsSettings)target;

            // Start watching for changes
            EditorGUI.BeginChangeCheck();

            // Draw the default inspector
            DrawDefaultInspector();

            // If any field has changed
            if (EditorGUI.EndChangeCheck())
            {
                // Mark the ScriptableObject as dirty to save changes
                EditorUtility.SetDirty(interactionSettings);
            }
        }
    }
}
#endif