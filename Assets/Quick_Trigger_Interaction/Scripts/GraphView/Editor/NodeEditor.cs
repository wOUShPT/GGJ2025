// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.NodeEditor.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace AstralShift.QTI.NodeEditor
{
    /// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
    [CustomNodeEditor(typeof(Node))]
    public class NodeEditor : NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, Node>
    {
        /// <summary> Fires every whenever a node was modified through the editor </summary>
        public static Action<Node> onUpdateNode;

        public static readonly Dictionary<NodePort, Vector2> portPositions = new Dictionary<NodePort, Vector2>();

        public virtual void OnHeaderGUI()
        {
        }

        public virtual void OnBodyGUI()
        {
        }

        public virtual int GetWidth()
        {
            Type type = target.GetType();
            int width;
            if (type.TryGetAttributeWidth(out width))
            {
                return width;
            }

            return 208;
        }

        /// <summary> Returns color for target node </summary>
        public virtual Color GetHeaderTint()
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

        /// <summary> Returns color for target node </summary>
        public virtual Color GetBodyTint()
        {
            if (EditorGUIUtility.isProSkin)
            {
                return NodeEditorPreferences.GetSettings().darkBodyColor;
            }
            else
            {
                return NodeEditorPreferences.GetSettings().lightBodyColor;
            }
        }

        public virtual GUIStyle GetHeaderStyle()
        {
            return QTIEditorResources.GraphView.Styles.NodeHeader;
        }

        public virtual GUIStyle GetBodyStyle()
        {
            return QTIEditorResources.GraphView.Styles.NodeBodyDark;
        }

        public virtual GUIStyle GetBodyHighlightStyle()
        {
            return QTIEditorResources.GraphView.Styles.NodeHighlight;
        }

        /// <summary> Override to display custom node header tooltips </summary>
        public virtual string GetHeaderTooltip()
        {
            return null;
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu)
        {
            bool canRemove = true;
            // Actions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is Node)
            {
                Node node = Selection.activeObject as Node;
                menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));
                menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);

                canRemove = NodeGraphEditor.GetEditor(node.graph, NodeEditorWindow.current).CanRemove(node);
            }

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Copy"), false, NodeEditorWindow.current.CopySelectedNodes);
            menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);

            if (canRemove)
            {
                menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
            }
            else
            {
                menu.AddItem(new GUIContent("Remove"), false, null);
            }

            // Custom sctions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is Node)
            {
                Node node = Selection.activeObject as Node;
                menu.AddCustomContextMenuItems(node);
            }
        }

        /// <summary> Rename the node asset. This will trigger a reimport of the node. </summary>
        public void Rename(string newName)
        {
            if (newName == null || newName.Trim() == "")
            {
                newName = NodeEditorUtilities.NodeDefaultName(target.GetType());
            }

            target.name = newName;
            OnRename();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }

        /// <summary> Called after this node's name has changed. </summary>
        public virtual void OnRename()
        {
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeEditorAttribute : Attribute,
            INodeEditorAttrib
        {
            private Type inspectedType;

            /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            public CustomNodeEditorAttribute(Type inspectedType)
            {
                this.inspectedType = inspectedType;
            }


            public Type GetInspectedType()
            {
                return inspectedType;
            }
        }
    }
}