// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;
using InteractionEditor = AstralShift.QTI.Interactions.InteractionEditor;

namespace AstralShift.QTI.NodeEditor
{
    [CustomNodeEditor(typeof(InteractionNode))]
    public class InteractionNodeEditor : InteractionBaseNodeEditor
    {
        protected InteractionNode _targetNode;
        protected SerializedObject _targetSerializedObject;

        public override void OnCreate()
        {
            _targetNode = target as InteractionNode;
            _defaultLabelWidth = EditorGUIUtility.labelWidth;
            _targetSerializedObject = new SerializedObject(_targetNode.component);
            base.OnCreate();
        }

        public override void OnHeaderGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            DrawComponentIcon(_targetNode.component, 30, 30);
            GUILayout.Label(target.name, QTIEditorResources.GraphView.Styles.NodeHeaderLabel, GUILayout.Height(30));
            GUILayout.Space(35);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a default Interaction Node
        /// </summary>
        public override void OnBodyGUI()
        {
            if (_targetNode == null || _targetNode.component == null)
            {
                return;
            }

            DrawBodyHeader(_targetNode);
            _defaultLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = _minLabelWidth;
            InteractionEditor editorInstance = (InteractionEditor)Editor.CreateEditor(_targetNode.component);
            editorInstance.CreateInspectorGUI();
            editorInstance.DrawProperties();
            EditorGUIUtility.labelWidth = _defaultLabelWidth;
        }

        protected virtual void DrawBodyHeader(InteractionBaseNode node)
        {
            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(new GUIContent(""), node.GetInputPort("entry"), GUILayout.MinWidth(0));
            NodeEditorGUILayout.PortField(new GUIContent(" OnEnd "), new GUIStyle("BoldLabel"),
                node.GetOutputPort("exit"), GUILayout.MinWidth(0));
            GUILayout.EndHorizontal();
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

        /// <summary>
        /// Overriden to do nothing
        /// </summary>
        public override void OnRename()
        {
        }

        /// <summary>
        /// Calculates the node minimum width value
        /// </summary>
        /// <returns></returns>
        public int CalculateMinWidth()
        {
            if (_targetSerializedObject == null)
            {
                _targetSerializedObject = new SerializedObject(_targetNode.component);
            }

            CalculateMinLabelWidth(_targetNode.name);
            SerializedProperty interactionProperty = _targetSerializedObject.GetIterator();
            while (interactionProperty.NextVisible(true))
            {
                if (interactionProperty.displayName == "On End Interactions"
                    || interactionProperty.displayName == "Script")
                {
                    continue;
                }

                CalculateMinLabelWidth(interactionProperty.displayName);
            }

            return (int)Mathf.Clamp(_minLabelWidth + 200, 350, 1500);
        }

        /// <summary>
        /// Returns node width
        /// </summary>
        /// <returns></returns>
        public override int GetWidth()
        {
            if (_targetNode == null)
            {
                _targetNode = target as InteractionNode;
            }

            _targetNode.size.x = CalculateMinWidth();
            return CalculateMinWidth();
        }
    }
}