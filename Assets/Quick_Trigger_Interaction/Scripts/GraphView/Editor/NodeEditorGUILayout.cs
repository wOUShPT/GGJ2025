﻿// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    /// <summary> xNode-specific version of <see cref="EditorGUILayout"/> </summary>
    public static class NodeEditorGUILayout
    {
        private static readonly Dictionary<Object, Dictionary<string, ReorderableList>> _reorderableListCache =
            new Dictionary<Object, Dictionary<string, ReorderableList>>();

        private static int _reorderableListIndex = -1;

        /// <summary> Make a field for a serialized property. Automatically displays relevant node port. </summary>
        public static void PropertyField(SerializedProperty property, bool includeChildren = true,
            params GUILayoutOption[] options)
        {
            PropertyField(property, (GUIContent)null, includeChildren, options);
        }

        /// <summary> Make a field for a serialized property. Automatically displays relevant node port. </summary>
        public static void PropertyField(SerializedProperty property, GUIContent label, bool includeChildren = true,
            params GUILayoutOption[] options)
        {
            if (property == null) throw new NullReferenceException();
            Node node = property.serializedObject.targetObject as Node;
            if (node == null)
            {
                PropertyField(property, label, null, includeChildren);
                return;
            }

            NodePort port = node.GetPort(property.name);
            PropertyField(property, label, port, includeChildren);
        }

        /// <summary> Make a field for a serialized property. Manual node port override. </summary>
        public static void PropertyField(SerializedProperty property, NodePort port, bool includeChildren = true,
            params GUILayoutOption[] options)
        {
            PropertyField(property, null, port, includeChildren, options);
        }

        /// <summary> Make a field for a serialized property. Manual node port override. </summary>
        public static void PropertyField(SerializedProperty property, GUIContent label, NodePort port,
            bool includeChildren = true, params GUILayoutOption[] options)
        {
            if (property == null)
            {
                throw new NullReferenceException();
            }

            // If property is not a port, display a regular property field
            if (port == null)
            {
                if (property.type == typeof(Vector2).ToString())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(property.displayName);
                    property.vector2Value = EditorGUILayout.Vector2Field("", property.vector3Value);
                    EditorGUILayout.EndHorizontal();
                    ;
                }
                else if (property.type == typeof(Vector3).ToString())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(property.displayName);
                    property.vector3Value = EditorGUILayout.Vector3Field("", property.vector3Value);
                    EditorGUILayout.EndHorizontal();
                    ;
                }
                else if (property.type == typeof(Vector4).ToString())
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(property.displayName);
                    property.vector3Value = EditorGUILayout.Vector4Field("", property.vector4Value);
                    EditorGUILayout.EndHorizontal();
                    ;
                }
                else
                {
                    EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                }
            }
            else
            {
                Rect rect = new Rect();

                List<PropertyAttribute> propertyAttributes =
                    NodeEditorUtilities.GetCachedPropertyAttribs(port.node.GetType(), property.name);

                // If property is an input, display a regular property field and put a port handle on the left side
                if (port.direction == NodePort.IO.Input)
                {
                    // Get data from [Input] attribute
                    Node.ShowBackingValue showBacking = Node.ShowBackingValue.Unconnected;
                    Node.InputAttribute inputAttribute;
                    bool dynamicPortList = false;
                    if (NodeEditorUtilities.GetCachedAttrib(port.node.GetType(), property.name, out inputAttribute))
                    {
                        dynamicPortList = inputAttribute.dynamicPortList;
                        showBacking = inputAttribute.backingValue;
                    }

                    bool usePropertyAttributes = dynamicPortList ||
                                                 showBacking == Node.ShowBackingValue.Never ||
                                                 (showBacking == Node.ShowBackingValue.Unconnected && port.IsConnected);

                    float spacePadding = 0;
                    string tooltip = null;
                    foreach (var attr in propertyAttributes)
                    {
                        if (attr is SpaceAttribute)
                        {
                            if (usePropertyAttributes)
                            {
                                GUILayout.Space((attr as SpaceAttribute).height);
                            }
                            else
                            {
                                spacePadding += (attr as SpaceAttribute).height;
                            }
                        }
                        else if (attr is HeaderAttribute)
                        {
                            if (usePropertyAttributes)
                            {
                                //GUI Values are from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptAttributeGUI/Implementations/DecoratorDrawers.cs
                                Rect position = GUILayoutUtility.GetRect(0,
                                    (EditorGUIUtility.singleLineHeight * 1.5f) -
                                    EditorGUIUtility
                                        .standardVerticalSpacing); //Layout adds standardVerticalSpacing after rect so we subtract it.
                                position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
                                position = EditorGUI.IndentedRect(position);
                                GUI.Label(position, (attr as HeaderAttribute).header, EditorStyles.boldLabel);
                            }
                            else
                            {
                                spacePadding += EditorGUIUtility.singleLineHeight * 1.5f;
                            }
                        }
                        else if (attr is TooltipAttribute)
                        {
                            tooltip = (attr as TooltipAttribute).tooltip;
                        }
                    }

                    if (dynamicPortList)
                    {
                        Type type = GetType(property);
                        Node.ConnectionType connectionType = inputAttribute != null
                            ? inputAttribute.connectionType
                            : Node.ConnectionType.Multiple;
                        DynamicPortList(property.name, type, property.serializedObject, port.direction, connectionType);
                        return;
                    }

                    switch (showBacking)
                    {
                        case Node.ShowBackingValue.Unconnected:

                            // Display a label if port is connected
                            if (port.IsConnected)
                            {
                                EditorGUILayout.LabelField(label != null
                                    ? label
                                    : new GUIContent(property.displayName, tooltip));
                            }
                            // Display an editable property field if port is not connected
                            else
                            {
                                EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            }

                            break;

                        case Node.ShowBackingValue.Never:

                            // Display a label
                            EditorGUILayout.LabelField(label != null
                                ? label
                                : new GUIContent(property.displayName, tooltip));
                            break;

                        case Node.ShowBackingValue.Always:

                            // Display an editable property field
                            EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                    }

                    rect = GUILayoutUtility.GetLastRect();
                    float paddingLeft = NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.left;
                    rect.position = rect.position - new Vector2(16 + paddingLeft, -spacePadding);
                    // If property is an output, display a text label and put a port handle on the right side
                }
                else if (port.direction == NodePort.IO.Output)
                {
                    // Get data from [Output] attribute
                    Node.ShowBackingValue showBacking = Node.ShowBackingValue.Unconnected;
                    Node.OutputAttribute outputAttribute;
                    bool dynamicPortList = false;
                    if (NodeEditorUtilities.GetCachedAttrib(port.node.GetType(), property.name, out outputAttribute))
                    {
                        dynamicPortList = outputAttribute.dynamicPortList;
                        showBacking = outputAttribute.backingValue;
                    }

                    bool usePropertyAttributes = dynamicPortList ||
                                                 showBacking == Node.ShowBackingValue.Never ||
                                                 (showBacking == Node.ShowBackingValue.Unconnected && port.IsConnected);

                    float spacePadding = 0;
                    string tooltip = null;
                    foreach (var attr in propertyAttributes)
                    {
                        if (attr is SpaceAttribute)
                        {
                            if (usePropertyAttributes) GUILayout.Space((attr as SpaceAttribute).height);
                            else spacePadding += (attr as SpaceAttribute).height;
                        }
                        else if (attr is HeaderAttribute)
                        {
                            if (usePropertyAttributes)
                            {
                                //GUI Values are from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptAttributeGUI/Implementations/DecoratorDrawers.cs
                                Rect position = GUILayoutUtility.GetRect(0,
                                    (EditorGUIUtility.singleLineHeight * 1.5f) -
                                    EditorGUIUtility
                                        .standardVerticalSpacing); //Layout adds standardVerticalSpacing after rect so we subtract it.
                                position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
                                position = EditorGUI.IndentedRect(position);
                                GUI.Label(position, (attr as HeaderAttribute).header, EditorStyles.boldLabel);
                            }
                            else
                            {
                                spacePadding += EditorGUIUtility.singleLineHeight * 1.5f;
                            }
                        }
                        else if (attr is TooltipAttribute)
                        {
                            tooltip = (attr as TooltipAttribute).tooltip;
                        }
                    }

                    if (dynamicPortList)
                    {
                        Type type = GetType(property);
                        Node.ConnectionType connectionType = outputAttribute != null
                            ? outputAttribute.connectionType
                            : Node.ConnectionType.Multiple;
                        DynamicPortList(property.name, type, property.serializedObject, port.direction, connectionType);
                        return;
                    }

                    switch (showBacking)
                    {
                        case Node.ShowBackingValue.Unconnected:

                            // Display a label if port is connected
                            if (port.IsConnected)
                            {
                                EditorGUILayout.LabelField(
                                    label != null ? label : new GUIContent(property.displayName, tooltip),
                                    QTIEditorResources.GraphViewResources.OutputPort, GUILayout.MinWidth(30));
                            }
                            // Display an editable property field if port is not connected
                            else
                                EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));

                            break;

                        case Node.ShowBackingValue.Never:

                            // Display a label
                            EditorGUILayout.LabelField(
                                label != null ? label : new GUIContent(property.displayName, tooltip),
                                QTIEditorResources.GraphViewResources.OutputPort, GUILayout.MinWidth(30));
                            break;

                        case Node.ShowBackingValue.Always:

                            // Display an editable property field
                            EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                    }

                    rect = GUILayoutUtility.GetLastRect();
                    rect.width += NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.right;
                    rect.position = rect.position + new Vector2(rect.width, spacePadding);
                }

                rect.size = new Vector2(16, 16);

                Color backgroundColor = NodeEditorWindow.current.graphEditor.GetPortBackgroundColor(port);
                Color col = NodeEditorWindow.current.graphEditor.GetPortColor(port);
                GUIStyle portStyle = NodeEditorWindow.current.graphEditor.GetPortStyle(port);
                DrawPortHandle(rect, backgroundColor, col, portStyle.normal.background, portStyle.active.background);

                // Register the handle position
                Vector2 portPos = rect.center;
                NodeEditor.portPositions[port] = portPos;
            }
        }

        private static Type GetType(SerializedProperty property)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetFieldInfo(property.name);
            return fi.FieldType;
        }

        /// <summary> Make a simple port field. </summary>
        public static void PortField(NodePort port, params GUILayoutOption[] options)
        {
            PortField(null, port, options);
        }

        /// <summary> Make a simple port field. </summary>
        public static void PortField(GUIContent label, NodePort port, params GUILayoutOption[] options)
        {
            if (port == null)
            {
                return;
            }

            if (options == null)
            {
                options = new[] { GUILayout.MinWidth(30) };
            }

            Vector2 position = Vector3.zero;
            GUIContent content = label != null ? label : new GUIContent(ObjectNames.NicifyVariableName(port.fieldName));

            // If property is an input, display a regular property field and put a port handle on the left side
            if (port.direction == NodePort.IO.Input)
            {
                // Display a label
                EditorGUILayout.LabelField(content, options);

                Rect rect = GUILayoutUtility.GetLastRect();
                float paddingLeft = NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.left;
                position = rect.position - new Vector2(16 + paddingLeft, 0);
            }
            // If property is an output, display a text label and put a port handle on the right side
            else if (port.direction == NodePort.IO.Output)
            {
                // Display a label
                EditorGUILayout.LabelField(content, QTIEditorResources.GraphViewResources.OutputPort, options);

                Rect rect = GUILayoutUtility.GetLastRect();
                rect.width += NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.right;
                position = rect.position + new Vector2(rect.width, 0);
            }

            PortField(position, port);
        }

        public static void PortField(GUIContent label, GUIStyle labelStyle, NodePort port,
            params GUILayoutOption[] options)
        {
            if (port == null)
            {
                return;
            }

            if (options == null)
            {
                options = new[] { GUILayout.MinWidth(30) };
            }

            Vector2 position = Vector3.zero;
            GUIContent content = label != null ? label : new GUIContent(ObjectNames.NicifyVariableName(port.fieldName));

            // If property is an input, display a regular property field and put a port handle on the left side
            if (port.direction == NodePort.IO.Input)
            {
                // Display a label
                EditorGUILayout.LabelField(content, labelStyle, options);

                Rect rect = GUILayoutUtility.GetLastRect();
                float paddingLeft = NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.left;
                position = rect.position - new Vector2(16 + paddingLeft, 0);
            }
            // If property is an output, display a text label and put a port handle on the right side
            else if (port.direction == NodePort.IO.Output)
            {
                // Display a label
                labelStyle.alignment = TextAnchor.UpperRight;
                EditorGUILayout.LabelField(content, labelStyle, options);

                Rect rect = GUILayoutUtility.GetLastRect();
                rect.width += NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.right;
                position = rect.position + new Vector2(rect.width, 0);
            }

            PortField(position, port);
        }

        /// <summary> Make a simple port field. </summary>
        public static void PortField(Vector2 position, NodePort port)
        {
            if (port == null)
            {
                return;
            }

            Rect rect = new Rect(position, new Vector2(16, 16));

            Color backgroundColor = NodeEditorWindow.current.graphEditor.GetPortBackgroundColor(port);
            Color col = NodeEditorWindow.current.graphEditor.GetPortColor(port);
            GUIStyle portStyle = NodeEditorWindow.current.graphEditor.GetPortStyle(port);

            DrawPortHandle(rect, backgroundColor, col, portStyle.normal.background, portStyle.active.background);

            // Register the handle position
            Vector2 portPos = rect.center;
            NodeEditor.portPositions[port] = portPos;
        }

        /// <summary> Add a port field to previous layout element. </summary>
        public static void AddPortField(NodePort port)
        {
            if (port == null)
            {
                return;
            }

            Rect rect = new Rect();

            // If property is an input, display a regular property field and put a port handle on the left side
            if (port.direction == NodePort.IO.Input)
            {
                rect = GUILayoutUtility.GetLastRect();
                float paddingLeft = NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.left;
                rect.position = rect.position - new Vector2(16 + paddingLeft, 0);
                // If property is an output, display a text label and put a port handle on the right side
            }
            else if (port.direction == NodePort.IO.Output)
            {
                rect = GUILayoutUtility.GetLastRect();
                rect.width += NodeEditorWindow.current.graphEditor.GetPortStyle(port).padding.right;
                rect.position = rect.position + new Vector2(rect.width, 0);
            }

            rect.size = new Vector2(16, 16);

            Color backgroundColor = NodeEditorWindow.current.graphEditor.GetPortBackgroundColor(port);
            Color col = NodeEditorWindow.current.graphEditor.GetPortColor(port);
            GUIStyle portStyle = NodeEditorWindow.current.graphEditor.GetPortStyle(port);

            DrawPortHandle(rect, backgroundColor, col, portStyle.normal.background, portStyle.active.background);

            // Register the handle position
            Vector2 portPos = rect.center;
            NodeEditor.portPositions[port] = portPos;
        }

        /// <summary> Draws an input and an output port on the same line </summary>
        public static void PortPair(NodePort input, NodePort output)
        {
            GUILayout.BeginHorizontal();
            PortField(input, GUILayout.MinWidth(0));
            PortField(output, GUILayout.MinWidth(0));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the port
        /// </summary>
        /// <param name="rect">position and size</param>
        /// <param name="backgroundColor">color for background texture of the port. Normaly used to Border</param>
        /// <param name="typeColor"></param>
        /// <param name="border">texture for border of the dot port</param>
        /// <param name="dot">texture for the dot port</param>
        public static void DrawPortHandle(Rect rect, Color backgroundColor, Color typeColor, Texture2D border,
            Texture2D dot)
        {
            Color col = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, border);
            GUI.color = typeColor;
            GUI.DrawTexture(rect, dot);
            GUI.color = col;
        }


        #region Obsolete

        [Obsolete("Use IsDynamicPortListPort instead")]
        public static bool IsInstancePortListPort(NodePort port)
        {
            return IsDynamicPortListPort(port);
        }

        [Obsolete("Use DynamicPortList instead")]
        public static void InstancePortList(string fieldName, Type type, SerializedObject serializedObject,
            NodePort.IO io, Node.ConnectionType connectionType = Node.ConnectionType.Multiple,
            Node.TypeConstraint typeConstraint = Node.TypeConstraint.None, Action<ReorderableList> onCreation = null)
        {
            DynamicPortList(fieldName, type, serializedObject, io, connectionType, typeConstraint, onCreation);
        }

        #endregion

        /// <summary> Is this port part of a DynamicPortList? </summary>
        public static bool IsDynamicPortListPort(NodePort port)
        {
            string[] parts = port.fieldName.Split(' ');
            if (parts.Length != 2)
            {
                return false;
            }

            Dictionary<string, ReorderableList> cache;
            if (_reorderableListCache.TryGetValue(port.node, out cache))
            {
                ReorderableList list;
                if (cache.TryGetValue(parts[0], out list))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> Draw an editable list of dynamic ports. Port names are named as "[fieldName] [index]" </summary>
        /// <param name="fieldName">Supply a list for editable values</param>
        /// <param name="type">Value type of added dynamic ports</param>
        /// <param name="serializedObject">The serializedObject of the node</param>
        /// <param name="connectionType">Connection type of added dynamic ports</param>
        /// <param name="onCreation">Called on the list on creation. Use this if you want to customize the created ReorderableList</param>
        public static void DynamicPortList(string fieldName, Type type, SerializedObject serializedObject,
            NodePort.IO io, Node.ConnectionType connectionType = Node.ConnectionType.Multiple,
            Node.TypeConstraint typeConstraint = Node.TypeConstraint.None, Action<ReorderableList> onCreation = null)
        {
            Node node = serializedObject.targetObject as Node;

            var indexedPorts = node.DynamicPorts.Select(x =>
            {
                string[] split = x.fieldName.Split(' ');
                if (split != null && split.Length == 2 && split[0] == fieldName)
                {
                    int i = -1;
                    if (int.TryParse(split[1], out i))
                    {
                        return new { index = i, port = x };
                    }
                }

                return new { index = -1, port = (NodePort)null };
            }).Where(x => x.port != null);
            List<NodePort> dynamicPorts = indexedPorts.OrderBy(x => x.index).Select(x => x.port).ToList();

            node.UpdatePorts();

            ReorderableList list = null;
            Dictionary<string, ReorderableList> rlc;
            if (_reorderableListCache.TryGetValue(serializedObject.targetObject, out rlc))
            {
                if (!rlc.TryGetValue(fieldName, out list))
                {
                    list = null;
                }
            }

            // If a ReorderableList isn't cached for this array, do so.
            if (list == null)
            {
                SerializedProperty arrayData = serializedObject.FindProperty(fieldName);
                list = CreateReorderableList(fieldName, dynamicPorts, arrayData, type, serializedObject, io,
                    connectionType, typeConstraint, onCreation);
                if (_reorderableListCache.TryGetValue(serializedObject.targetObject, out rlc)) rlc.Add(fieldName, list);
                else
                    _reorderableListCache.Add(serializedObject.targetObject,
                        new Dictionary<string, ReorderableList> { { fieldName, list } });
            }

            list.list = dynamicPorts;
            list.DoLayoutList();
        }

        private static ReorderableList CreateReorderableList(string fieldName, List<NodePort> dynamicPorts,
            SerializedProperty arrayData, Type type, SerializedObject serializedObject, NodePort.IO io,
            Node.ConnectionType connectionType, Node.TypeConstraint typeConstraint, Action<ReorderableList> onCreation)
        {
            bool hasArrayData = arrayData != null && arrayData.isArray;
            Node node = serializedObject.targetObject as Node;
            ReorderableList list = new ReorderableList(dynamicPorts, null, true, true, true, true);
            string label = arrayData != null ? arrayData.displayName : ObjectNames.NicifyVariableName(fieldName);

            list.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    NodePort port = node.GetPort(fieldName + " " + index);
                    if (hasArrayData && arrayData.propertyType != SerializedPropertyType.String)
                    {
                        if (arrayData.arraySize <= index)
                        {
                            EditorGUI.LabelField(rect, "Array[" + index + "] data out of range");
                            return;
                        }

                        SerializedProperty itemData = arrayData.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(rect, itemData, true);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, port != null ? port.fieldName : "");
                    }

                    if (port != null)
                    {
                        Vector2 pos = rect.position +
                                      (port.IsOutput ? new Vector2(rect.width + 6, 0) : new Vector2(-36, 0));
                        PortField(pos, port);
                    }
                };
            list.elementHeightCallback =
                index =>
                {
                    if (hasArrayData)
                    {
                        if (arrayData.arraySize <= index)
                        {
                            return EditorGUIUtility.singleLineHeight;
                        }

                        SerializedProperty itemData = arrayData.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(itemData);
                    }

                    return EditorGUIUtility.singleLineHeight;
                };
            list.drawHeaderCallback =
                rect => { EditorGUI.LabelField(rect, label); };
            list.onSelectCallback =
                rl => { _reorderableListIndex = rl.index; };
            list.onReorderCallback =
                rl =>
                {
                    serializedObject.Update();
                    bool hasRect = false;
                    bool hasNewRect = false;
                    Rect rect = Rect.zero;
                    Rect newRect = Rect.zero;
                    // Move up
                    if (rl.index > _reorderableListIndex)
                    {
                        for (int i = _reorderableListIndex; i < rl.index; ++i)
                        {
                            NodePort port = node.GetPort(fieldName + " " + i);
                            NodePort nextPort = node.GetPort(fieldName + " " + (i + 1));
                            port.SwapConnections(nextPort);

                            // Swap cached positions to mitigate twitching
                            hasRect = NodeEditorWindow.current.portConnectionPoints.TryGetValue(port, out rect);
                            hasNewRect =
                                NodeEditorWindow.current.portConnectionPoints.TryGetValue(nextPort, out newRect);
                            NodeEditorWindow.current.portConnectionPoints[port] = hasNewRect ? newRect : rect;
                            NodeEditorWindow.current.portConnectionPoints[nextPort] = hasRect ? rect : newRect;
                        }
                    }
                    // Move down
                    else
                    {
                        for (int i = _reorderableListIndex; i > rl.index; --i)
                        {
                            NodePort port = node.GetPort(fieldName + " " + i);
                            NodePort nextPort = node.GetPort(fieldName + " " + (i - 1));
                            port.SwapConnections(nextPort);

                            // Swap cached positions to mitigate twitching
                            hasRect = NodeEditorWindow.current.portConnectionPoints.TryGetValue(port, out rect);
                            hasNewRect =
                                NodeEditorWindow.current.portConnectionPoints.TryGetValue(nextPort, out newRect);
                            NodeEditorWindow.current.portConnectionPoints[port] = hasNewRect ? newRect : rect;
                            NodeEditorWindow.current.portConnectionPoints[nextPort] = hasRect ? rect : newRect;
                        }
                    }

                    // Apply changes
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    // Move array data if there is any
                    if (hasArrayData)
                    {
                        arrayData.MoveArrayElement(_reorderableListIndex, rl.index);
                    }

                    // Apply changes
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    NodeEditorWindow.current.Repaint();
                    EditorApplication.delayCall += NodeEditorWindow.current.Repaint;
                };
            list.onAddCallback =
                rl =>
                {
                    // Add dynamic port postfixed with an index number
                    string newName = fieldName + " 0";
                    int i = 0;
                    while (node.HasPort(newName)) newName = fieldName + " " + (++i);

                    if (io == NodePort.IO.Output)
                    {
                        node.AddDynamicOutput(type, connectionType, Node.TypeConstraint.None, newName);
                    }
                    else
                    {
                        node.AddDynamicInput(type, connectionType, typeConstraint, newName);
                    }

                    serializedObject.Update();
                    EditorUtility.SetDirty(node);
                    if (hasArrayData)
                    {
                        arrayData.InsertArrayElementAtIndex(arrayData.arraySize);
                    }

                    serializedObject.ApplyModifiedProperties();
                };
            list.onRemoveCallback =
                rl =>
                {
                    var indexedPorts = node.DynamicPorts.Select(x =>
                    {
                        string[] split = x.fieldName.Split(' ');
                        if (split != null && split.Length == 2 && split[0] == fieldName)
                        {
                            int i = -1;
                            if (int.TryParse(split[1], out i))
                            {
                                return new { index = i, port = x };
                            }
                        }

                        return new { index = -1, port = (NodePort)null };
                    }).Where(x => x.port != null);
                    dynamicPorts = indexedPorts.OrderBy(x => x.index).Select(x => x.port).ToList();

                    int index = rl.index;

                    if (dynamicPorts[index] == null)
                    {
                        Debug.LogWarning("No port found at index " + index + " - Skipped");
                    }
                    else if (dynamicPorts.Count <= index)
                    {
                        Debug.LogWarning("DynamicPorts[" + index + "] out of range. Length was " + dynamicPorts.Count +
                                         " - Skipped");
                    }
                    else
                    {
                        // Clear the removed ports connections
                        dynamicPorts[index].ClearConnections();
                        // Move following connections one step up to replace the missing connection
                        for (int k = index + 1; k < dynamicPorts.Count(); k++)
                        {
                            for (int j = 0; j < dynamicPorts[k].ConnectionCount; j++)
                            {
                                NodePort other = dynamicPorts[k].GetConnection(j);
                                dynamicPorts[k].Disconnect(other);
                                dynamicPorts[k - 1].Connect(other);
                            }
                        }

                        // Remove the last dynamic port, to avoid messing up the indexing
                        node.RemoveDynamicPort(dynamicPorts[dynamicPorts.Count() - 1].fieldName);
                        serializedObject.Update();
                        EditorUtility.SetDirty(node);
                    }

                    if (hasArrayData && arrayData.propertyType != SerializedPropertyType.String)
                    {
                        if (arrayData.arraySize <= index)
                        {
                            Debug.LogWarning("Attempted to remove array index " + index + " where only " +
                                             arrayData.arraySize + " exist - Skipped");
                            Debug.Log(rl.list[0]);
                            return;
                        }

                        arrayData.DeleteArrayElementAtIndex(index);
                        // Error handling. If the following happens too often, file a bug report at https://github.com/Siccity/xNode/issues
                        if (dynamicPorts.Count <= arrayData.arraySize)
                        {
                            while (dynamicPorts.Count <= arrayData.arraySize)
                            {
                                arrayData.DeleteArrayElementAtIndex(arrayData.arraySize - 1);
                            }

                            Debug.LogWarning("Array size exceeded dynamic ports size. Excess items removed.");
                        }

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                };

            if (hasArrayData)
            {
                int dynamicPortCount = dynamicPorts.Count;
                while (dynamicPortCount < arrayData.arraySize)
                {
                    // Add dynamic port postfixed with an index number
                    string newName = arrayData.name + " 0";
                    int i = 0;

                    while (node.HasPort(newName))
                    {
                        newName = arrayData.name + " " + (++i);
                    }

                    if (io == NodePort.IO.Output)
                    {
                        node.AddDynamicOutput(type, connectionType, typeConstraint, newName);
                    }
                    else
                    {
                        node.AddDynamicInput(type, connectionType, typeConstraint, newName);
                    }

                    EditorUtility.SetDirty(node);
                    dynamicPortCount++;
                }

                while (arrayData.arraySize < dynamicPortCount)
                {
                    arrayData.InsertArrayElementAtIndex(arrayData.arraySize);
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            if (onCreation != null)
            {
                onCreation(list);
            }

            return list;
        }
    }
}