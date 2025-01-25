﻿// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.NodeEditor.Internal;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    /// <summary> Base class to derive custom Node Graph editors from. Use this to override how graphs are drawn in the editor. </summary>
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public class
        NodeGraphEditor : NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute, NodeGraph>
    {
        /// <summary> Are we currently renaming a node? </summary>
        protected bool isRenaming;

        public virtual void OnGUI()
        {
        }

        /// <summary> Called when opened by NodeEditorWindow </summary>
        public virtual void OnOpen()
        {
        }

        /// <summary> Called when NodeEditorWindow gains focus </summary>
        public virtual void OnWindowFocus()
        {
        }

        /// <summary> Called when NodeEditorWindow loses focus </summary>
        public virtual void OnWindowFocusLost()
        {
        }

        public virtual Texture2D GetGridTexture()
        {
            if (EditorGUIUtility.isProSkin)
            {
                return NodeEditorPreferences.GetSettings().GridTextureDark;
            }
            else
            {
                return NodeEditorPreferences.GetSettings().GridTextureLight;
            }
        }

        public virtual Texture2D GetSecondaryGridTexture()
        {
            if (EditorGUIUtility.isProSkin)
            {
                return NodeEditorPreferences.GetSettings().CrossTextureDark;
            }
            else
            {
                return NodeEditorPreferences.GetSettings().CrossTextureLight;
            }
        }

        /// <summary> Return default settings for this graph type. This is the settings the user will load if no previous settings have been saved. </summary>
        public virtual NodeEditorPreferences.Settings GetDefaultPreferences()
        {
            return new NodeEditorPreferences.Settings();
        }

        /// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
        public virtual string GetNodeMenuName(Type type)
        {
            //Check if type has the CreateNodeMenuAttribute
            Node.CreateNodeMenuAttribute attrib;

            if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
            {
                return attrib.menuName;
            }

            // Return generated path
            return NodeEditorUtilities.NodeDefaultPath(type);
        }

        /// <summary> The order by which the menu items are displayed. </summary>
        public virtual int GetNodeMenuOrder(Type type)
        {
            //Check if type has the CreateNodeMenuAttribute
            Node.CreateNodeMenuAttribute attrib;
            if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
            {
                return attrib.order;
            }

            return 0;
        }

        /// <summary>
        /// Called before connecting two ports in the graph view to see if the output port is compatible with the input port
        /// </summary>
        public virtual bool CanConnect(NodePort output, NodePort input)
        {
            return output.CanConnectTo(input);
        }

        /// <summary>
        /// Add items for the context menu when right-clicking this node.
        /// Override to add custom menu items.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="compatibleType">Use it to filter only nodes with ports value type, compatible with this type</param>
        /// <param name="direction">Direction of the compatiblity</param>
        public virtual void AddContextMenuItems(GenericMenu menu, Type compatibleType = null,
            NodePort.IO direction = NodePort.IO.Input)
        {
            Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);

            Type[] nodeTypes;

            if (compatibleType != null && NodeEditorPreferences.GetSettings().createFilter)
            {
                nodeTypes = NodeEditorUtilities
                    .GetCompatibleNodesTypes(NodeEditorReflection.nodeTypes, compatibleType, direction)
                    .OrderBy(GetNodeMenuOrder).ToArray();
            }
            else
            {
                nodeTypes = NodeEditorReflection.nodeTypes.OrderBy(GetNodeMenuOrder).ToArray();
            }

            for (int i = 0; i < nodeTypes.Length; i++)
            {
                Type type = nodeTypes[i];

                //Get node context menu path
                string path = GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                // Check if user is allowed to add more of given node type
                Node.DisallowMultipleNodesAttribute disallowAttrib;
                bool disallowed = false;
                if (NodeEditorUtilities.GetAttrib(type, out disallowAttrib))
                {
                    int typeCount = target.nodes.Count(x => x.GetType() == type);
                    if (typeCount >= disallowAttrib.max)
                    {
                        disallowed = true;
                    }
                }

                // Add node entry to context menu
                if (disallowed)
                {
                    menu.AddItem(new GUIContent(path), false, null);
                }
                else
                {
                    menu.AddItem(new GUIContent(path), false, () =>
                    {
                        Node node = CreateNode(type, pos);
                        if (node != null)
                        {
                            NodeEditorWindow.current.AutoConnect(node); // handle null nodes to avoid nullref exceptions
                        }
                    });
                }
            }

            menu.AddSeparator("");

            if (NodeEditorWindow.copyBuffer != null && NodeEditorWindow.copyBuffer.Length > 0)
            {
                menu.AddItem(new GUIContent("Paste"), false, () => NodeEditorWindow.current.PasteNodes(pos));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
            }

            menu.AddItem(new GUIContent("Preferences"), false, () => NodeEditorReflection.OpenPreferences());
            menu.AddCustomContextMenuItems(target);
        }

        /// <summary> Returned gradient is used to color noodles </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual Gradient GetConnectionGradient(NodePort output, NodePort input)
        {
            Gradient grad = new Gradient();

            // If dragging the noodle, draw solid, slightly transparent
            if (input == null)
            {
                Color a = GetTypeColor(output.ValueType);
                grad.SetKeys(
                    new[] { new GradientColorKey(a, 0f) },
                    new[] { new GradientAlphaKey(0.6f, 0f) }
                );
            }
            // If normal, draw gradient fading from one input color to the other
            else
            {
                Color a = GetTypeColor(output.ValueType);
                Color b = GetTypeColor(input.ValueType);
                // If any port is hovered, tint white
                if (window._hoveredPort == output || window._hoveredPort == input)
                {
                    a = Color.Lerp(a, Color.white, 0.8f);
                    b = Color.Lerp(b, Color.white, 0.8f);
                }

                grad.SetKeys(
                    new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
                    new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
            }

            return grad;
        }

        /// <summary> Returned float is used for noodle thickness </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual float GetConnectionThickness(NodePort output, NodePort input)
        {
            return NodeEditorPreferences.GetSettings().connectionThickness;
        }

        public virtual ConnectionPath GetConnectionPath(NodePort output, NodePort input)
        {
            return NodeEditorPreferences.GetSettings().connectionPath;
        }

        public virtual ConnectionStroke GetConnectionStroke(NodePort output, NodePort input)
        {
            return NodeEditorPreferences.GetSettings().connectionStroke;
        }

        public virtual Vector2 GetNodePadding()
        {
            NodeEditorPreferences.Settings settings = NodeEditorPreferences.GetSettings();
            return new Vector2(settings.nodePaddingX, settings.nodePaddingY);
        }

        /// <summary> Returned color is used to color ports </summary>
        public virtual Color GetPortColor(NodePort port)
        {
            return GetTypeColor(port.ValueType);
        }

        /// <summary>
        /// The returned Style is used to configure the paddings and icon texture of the ports.
        /// Use these properties to customize your port style.
        ///
        /// The properties used is:
        /// <see cref="GUIStyle.padding"/>[Left and Right], <see cref="GUIStyle.normal"/> [Background] = border texture,
        /// and <seealso cref="GUIStyle.active"/> [Background] = dot texture;
        /// </summary>
        /// <param name="port">the owner of the style</param>
        /// <returns></returns>
        public virtual GUIStyle GetPortStyle(NodePort port)
        {
            if (port.direction == NodePort.IO.Input)
            {
                return QTIEditorResources.GraphView.Styles.InputPort;
            }

            return QTIEditorResources.GraphView.Styles.OutputPort;
        }

        /// <summary> The returned color is used to color the background of the door.
        /// Usually used for outer edge effect </summary>
        public virtual Color GetPortBackgroundColor(NodePort port)
        {
            return Color.gray;
        }

        /// <summary> Returns generated color for a type. This color is editable in preferences </summary>
        public virtual Color GetTypeColor(Type type)
        {
            return NodeEditorPreferences.GetTypeColor(type);
        }

        /// <summary> Override to display custom tooltips </summary>
        public virtual string GetPortTooltip(NodePort port)
        {
            Type portType = port.ValueType;
            string tooltip = "";
            tooltip = portType.PrettyName();
            if (port.IsOutput)
            {
                object obj = port.node.GetValue(port);
                tooltip += " = " + (obj != null ? obj.ToString() : "null");
            }

            return tooltip;
        }

        /// <summary> Deal with objects dropped into the graph through DragAndDrop </summary>
        public virtual void OnDropObjects(Object[] objects)
        {
            if (GetType() != typeof(NodeGraphEditor))
            {
                Debug.Log("No OnDropObjects override defined for " + GetType());
            }
        }

        /// <summary> Create a node and save it in the graph asset </summary>
        public virtual Node CreateNode(Type type, Vector2 position)
        {
            Undo.RecordObject(target, "Create Node");
            Node node = target.AddNode(type);
            if (node == null)
            {
                return null; // handle null nodes to avoid nullref exceptions
            }

            Undo.RegisterCreatedObjectUndo(node, "Create Node");
            node.position = position;
            if (node.name == null || node.name.Trim() == "")
            {
                node.name = NodeEditorUtilities.NodeDefaultName(type);
            }

            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
            {
                AssetDatabase.AddObjectToAsset(node, target);
            }

            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }

            NodeEditorWindow.RepaintAll();
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public Node CopyNode(Node original)
        {
            Undo.RecordObject(target, "Duplicate Node");
            Node node = target.CopyNode(original);
            Undo.RegisterCreatedObjectUndo(node, "Duplicate Node");
            node.name = original.name;
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
            {
                AssetDatabase.AddObjectToAsset(node, target);
            }

            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }

            return node;
        }

        /// <summary> Return false for nodes that can't be removed </summary>
        public virtual bool CanRemove(Node node)
        {
            // Check graph attributes to see if this node is required
            Type graphType = target.GetType();
            NodeGraph.RequireNodeAttribute[] attribs = Array.ConvertAll(
                graphType.GetCustomAttributes(typeof(NodeGraph.RequireNodeAttribute), true),
                x => x as NodeGraph.RequireNodeAttribute);
            if (attribs.Any(x => x.Requires(node.GetType())))
            {
                if (target.nodes.Count(x => x.GetType() == node.GetType()) <= 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary> Safely remove a node and all its connections. </summary>
        public virtual void RemoveNode(Node node)
        {
            if (!CanRemove(node))
            {
                return;
            }

            // Remove the node
            Undo.RecordObject(node, "Delete Node");
            Undo.RecordObject(target, "Delete Node");
            foreach (var port in node.Ports)
            {
                foreach (var conn in port.GetConnections())
                {
                    Undo.RecordObject(conn.node, "Delete Node");
                }
            }

            target.RemoveNode(node);
            Undo.DestroyObjectImmediate(node);
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeGraphEditorAttribute : Attribute,
            INodeEditorAttrib
        {
            private Type inspectedType;
            private bool editorForChildClasses;
            public string editorPrefsKey;

            /// <summary> Tells a NodeGraphEditor which Graph type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
            public CustomNodeGraphEditorAttribute(Type inspectedType,
                string editorPrefsKey = "InteractionsGraphView.Settings")
            {
                this.inspectedType = inspectedType;
                this.editorPrefsKey = editorPrefsKey;
                editorForChildClasses = false;
            }

            /// <summary> Tells a NodeGraphEditor which Graph type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
            public CustomNodeGraphEditorAttribute(Type inspectedType, bool editorForChildClasses,
                string editorPrefsKey = "InteractionsGraphView.Settings")
            {
                this.inspectedType = inspectedType;
                this.editorPrefsKey = editorPrefsKey;
                this.editorForChildClasses = editorForChildClasses;
            }

            public Type GetInspectedType()
            {
                return inspectedType;
            }

            public bool IsEditorForChildClasses()
            {
                return editorForChildClasses;
            }
        }
    }
}