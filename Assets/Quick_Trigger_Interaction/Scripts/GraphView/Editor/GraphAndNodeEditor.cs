// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.NodeEditor
{
    /// <summary> Override graph inspector to show an 'Open Graph' button at the top </summary>
    [CustomEditor(typeof(NodeGraph), true)]
    [CanEditMultipleObjects]
    public class GlobalGraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
            {
                NodeEditorWindow.Open(serializedObject.targetObject as NodeGraph);
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(Node), true)]
    [CanEditMultipleObjects]
    public class GlobalNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
            {
                SerializedProperty graphProp = serializedObject.FindProperty("graph");
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as NodeGraph);
                w.Home(); // Focus selected node
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            // Now draw the node itself.
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}