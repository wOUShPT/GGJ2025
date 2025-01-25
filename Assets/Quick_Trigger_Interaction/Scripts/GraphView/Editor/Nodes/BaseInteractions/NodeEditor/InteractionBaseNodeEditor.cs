// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.NodeEditor
{
    public class InteractionBaseNodeEditor : NodeEditor
    {
        private Texture _componentIcon;
        protected float _defaultLabelWidth;
        protected float _minLabelWidth;

        public override void OnHeaderGUI()
        {
        }

        public override void OnBodyGUI()
        {
        }

        /// <summary>
        /// Calculates property label minimum width value
        /// </summary>
        /// <param name="label"></param>
        protected virtual void CalculateMinLabelWidth(string label)
        {
            int newWidth = (int)GUI.skin.label.CalcSize(new GUIContent(label)).x;
            if (newWidth > _minLabelWidth)
            {
                _minLabelWidth = newWidth;
            }
        }

        protected Texture GetComponentIcon(Component component)
        {
            if (_componentIcon == null)
            {
                _componentIcon = EditorGUIUtility.GetIconForObject(component);
                if (_componentIcon == null)
                {
                    GUIContent iconContent = EditorGUIUtility.IconContent("cs Script Icon");
                    _componentIcon = iconContent.image;
                }
            }

            return _componentIcon;
        }

        public virtual void DrawComponentIcon(Component component, float width, float height)
        {
            GUIStyle style = new GUIStyle("Label");
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
            GUILayout.Label(GetComponentIcon(component), style, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.EndVertical();
        }

        public override Color GetHeaderTint()
        {
            // Try get color from [NodeTint] attribute
            Type type = target.GetType();
            Color color;
            if (type.TryGetAttributeTint(out color))
            {
                return color;
            }

            InteractionBaseNode targetNode = target as InteractionBaseNode;
            return NodeEditorPreferences.GetTypeColor(targetNode.component.GetType());
        }
    }
}