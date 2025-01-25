// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.NodeEditor
{
    [CustomNodeEditor(typeof(ConditionInteractionNode))]
    public class ConditionInteractionNodeEditor : InteractionNodeEditor
    {
        private new ConditionInteractionNode _targetNode;

        public override void OnCreate()
        {
            _targetNode = target as ConditionInteractionNode;
            base.OnCreate();
        }

        protected override void DrawBodyHeader(InteractionBaseNode node)
        {
            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(new GUIContent(""), node.GetInputPort("entry"), GUILayout.MinWidth(0));
            NodeEditorGUILayout.PortField(new GUIContent(" OnTrue "), new GUIStyle("BoldLabel"),
                node.GetOutputPort("OnTrue"), GUILayout.MinWidth(0));
            GUILayout.EndHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            NodeEditorGUILayout.PortField(new GUIContent(" OnFalse "), new GUIStyle("BoldLabel"),
                node.GetOutputPort("OnFalse"), GUILayout.MinWidth(0));
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("GameObject/Component", QTIEditorResources.GraphView.Styles.NodeSection);

            // Pings the Interaction GameObject in hierarchy and the Interaction Component in Inspector
            if (node.component != null && GUILayout.Button(node.gameObject.ToString(), new GUIStyle("ObjectField")))
            {
                QTIGraphViewLauncher.PingInteractionComponent(node.component);
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Properties", QTIEditorResources.GraphView.Styles.NodeSection);
        }
    }
}