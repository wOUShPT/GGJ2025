﻿// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.NodeEditor.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    /// <summary>
    /// Contains GUI methods
    /// </summary>
    public partial class NodeEditorWindow
    {
        public NodeGraphEditor graphEditor;
        protected List<Object> selectionCache;
        protected List<Node> culledNodes;

        /// <summary>
        /// 19 if docked, 22 if not
        /// </summary>
        protected int topPadding
        {
            get { return isDocked() ? 19 : 22; }
        }

        /// <summary>
        /// Executed after all other window GUI. Useful if Zoom is ruining your day. Automatically resets after being run.
        /// </summary>
        public event Action onLateGUI;

        private static readonly Vector3[] polyLineTempArray = new Vector3[2];

        protected virtual void OnGUI()
        {
            Event e = Event.current;
            Matrix4x4 m = GUI.matrix;
            if (graph == null)
            {
                return;
            }

            ValidateGraphEditor();
            Controls();

            DrawGrid(position, zoom, panOffset);
            DrawConnections();
            DrawDraggedConnection();
            DrawNodes();
            DrawSelectionBox();
            DrawTooltip();
            graphEditor.OnGUI();

            RunAndResetOnLateGUI();
            GUI.matrix = m;
        }

        protected virtual void RunAndResetOnLateGUI()
        {
            // Run and reset onLateGUI
            if (onLateGUI != null)
            {
                onLateGUI();
                onLateGUI = null;
            }
        }

        public static void BeginZoomed(Rect rect, float zoom, float topPadding)
        {
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Vector4 padding = new Vector4(0, topPadding, 0, 0);
            padding *= zoom;
            GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f,
                -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
                rect.width * zoom,
                rect.height * zoom));
        }

        public static void EndZoomed(Rect rect, float zoom, float topPadding)
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        public virtual void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = graphEditor.GetGridTexture();
            Texture2D crossTex = graphEditor.GetSecondaryGridTexture();

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
            GUI.DrawTextureWithTexCoords(new Rect(rect.position.x + 50, rect.position.y - 50, rect.width, rect.height),
                QTIEditorResources.GraphViewResources.NodeLogoTex, new Rect(new Vector2(0, 0), tileAmount / 2 / zoom));
        }

        public void DrawSelectionBox()
        {
            if (currentActivity == NodeActivity.DragGrid)
            {
                Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
                Vector2 size = curPos - _dragBoxStart;
                Rect r = new Rect(_dragBoxStart, size);
                r.position = GridToWindowPosition(r.position);
                r.size /= zoom;
                Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
            }
        }

        public static bool DropdownButton(string name, float width)
        {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        /// <summary>
        /// Show right-click context menu for hovered reroute
        /// </summary>
        protected virtual void ShowRerouteContextMenu(RerouteReference reroute)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove"), false, () => reroute.RemovePoint());
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Show right-click context menu for hovered port
        /// </summary>
        protected void ShowPortContextMenu(NodePort hoveredPort)
        {
            GenericMenu contextMenu = new GenericMenu();
            foreach (var port in hoveredPort.GetConnections())
            {
                var name = port.node.name;
                var index = hoveredPort.GetConnectionIndex(port);
                contextMenu.AddItem(new GUIContent(string.Format("Disconnect({0})", name)), false,
                    () => hoveredPort.Disconnect(index));
            }

            contextMenu.AddItem(new GUIContent("Clear Connections"), false, () => hoveredPort.ClearConnections());
            //Get compatible nodes with this port
            if (NodeEditorPreferences.GetSettings().createFilter)
            {
                contextMenu.AddSeparator("");

                if (hoveredPort.direction == NodePort.IO.Input)
                {
                    graphEditor.AddContextMenuItems(contextMenu, hoveredPort.ValueType, NodePort.IO.Output);
                }
                else
                {
                    graphEditor.AddContextMenuItems(contextMenu, hoveredPort.ValueType);
                }
            }

            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }
        }

        static Vector2 CalculateBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t, uu = u * u;
            float uuu = uu * u, ttt = tt * t;
            return new Vector2(
                (uuu * p0.x) + (3 * uu * t * p1.x) + (3 * u * tt * p2.x) + (ttt * p3.x),
                (uuu * p0.y) + (3 * uu * t * p1.y) + (3 * u * tt * p2.y) + (ttt * p3.y)
            );
        }

        /// <summary>
        /// Draws a line segment without allocating temporary arrays
        /// </summary>
        static void DrawAAPolyLineNonAlloc(float thickness, Vector2 p0, Vector2 p1)
        {
            polyLineTempArray[0].x = p0.x;
            polyLineTempArray[0].y = p0.y;
            polyLineTempArray[1].x = p1.x;
            polyLineTempArray[1].y = p1.y;
            Handles.DrawAAPolyLine(thickness, polyLineTempArray);
        }

        /// <summary>
        /// Draw a bezier from output to input in grid coordinates
        /// </summary>
        public void DrawConnection(Gradient gradient, ConnectionPath path, ConnectionStroke stroke, float thickness,
            List<Vector2> gridPoints)
        {
            // convert grid points to window points
            for (int i = 0; i < gridPoints.Count; ++i)
            {
                gridPoints[i] = GridToWindowPosition(gridPoints[i]);
            }

            Color originalHandlesColor = Handles.color;
            Handles.color = gradient.Evaluate(0f);
            int length = gridPoints.Count;
            switch (path)
            {
                case ConnectionPath.Curvy:

                    Vector2 outputTangent = Vector2.right;
                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 inputTangent;
                        // Cached most variables that repeat themselves here to avoid so many indexer calls :p
                        Vector2 point_a = gridPoints[i];
                        Vector2 point_b = gridPoints[i + 1];
                        float dist_ab = Vector2.Distance(point_a, point_b);
                        if (i == 0)
                        {
                            outputTangent = zoom * dist_ab * 0.01f * Vector2.right;
                        }

                        if (i < length - 2)
                        {
                            Vector2 point_c = gridPoints[i + 2];
                            Vector2 ab = (point_b - point_a).normalized;
                            Vector2 cb = (point_b - point_c).normalized;
                            Vector2 ac = (point_c - point_a).normalized;
                            Vector2 p = (ab + cb) * 0.5f;
                            float tangentLength = (dist_ab + Vector2.Distance(point_b, point_c)) * 0.005f * zoom;
                            float side = ((ac.x * (point_b.y - point_a.y)) - (ac.y * (point_b.x - point_a.x)));

                            p = tangentLength * Mathf.Sign(side) * new Vector2(-p.y, p.x);
                            inputTangent = p;
                        }
                        else
                        {
                            inputTangent = zoom * dist_ab * 0.01f * Vector2.left;
                        }

                        // Calculates the tangents for the bezier's curves.
                        float zoomCoef = 50 / zoom;
                        Vector2 tangent_a = point_a + outputTangent * zoomCoef;
                        Vector2 tangent_b = point_b + inputTangent * zoomCoef;
                        // Hover effect.
                        int division = Mathf.RoundToInt(.2f * dist_ab) + 3;
                        // Coloring and bezier drawing.
                        int draw = 0;
                        Vector2 bezierPrevious = point_a;
                        for (int j = 1; j <= division; ++j)
                        {
                            if (stroke == ConnectionStroke.Dashed)
                            {
                                draw++;
                                if (draw >= 2)
                                {
                                    draw = -2;
                                }

                                if (draw < 0)
                                {
                                    continue;
                                }

                                if (draw == 0)
                                {
                                    bezierPrevious = CalculateBezierPoint(point_a, tangent_a, tangent_b, point_b,
                                        (j - 1f) / division);
                                }
                            }

                            if (i == length - 2)
                            {
                                Handles.color = gradient.Evaluate((j + 1f) / division);
                            }

                            Vector2 bezierNext = CalculateBezierPoint(point_a, tangent_a, tangent_b, point_b,
                                j / (float)division);
                            DrawAAPolyLineNonAlloc(thickness, bezierPrevious, bezierNext);
                            bezierPrevious = bezierNext;
                        }

                        outputTangent = -inputTangent;
                    }

                    break;

                case ConnectionPath.Straight:

                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 point_a = gridPoints[i];
                        Vector2 point_b = gridPoints[i + 1];
                        // Draws the line with the coloring.
                        Vector2 prev_point = point_a;
                        // Approximately one segment per 5 pixels
                        int segments = (int)Vector2.Distance(point_a, point_b) / 5;
                        segments = Math.Max(segments, 1);

                        int draw = 0;
                        for (int j = 0; j <= segments; j++)
                        {
                            draw++;
                            float t = j / (float)segments;
                            Vector2 lerp = Vector2.Lerp(point_a, point_b, t);
                            if (draw > 0)
                            {
                                if (i == length - 2)
                                {
                                    Handles.color = gradient.Evaluate(t);
                                }

                                DrawAAPolyLineNonAlloc(thickness, prev_point, lerp);
                            }

                            prev_point = lerp;
                            if (stroke == ConnectionStroke.Dashed && draw >= 2)
                            {
                                draw = -2;
                            }
                        }
                    }

                    break;

                case ConnectionPath.Angled:

                    for (int i = 0; i < length - 1; i++)
                    {
                        if (i == length - 1)
                        {
                            continue; // Skip last index
                        }

                        if (gridPoints[i].x <= gridPoints[i + 1].x - (50 / zoom))
                        {
                            float midpoint = (gridPoints[i].x + gridPoints[i + 1].x) * 0.5f;
                            Vector2 start_1 = gridPoints[i];
                            Vector2 end_1 = gridPoints[i + 1];
                            start_1.x = midpoint;
                            end_1.x = midpoint;
                            if (i == length - 2)
                            {
                                DrawAAPolyLineNonAlloc(thickness, gridPoints[i], start_1);
                                Handles.color = gradient.Evaluate(0.5f);
                                DrawAAPolyLineNonAlloc(thickness, start_1, end_1);
                                Handles.color = gradient.Evaluate(1f);
                                DrawAAPolyLineNonAlloc(thickness, end_1, gridPoints[i + 1]);
                            }
                            else
                            {
                                DrawAAPolyLineNonAlloc(thickness, gridPoints[i], start_1);
                                DrawAAPolyLineNonAlloc(thickness, start_1, end_1);
                                DrawAAPolyLineNonAlloc(thickness, end_1, gridPoints[i + 1]);
                            }
                        }
                        else
                        {
                            float midpoint = (gridPoints[i].y + gridPoints[i + 1].y) * 0.5f;
                            Vector2 start_1 = gridPoints[i];
                            Vector2 end_1 = gridPoints[i + 1];
                            start_1.x += 25 / zoom;
                            end_1.x -= 25 / zoom;
                            Vector2 start_2 = start_1;
                            Vector2 end_2 = end_1;
                            start_2.y = midpoint;
                            end_2.y = midpoint;
                            if (i == length - 2)
                            {
                                DrawAAPolyLineNonAlloc(thickness, gridPoints[i], start_1);
                                Handles.color = gradient.Evaluate(0.25f);
                                DrawAAPolyLineNonAlloc(thickness, start_1, start_2);
                                Handles.color = gradient.Evaluate(0.5f);
                                DrawAAPolyLineNonAlloc(thickness, start_2, end_2);
                                Handles.color = gradient.Evaluate(0.75f);
                                DrawAAPolyLineNonAlloc(thickness, end_2, end_1);
                                Handles.color = gradient.Evaluate(1f);
                                DrawAAPolyLineNonAlloc(thickness, end_1, gridPoints[i + 1]);
                            }
                            else
                            {
                                DrawAAPolyLineNonAlloc(thickness, gridPoints[i], start_1);
                                DrawAAPolyLineNonAlloc(thickness, start_1, start_2);
                                DrawAAPolyLineNonAlloc(thickness, start_2, end_2);
                                DrawAAPolyLineNonAlloc(thickness, end_2, end_1);
                                DrawAAPolyLineNonAlloc(thickness, end_1, gridPoints[i + 1]);
                            }
                        }
                    }

                    break;

                case ConnectionPath.ShaderLab:

                    Vector2 start = gridPoints[0];
                    Vector2 end = gridPoints[length - 1];
                    //Modify first and last point in array so we can loop trough them nicely.
                    gridPoints[0] = gridPoints[0] + Vector2.right * (20 / zoom);
                    gridPoints[length - 1] = gridPoints[length - 1] + Vector2.left * (20 / zoom);
                    //Draw first vertical lines going out from nodes
                    Handles.color = gradient.Evaluate(0f);
                    DrawAAPolyLineNonAlloc(thickness, start, gridPoints[0]);
                    Handles.color = gradient.Evaluate(1f);
                    DrawAAPolyLineNonAlloc(thickness, end, gridPoints[length - 1]);
                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 point_a = gridPoints[i];
                        Vector2 point_b = gridPoints[i + 1];
                        // Draws the line with the coloring.
                        Vector2 prev_point = point_a;
                        // Approximately one segment per 5 pixels
                        int segments = (int)Vector2.Distance(point_a, point_b) / 5;
                        segments = Math.Max(segments, 1);

                        int draw = 0;
                        for (int j = 0; j <= segments; j++)
                        {
                            draw++;
                            float t = j / (float)segments;
                            Vector2 lerp = Vector2.Lerp(point_a, point_b, t);
                            if (draw > 0)
                            {
                                if (i == length - 2)
                                {
                                    Handles.color = gradient.Evaluate(t);
                                }

                                DrawAAPolyLineNonAlloc(thickness, prev_point, lerp);
                            }

                            prev_point = lerp;
                            if (stroke == ConnectionStroke.Dashed && draw >= 2)
                            {
                                draw = -2;
                            }
                        }
                    }

                    gridPoints[0] = start;
                    gridPoints[length - 1] = end;
                    break;
            }

            Handles.color = originalHandlesColor;
        }

        /// <summary>
        /// Draws all connections
        /// </summary>
        public virtual void DrawConnections()
        {
            Vector2 mousePos = Event.current.mousePosition;
            List<RerouteReference> selection = _preBoxSelectionReroute != null
                ? new List<RerouteReference>(_preBoxSelectionReroute)
                : new List<RerouteReference>();
            _hoveredReroute = new RerouteReference();

            List<Vector2> gridPoints = new List<Vector2>(2);

            Color col = GUI.color;

            if (graph == null || graph.nodes == null)
            {
                return;
            }

            foreach (Node node in graph.nodes)
            {
                //If a null node is found, return. This can happen if the nodes associated script is deleted. It is currently not possible in Unity to delete a null asset.
                if (node == null)
                {
                    continue;
                }

                // Draw full connections and output > reroute
                foreach (NodePort output in node.Outputs)
                {
                    //Needs cleanup. Null checks are ugly
                    Rect fromRect;
                    if (!_portConnectionPoints.TryGetValue(output, out fromRect))
                    {
                        continue;
                    }

                    Color portColor = graphEditor.GetPortColor(output);
                    GUIStyle portStyle = graphEditor.GetPortStyle(output);

                    for (int k = 0; k < output.ConnectionCount; k++)
                    {
                        NodePort input = output.GetConnection(k);

                        Gradient connectionGradient = graphEditor.GetConnectionGradient(output, input);
                        float connectionThickness = graphEditor.GetConnectionThickness(output, input);
                        ConnectionPath connectionPath = graphEditor.GetConnectionPath(output, input);
                        ConnectionStroke connectionStroke = graphEditor.GetConnectionStroke(output, input);

                        // Error handling
                        if (input == null)
                        {
                            continue; //If a script has been updated and the port doesn't exist, it is removed and null is returned. If this happens, return.
                        }

                        if (!input.IsConnectedTo(output))
                        {
                            input.Connect(output);
                        }

                        Rect toRect;
                        if (!_portConnectionPoints.TryGetValue(input, out toRect))
                        {
                            continue;
                        }

                        List<Vector2> reroutePoints = output.GetReroutePoints(k);

                        gridPoints.Clear();
                        gridPoints.Add(fromRect.center);
                        gridPoints.AddRange(reroutePoints);
                        gridPoints.Add(toRect.center);
                        DrawConnection(connectionGradient, connectionPath, connectionStroke, connectionThickness,
                            gridPoints);

                        // Loop through reroute points again and draw the points
                        for (int i = 0; i < reroutePoints.Count; i++)
                        {
                            RerouteReference rerouteRef = new RerouteReference(output, k, i);
                            // Draw reroute point at position
                            Rect rect = new Rect(reroutePoints[i], new Vector2(12, 12));
                            rect.position = new Vector2(rect.position.x - 6, rect.position.y - 6);
                            rect = GridToWindowRect(rect);

                            // Draw selected reroute points with an outline
                            if (selectedReroutes.Contains(rerouteRef))
                            {
                                GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
                                GUI.DrawTexture(rect, portStyle.normal.background);
                            }

                            GUI.color = portColor;
                            GUI.DrawTexture(rect, portStyle.active.background);
                            if (rect.Overlaps(_selectionBox))
                            {
                                selection.Add(rerouteRef);
                            }

                            if (rect.Contains(mousePos))
                            {
                                _hoveredReroute = rerouteRef;
                            }
                        }
                    }
                }
            }

            GUI.color = col;
            if (Event.current.type != EventType.Layout && currentActivity == NodeActivity.DragGrid)
            {
                selectedReroutes = selection;
            }
        }

        protected virtual void DrawNodes()
        {
            Event e = Event.current;
            if (e.type == EventType.Layout)
            {
                selectionCache = new List<Object>(Selection.objects);
            }

            MethodInfo onValidate = null;
            if (Selection.activeObject != null && Selection.activeObject is Node)
            {
                onValidate = Selection.activeObject.GetType().GetMethod("OnValidate");
                if (onValidate != null)
                {
                    EditorGUI.BeginChangeCheck();
                }
            }

            BeginZoomed(position, zoom, topPadding);

            Vector2 mousePos = Event.current.mousePosition;

            if (e.type != EventType.Layout)
            {
                _hoveredNode = null;
                _hoveredPort = null;
            }

            List<Object> preSelection =
                _preBoxSelection != null ? new List<Object>(_preBoxSelection) : new List<Object>();

            // Selection box stuff
            Vector2 boxStartPos = GridToWindowPositionNoClipped(_dragBoxStart);
            Vector2 boxSize = mousePos - boxStartPos;
            if (boxSize.x < 0)
            {
                boxStartPos.x += boxSize.x;
                boxSize.x = Mathf.Abs(boxSize.x);
            }

            if (boxSize.y < 0)
            {
                boxStartPos.y += boxSize.y;
                boxSize.y = Mathf.Abs(boxSize.y);
            }

            Rect selectionBox = new Rect(boxStartPos, boxSize);

            //Save guiColor so we can revert it
            Color guiColor = GUI.color;

            List<NodePort> removeEntries = new List<NodePort>();

            if (e.type == EventType.Layout)
            {
                culledNodes = new List<Node>();
            }

            for (int n = 0; n < graph.nodes.Count; n++)
            {
                // Skip null nodes. The user could be in the process of renaming scripts, so removing them at this point is not advisable.
                if (graph.nodes[n] == null)
                {
                    continue;
                }

                if (n >= graph.nodes.Count)
                {
                    return;
                }

                Node node = graph.nodes[n];

                // Culling
                if (e.type == EventType.Layout)
                {
                    // Cull unselected nodes outside view
                    if (!Selection.Contains(node) && ShouldBeCulled(node))
                    {
                        culledNodes.Add(node);
                        continue;
                    }
                }
                else if (culledNodes.Contains(node))
                {
                    continue;
                }

                if (e.type == EventType.Repaint)
                {
                    removeEntries.Clear();
                    foreach (var kvp in _portConnectionPoints)
                    {
                        if (kvp.Key.node == node) removeEntries.Add(kvp.Key);
                    }

                    foreach (var k in removeEntries)
                    {
                        _portConnectionPoints.Remove(k);
                    }
                }

                NodeEditor nodeEditor = NodeEditor.GetEditor(node, this);

                NodeEditor.portPositions.Clear();

                // Set default label width. This is potentially overridden in OnBodyGUI
                EditorGUIUtility.labelWidth = 84;

                //Get node position
                Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

                GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

                bool selected = selectionCache.Contains(graph.nodes[n]);

                if (selected)
                {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUIStyle highlightStyle = new GUIStyle(nodeEditor.GetBodyHighlightStyle());
                    highlightStyle.padding = style.padding;
                    style.padding = new RectOffset();
                    GUI.color = nodeEditor.GetHeaderTint();
                    GUILayout.BeginVertical(style);
                    GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
                    GUILayout.BeginVertical(new GUIStyle(highlightStyle));
                }
                else
                {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUI.color = nodeEditor.GetHeaderTint();
                    GUILayout.BeginVertical(style);
                }

                GUI.color = guiColor;
                EditorGUI.BeginChangeCheck();

                //Draw node contents
                nodeEditor.OnHeaderGUI();
                nodeEditor.OnBodyGUI();

                //If user changed a value, notify other scripts through onUpdateNode
                if (EditorGUI.EndChangeCheck())
                {
                    if (NodeEditor.onUpdateNode != null)
                    {
                        NodeEditor.onUpdateNode(node);
                    }

                    EditorUtility.SetDirty(node);
                    nodeEditor.serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();

                //Cache data about the node for next frame
                if (e.type == EventType.Repaint)
                {
                    Vector2 size = GUILayoutUtility.GetLastRect().size;
                    if (nodeSizes.ContainsKey(node))
                    {
                        nodeSizes[node] = size;
                    }
                    else
                    {
                        nodeSizes.Add(node, size);
                    }

                    foreach (var kvp in NodeEditor.portPositions)
                    {
                        Vector2 portHandlePos = kvp.Value;
                        portHandlePos += node.position;
                        Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                        portConnectionPoints[kvp.Key] = rect;
                    }
                }

                if (selected) GUILayout.EndVertical();

                if (e.type != EventType.Layout)
                {
                    //Check if we are hovering this node
                    Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                    Rect windowRect = new Rect(nodePos, nodeSize);
                    if (windowRect.Contains(mousePos))
                    {
                        _hoveredNode = node;
                    }

                    //If dragging a selection box, add nodes inside to selection
                    if (currentActivity == NodeActivity.DragGrid)
                    {
                        if (windowRect.Overlaps(selectionBox)) preSelection.Add(node);
                    }

                    //Check if we are hovering any of this nodes ports
                    //Check input ports
                    foreach (NodePort input in node.Inputs)
                    {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(input))
                        {
                            continue;
                        }

                        Rect r = GridToWindowRectNoClipped(portConnectionPoints[input]);
                        if (r.Contains(mousePos))
                        {
                            _hoveredPort = input;
                        }
                    }

                    //Check all output ports
                    foreach (NodePort output in node.Outputs)
                    {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(output))
                        {
                            continue;
                        }

                        Rect r = GridToWindowRectNoClipped(portConnectionPoints[output]);
                        if (r.Contains(mousePos))
                        {
                            _hoveredPort = output;
                        }
                    }
                }

                GUILayout.EndArea();
            }

            if (e.type != EventType.Layout && currentActivity == NodeActivity.DragGrid)
            {
                Selection.objects = preSelection.ToArray();
            }

            EndZoomed(position, zoom, topPadding);

            //If a change in is detected in the selected node, call OnValidate method.
            //This is done through reflection because OnValidate is only relevant in editor,
            //and thus, the code should not be included in build.
            if (onValidate != null && EditorGUI.EndChangeCheck())
            {
                onValidate.Invoke(Selection.activeObject, null);
            }
        }

        /// <summary>
        /// Calculates if a node should be culled out of view
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual bool ShouldBeCulled(Node node)
        {
            Vector2 nodePos = GridToWindowPositionNoClipped(node.position);
            if (nodePos.x / _zoom > position.width) // Right
            {
                return true;
            }

            if (nodePos.y / _zoom > position.height) // Bottom
            {
                return true;
            }

            if (nodeSizes.ContainsKey(node))
            {
                Vector2 size = nodeSizes[node];

                if (nodePos.x + size.x < 0) // Left
                {
                    return true;
                }

                if (nodePos.y + size.y < 0) // Top
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Draw mouseover node tooltips
        /// </summary>
        protected virtual void DrawTooltip()
        {
            if (!NodeEditorPreferences.GetSettings().portTooltips || graphEditor == null)
            {
                return;
            }

            string tooltip = null;
            if (_hoveredPort != null)
            {
                tooltip = graphEditor.GetPortTooltip(_hoveredPort);
            }
            else if (_hoveredNode != null && IsHoveringNode && IsHoveringTitle(_hoveredNode))
            {
                tooltip = NodeEditor.GetEditor(_hoveredNode, this).GetHeaderTooltip();
            }

            if (string.IsNullOrEmpty(tooltip))
            {
                return;
            }

            GUIContent content = new GUIContent(tooltip);
            Vector2 size = QTIEditorResources.GraphView.Styles.Tooltip.CalcSize(content);
            size.x += 8;
            Rect rect = new Rect(Event.current.mousePosition - (size), size);
            EditorGUI.LabelField(rect, content, QTIEditorResources.GraphView.Styles.Tooltip);
            Repaint();
        }
    }
}