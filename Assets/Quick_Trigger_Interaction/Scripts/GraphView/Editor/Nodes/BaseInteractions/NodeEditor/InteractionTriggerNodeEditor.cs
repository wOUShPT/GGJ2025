// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;
using InteractionTriggerEditor = AstralShift.QTI.Triggers.InteractionTriggerEditor;


namespace AstralShift.QTI.NodeEditor
{
    [CustomNodeEditor(typeof(InteractionTriggerNode))]
    public class InteractionTriggerNodeEditor : InteractionBaseNodeEditor
    {
        protected InteractionTriggerNode _targetNode;
        protected SerializedObject _targetSerializedObject;
        protected new float _defaultLabelWidth;
        protected new float _minLabelWidth;

        public override void OnCreate()
        {
            _targetNode = target as InteractionTriggerNode;
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
        /// Draws a default Interaction Trigger Node
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
            InteractionTriggerEditor editorInstance =
                (InteractionTriggerEditor)Editor.CreateEditor(_targetNode.component);
            editorInstance.CreateInspectorGUI();
            editorInstance.DrawProperties();
            EditorGUIUtility.labelWidth = _defaultLabelWidth;
        }

        public virtual void DrawBodyHeader(InteractionBaseNode node)
        {
            NodeEditorGUILayout.PortField(new GUIContent(" Interaction "), new GUIStyle("BoldLabel"),
                _targetNode.GetOutputPort("exit"), GUILayout.MinWidth(0));
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("GameObject/Component", QTIEditorResources.GraphView.Styles.NodeSection);

            // Pings the Interaction GameObject in hierarchy and the Interaction Component in Inspector
            if (_targetNode.component != null &&
                GUILayout.Button(_targetNode.gameObject.ToString(), new GUIStyle("ObjectField")))
            {
                QTIGraphViewLauncher.PingInteractionComponent(_targetNode.component);
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Properties", QTIEditorResources.GraphView.Styles.NodeSection);
        }

        /// <summary>
        /// Calculates the node minimum width value
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateMinWidth()
        {
            if (_targetSerializedObject == null)
            {
                _targetSerializedObject = new SerializedObject(_targetNode.component);
            }

            CalculateMinLabelWidth(_targetNode.name);
            SerializedProperty interactionProperty = _targetSerializedObject.GetIterator();
            while (interactionProperty.NextVisible(true))
            {
                if (interactionProperty.displayName == "Interaction" ||
                    interactionProperty.displayName == "On End Interactions" ||
                    interactionProperty.displayName == "Script")
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
                _targetNode = target as InteractionTriggerNode;
            }

            _targetNode.size.x = CalculateMinWidth();
            return CalculateMinWidth();
        }
    }
}