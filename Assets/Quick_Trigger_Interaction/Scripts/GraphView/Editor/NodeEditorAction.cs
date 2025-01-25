// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.NodeEditor.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    public partial class NodeEditorWindow
    {
        public enum NodeActivity
        {
            Idle,
            HoldNode,
            DragNode,
            HoldGrid,
            DragGrid
        }

        public static NodeActivity currentActivity = NodeActivity.Idle;

        public static bool isPanning { get; protected set; }
        public static Vector2[] dragOffset;

        public static Node[] copyBuffer;

        public bool IsDraggingPort
        {
            get { return _draggedOutput != null; }
        }

        public bool IsHoveringPort
        {
            get { return _hoveredPort != null; }
        }

        public bool IsHoveringNode
        {
            get { return _hoveredNode != null; }
        }

        public bool IsHoveringReroute
        {
            get { return _hoveredReroute.port != null; }
        }

        /// <summary> Return the dragged port or null if not exist </summary>
        public NodePort DraggedOutputPort
        {
            get
            {
                NodePort result = _draggedOutput;
                return result;
            }
        }

        /// <summary> Return the Hovered port or null if not exist </summary>
        public NodePort HoveredPort
        {
            get
            {
                NodePort result = _hoveredPort;
                return result;
            }
        }

        /// <summary> Return the Hovered node or null if not exist </summary>
        public Node HoveredNode
        {
            get
            {
                Node result = _hoveredNode;
                return result;
            }
        }

        protected Node _hoveredNode = null;
        [NonSerialized] public NodePort _hoveredPort = null;
        [NonSerialized] protected NodePort _draggedOutput;
        [NonSerialized] protected NodePort _draggedOutputTarget;
        [NonSerialized] protected NodePort _autoConnectOutput;
        [NonSerialized] protected List<Vector2> _draggedOutputReroutes = new List<Vector2>();

        protected RerouteReference _hoveredReroute = new RerouteReference();
        public List<RerouteReference> selectedReroutes = new List<RerouteReference>();
        protected Vector2 _dragBoxStart;
        protected Object[] _preBoxSelection;
        protected RerouteReference[] _preBoxSelectionReroute;
        protected Rect _selectionBox;
        protected bool _isDoubleClick;
        protected Vector2 _lastMousePosition;
        protected float _dragThreshold = 1f;

        protected virtual void Controls()
        {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        graphEditor.OnDropObjects(DragAndDrop.objectReferences);
                    }

                    break;

                case EventType.MouseMove:

                    //Keyboard commands will not get correct mouse position from Event
                    _lastMousePosition = e.mousePosition;
                    break;

                case EventType.ScrollWheel:

                    float oldZoom = zoom;
                    if (e.delta.y > 0)
                    {
                        zoom += 0.1f * zoom;
                    }
                    else
                    {
                        zoom -= 0.1f * zoom;
                    }

                    if (NodeEditorPreferences.GetSettings().zoomToMouse)
                    {
                        panOffset += (1 - oldZoom / zoom) * (WindowToGridPosition(e.mousePosition) + panOffset);
                    }

                    break;

                case EventType.MouseDrag:

                    if (e.button == 0)
                    {
                        if (IsDraggingPort)
                        {
                            // Set target even if we can't connect, so as to prevent auto-conn menu from opening erroneously
                            if (IsHoveringPort && _hoveredPort.IsInput && !_draggedOutput.IsConnectedTo(_hoveredPort))
                            {
                                _draggedOutputTarget = _hoveredPort;
                            }
                            else
                            {
                                _draggedOutputTarget = null;
                            }

                            Repaint();
                        }
                        else if (currentActivity == NodeActivity.HoldNode)
                        {
                            RecalculateDragOffsets(e);
                            currentActivity = NodeActivity.DragNode;
                            Repaint();
                        }

                        if (currentActivity == NodeActivity.DragNode)
                        {
                            // Holding ctrl inverts grid snap
                            bool gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                            if (e.control)
                            {
                                gridSnap = !gridSnap;
                            }

                            Vector2 mousePos = WindowToGridPosition(e.mousePosition);
                            // Move selected nodes with offset
                            for (int i = 0; i < Selection.objects.Length; i++)
                            {
                                if (Selection.objects[i] is Node)
                                {
                                    Node node = Selection.objects[i] as Node;
                                    Undo.RecordObject(node, "Moved Node");
                                    Vector2 initial = node.position;
                                    node.position = mousePos + dragOffset[i];
                                    if (gridSnap)
                                    {
                                        node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                                        node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                                    }

                                    // Offset portConnectionPoints instantly if a node is dragged so they aren't delayed by a frame.
                                    Vector2 offset = node.position - initial;
                                    if (offset.sqrMagnitude > 0)
                                    {
                                        foreach (NodePort output in node.Outputs)
                                        {
                                            Rect rect;
                                            if (portConnectionPoints.TryGetValue(output, out rect))
                                            {
                                                rect.position += offset;
                                                portConnectionPoints[output] = rect;
                                            }
                                        }

                                        foreach (NodePort input in node.Inputs)
                                        {
                                            Rect rect;
                                            if (portConnectionPoints.TryGetValue(input, out rect))
                                            {
                                                rect.position += offset;
                                                portConnectionPoints[input] = rect;
                                            }
                                        }
                                    }
                                }
                            }

                            // Move selected reroutes with offset
                            for (int i = 0; i < selectedReroutes.Count; i++)
                            {
                                Vector2 pos = mousePos + dragOffset[Selection.objects.Length + i];
                                if (gridSnap)
                                {
                                    pos.x = (Mathf.Round(pos.x / 16) * 16);
                                    pos.y = (Mathf.Round(pos.y / 16) * 16);
                                }

                                selectedReroutes[i].SetPoint(pos);
                            }

                            Repaint();
                        }
                        else if (currentActivity == NodeActivity.HoldGrid)
                        {
                            currentActivity = NodeActivity.DragGrid;
                            _preBoxSelection = Selection.objects;
                            _preBoxSelectionReroute = selectedReroutes.ToArray();
                            _dragBoxStart = WindowToGridPosition(e.mousePosition);
                            Repaint();
                        }
                        else if (currentActivity == NodeActivity.DragGrid)
                        {
                            Vector2 boxStartPos = GridToWindowPosition(_dragBoxStart);
                            Vector2 boxSize = e.mousePosition - boxStartPos;
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

                            _selectionBox = new Rect(boxStartPos, boxSize);
                            Repaint();
                        }
                    }
                    else if (e.button == 1 || e.button == 2)
                    {
                        //check drag threshold for larger screens
                        if (e.delta.magnitude > _dragThreshold)
                        {
                            panOffset += e.delta * zoom;
                            isPanning = true;
                        }
                    }

                    break;

                case EventType.MouseDown:

                    Repaint();
                    if (e.button == 0)
                    {
                        _draggedOutputReroutes.Clear();

                        if (IsHoveringPort)
                        {
                            if (_hoveredPort.IsOutput)
                            {
                                _draggedOutput = _hoveredPort;
                                _autoConnectOutput = _hoveredPort;
                            }
                            else
                            {
                                _hoveredPort.VerifyConnections();
                                _autoConnectOutput = null;
                                if (_hoveredPort.IsConnected)
                                {
                                    Node node = _hoveredPort.node;
                                    NodePort output = _hoveredPort.Connection;
                                    int outputConnectionIndex = output.GetConnectionIndex(_hoveredPort);
                                    _draggedOutputReroutes = output.GetReroutePoints(outputConnectionIndex);
                                    _hoveredPort.Disconnect(output);
                                    _draggedOutput = output;
                                    _draggedOutputTarget = _hoveredPort;
                                    if (NodeEditor.onUpdateNode != null)
                                    {
                                        NodeEditor.onUpdateNode(node);
                                    }
                                }
                            }
                        }
                        else if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
                        {
                            // If mousedown on node header, select or deselect
                            if (!Selection.Contains(_hoveredNode))
                            {
                                SelectNode(_hoveredNode, e.control || e.shift);
                                if (!e.control && !e.shift)
                                {
                                    selectedReroutes.Clear();
                                }
                            }
                            else if (e.control || e.shift)
                            {
                                DeselectNode(_hoveredNode);
                            }

                            // Cache double click state, but only act on it in MouseUp - Except ClickCount only works in mouseDown.
                            _isDoubleClick = (e.clickCount == 2);

                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        }
                        else if (IsHoveringReroute)
                        {
                            // If reroute isn't selected
                            if (!selectedReroutes.Contains(_hoveredReroute))
                            {
                                // Add it
                                if (e.control || e.shift)
                                {
                                    selectedReroutes.Add(_hoveredReroute);
                                }
                                // Select it
                                else
                                {
                                    selectedReroutes = new List<RerouteReference> { _hoveredReroute };
                                    Selection.activeObject = null;
                                }
                            }
                            // Deselect
                            else if (e.control || e.shift)
                            {
                                selectedReroutes.Remove(_hoveredReroute);
                            }

                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        }
                        // If mousedown on grid background, deselect all
                        else if (!IsHoveringNode)
                        {
                            currentActivity = NodeActivity.HoldGrid;
                            if (!e.control && !e.shift)
                            {
                                selectedReroutes.Clear();
                                Selection.activeObject = null;
                            }
                        }
                    }

                    break;

                case EventType.MouseUp:

                    if (e.button == 0)
                    {
                        //Port drag release
                        if (IsDraggingPort)
                        {
                            // If connection is valid, save it
                            if (_draggedOutputTarget != null &&
                                graphEditor.CanConnect(_draggedOutput, _draggedOutputTarget))
                            {
                                Node node = _draggedOutputTarget.node;
                                if (graph.nodes.Count != 0)
                                {
                                    _draggedOutput.Connect(_draggedOutputTarget);
                                }

                                // ConnectionIndex can be -1 if the connection is removed instantly after creation
                                int connectionIndex = _draggedOutput.GetConnectionIndex(_draggedOutputTarget);
                                if (connectionIndex != -1)
                                {
                                    _draggedOutput.GetReroutePoints(connectionIndex).AddRange(_draggedOutputReroutes);
                                    if (NodeEditor.onUpdateNode != null)
                                    {
                                        NodeEditor.onUpdateNode(node);
                                    }

                                    EditorUtility.SetDirty(graph);
                                }
                            }
                            // Open context menu for auto-connection if there is no target node
                            else if (_draggedOutputTarget == null && NodeEditorPreferences.GetSettings().dragToCreate &&
                                     _autoConnectOutput != null)
                            {
                                GenericMenu menu = new GenericMenu();
                                graphEditor.AddContextMenuItems(menu, _draggedOutput.ValueType);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                            }

                            //Release dragged connection
                            _draggedOutput = null;
                            _draggedOutputTarget = null;
                            EditorUtility.SetDirty(graph);
                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }
                        else if (currentActivity == NodeActivity.DragNode)
                        {
                            IEnumerable<Node> nodes = Selection.objects.OfType<Node>();
                            foreach (Node node in nodes)
                            {
                                EditorUtility.SetDirty(node);
                            }

                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }
                        else if (!IsHoveringNode)
                        {
                            // If click outside node, release field focus
                            if (!isPanning)
                            {
                                EditorGUI.FocusTextInControl(null);
                                EditorGUIUtility.editingTextField = false;
                            }

                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }

                        // If click node header, select it.
                        if (currentActivity == NodeActivity.HoldNode && !(e.control || e.shift))
                        {
                            selectedReroutes.Clear();
                            SelectNode(_hoveredNode, false);

                            // Double click to center node
                            if (_isDoubleClick)
                            {
                                Vector2 nodeDimension = nodeSizes.ContainsKey(_hoveredNode)
                                    ? nodeSizes[_hoveredNode] / 2
                                    : Vector2.zero;
                                panOffset = -_hoveredNode.position - nodeDimension;
                            }
                        }

                        // If click reroute, select it.
                        if (IsHoveringReroute && !(e.control || e.shift))
                        {
                            selectedReroutes = new List<RerouteReference>
                            {
                                _hoveredReroute
                            };
                            Selection.activeObject = null;
                        }

                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    }
                    else if (e.button == 1 || e.button == 2)
                    {
                        if (!isPanning)
                        {
                            if (IsDraggingPort)
                            {
                                _draggedOutputReroutes.Add(WindowToGridPosition(e.mousePosition));
                            }
                            else if (currentActivity == NodeActivity.DragNode && Selection.activeObject == null &&
                                     selectedReroutes.Count == 1)
                            {
                                selectedReroutes[0].InsertPoint(selectedReroutes[0].GetPoint());
                                selectedReroutes[0] = new RerouteReference(selectedReroutes[0].port,
                                    selectedReroutes[0].connectionIndex, selectedReroutes[0].pointIndex + 1);
                            }
                            else if (IsHoveringReroute)
                            {
                                ShowRerouteContextMenu(_hoveredReroute);
                            }
                            else if (IsHoveringPort)
                            {
                                ShowPortContextMenu(_hoveredPort);
                            }
                            else if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
                            {
                                if (!Selection.Contains(_hoveredNode))
                                {
                                    SelectNode(_hoveredNode, false);
                                }

                                _autoConnectOutput = null;
                                GenericMenu menu = new GenericMenu();
                                NodeEditor.GetEditor(_hoveredNode, this).AddContextMenuItems(menu);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                                e.Use(); // Fixes copy/paste context menu appearing in Unity 5.6.6f2 - doesn't occur in 2018.3.2f1 Probably needs to be used in other places.
                            }
                            else if (!IsHoveringNode)
                            {
                                _autoConnectOutput = null;
                                GenericMenu menu = new GenericMenu();
                                graphEditor.AddContextMenuItems(menu);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                            }
                        }

                        isPanning = false;
                    }

                    // Reset DoubleClick
                    _isDoubleClick = false;
                    break;

                case EventType.KeyDown:

                    if (EditorGUIUtility.editingTextField || GUIUtility.keyboardControl != 0)
                    {
                        break;
                    }

                    if (e.keyCode == KeyCode.F)
                    {
                        Home();
                    }

                    if (NodeEditorUtilities.IsMac())
                    {
                        if (e.keyCode == KeyCode.Return)
                        {
                            RenameSelectedNode();
                        }
                    }
                    else
                    {
                        if (e.keyCode == KeyCode.F2)
                        {
                            RenameSelectedNode();
                        }
                    }

                    if (e.keyCode == KeyCode.A)
                    {
                        if (Selection.objects.Any(x => graph.nodes.Contains(x as Node)))
                        {
                            foreach (Node node in graph.nodes)
                            {
                                DeselectNode(node);
                            }
                        }
                        else
                        {
                            foreach (Node node in graph.nodes)
                            {
                                SelectNode(node, true);
                            }
                        }

                        Repaint();
                    }

                    break;

                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:

                    if (e.commandName == "SoftDelete")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                        }

                        e.Use();
                    }
                    else if (NodeEditorUtilities.IsMac() && e.commandName == "Delete")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                        }

                        e.Use();
                    }
                    else if (e.commandName == "Duplicate")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            DuplicateSelectedNodes();
                        }

                        e.Use();
                    }
                    else if (e.commandName == "Copy")
                    {
                        if (!EditorGUIUtility.editingTextField)
                        {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                CopySelectedNodes();
                            }

                            e.Use();
                        }
                    }
                    else if (e.commandName == "Paste")
                    {
                        if (!EditorGUIUtility.editingTextField)
                        {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                PasteNodes(WindowToGridPosition(_lastMousePosition));
                            }

                            e.Use();
                        }
                    }

                    Repaint();
                    break;

                case EventType.Ignore:

                    // If release mouse outside window
                    if (e.rawType == EventType.MouseUp && currentActivity == NodeActivity.DragGrid)
                    {
                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    }

                    break;
            }
        }

        protected virtual void RecalculateDragOffsets(Event current)
        {
            dragOffset = new Vector2[Selection.objects.Length + selectedReroutes.Count];
            // Selected nodes
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] is Node)
                {
                    Node node = Selection.objects[i] as Node;
                    dragOffset[i] = node.position - WindowToGridPosition(current.mousePosition);
                }
            }

            // Selected reroutes
            for (int i = 0; i < selectedReroutes.Count; i++)
            {
                dragOffset[Selection.objects.Length + i] =
                    selectedReroutes[i].GetPoint() - WindowToGridPosition(current.mousePosition);
            }
        }

        /// <summary> Puts all selected nodes in focus. If no nodes are present, resets view and zoom to to origin </summary>
        public virtual void Home()
        {
            var nodes = Selection.objects.Where(o => o is Node).Cast<Node>().ToList();
            if (nodes.Count > 0)
            {
                Vector2 minPos = nodes.Select(x => x.position)
                    .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
                Vector2 maxPos = nodes
                    .Select(x => x.position + (nodeSizes.ContainsKey(x) ? nodeSizes[x] : Vector2.zero))
                    .Aggregate((x, y) => new Vector2(Mathf.Max(x.x, y.x), Mathf.Max(x.y, y.y)));
                panOffset = -(minPos + (maxPos - minPos) / 2f);
            }
            else
            {
                zoom = 2;
                panOffset = Vector2.zero;
            }
        }

        /// <summary> Remove nodes in the graph in Selection.objects</summary>
        public virtual void RemoveSelectedNodes()
        {
            // We need to delete reroutes starting at the highest point index to avoid shifting indices
            selectedReroutes = selectedReroutes.OrderByDescending(x => x.pointIndex).ToList();
            for (int i = 0; i < selectedReroutes.Count; i++)
            {
                selectedReroutes[i].RemovePoint();
            }

            selectedReroutes.Clear();
            foreach (Object item in Selection.objects)
            {
                if (item is Node)
                {
                    Node node = item as Node;
                    graphEditor.RemoveNode(node);
                }
            }
        }

        /// <summary> Initiate a rename on the currently selected node </summary>
        public virtual void RenameSelectedNode()
        {
            if (Selection.objects.Length == 1 && Selection.activeObject is Node)
            {
                Node node = Selection.activeObject as Node;
                Vector2 size;
                if (nodeSizes.TryGetValue(node, out size))
                {
                    RenamePopup.Show(Selection.activeObject, size.x);
                }
                else
                {
                    RenamePopup.Show(Selection.activeObject);
                }
            }
        }

        /// <summary> Draw this node on top of other nodes by placing it last in the graph.nodes list </summary>
        public virtual void MoveNodeToTop(Node node)
        {
            int index;
            while ((index = graph.nodes.IndexOf(node)) != graph.nodes.Count - 1)
            {
                graph.nodes[index] = graph.nodes[index + 1];
                graph.nodes[index + 1] = node;
            }
        }

        /// <summary> Duplicate selected nodes and select the duplicates </summary>
        public virtual void DuplicateSelectedNodes()
        {
            // Get selected nodes which are part of this graph
            Node[] selectedNodes = Selection.objects.OfType<Node>().Where(x => x.graph == graph).ToArray();
            if (selectedNodes == null || selectedNodes.Length == 0)
            {
                return;
            }

            // Get top left node position
            Vector2 topLeftNode = selectedNodes.Select(x => x.position)
                .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            InsertDuplicateNodes(selectedNodes, topLeftNode + new Vector2(30, 30));
        }

        public virtual void CopySelectedNodes()
        {
            copyBuffer = Selection.objects.OfType<Node>().Where(x => x.graph == graph).ToArray();
        }

        public virtual void PasteNodes(Vector2 pos)
        {
            InsertDuplicateNodes(copyBuffer, pos);
        }

        protected virtual void InsertDuplicateNodes(Node[] nodes, Vector2 topLeft)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }

            // Get top-left node
            Vector2 topLeftNode = nodes.Select(x => x.position)
                .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            Vector2 offset = topLeft - topLeftNode;

            Object[] newNodes = new Object[nodes.Length];
            Dictionary<Node, Node> substitutes = new Dictionary<Node, Node>();
            for (int i = 0; i < nodes.Length; i++)
            {
                Node srcNode = nodes[i];
                if (srcNode == null)
                {
                    continue;
                }

                // Check if user is allowed to add more of given node type
                Node.DisallowMultipleNodesAttribute disallowAttrib;
                Type nodeType = srcNode.GetType();
                if (NodeEditorUtilities.GetAttrib(nodeType, out disallowAttrib))
                {
                    int typeCount = graph.nodes.Count(x => x.GetType() == nodeType);
                    if (typeCount >= disallowAttrib.max)
                    {
                        continue;
                    }
                }

                Node newNode = graphEditor.CopyNode(srcNode);
                substitutes.Add(srcNode, newNode);
                newNode.position = srcNode.position + offset;
                newNodes[i] = newNode;
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                Node srcNode = nodes[i];
                if (srcNode == null)
                {
                    continue;
                }

                foreach (NodePort port in srcNode.Ports)
                {
                    for (int c = 0; c < port.ConnectionCount; c++)
                    {
                        NodePort inputPort = port.direction == NodePort.IO.Input ? port : port.GetConnection(c);
                        NodePort outputPort = port.direction == NodePort.IO.Output ? port : port.GetConnection(c);

                        Node newNodeIn, newNodeOut;
                        if (substitutes.TryGetValue(inputPort.node, out newNodeIn) &&
                            substitutes.TryGetValue(outputPort.node, out newNodeOut))
                        {
                            newNodeIn.UpdatePorts();
                            newNodeOut.UpdatePorts();
                            inputPort = newNodeIn.GetInputPort(inputPort.fieldName);
                            outputPort = newNodeOut.GetOutputPort(outputPort.fieldName);
                        }

                        if (!inputPort.IsConnectedTo(outputPort))
                        {
                            inputPort.Connect(outputPort);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(graph);
            // Select the new nodes
            Selection.objects = newNodes;
        }

        /// <summary> Draw a connection as we are dragging it </summary>
        public virtual void DrawDraggedConnection()
        {
            if (IsDraggingPort)
            {
                Gradient gradient = graphEditor.GetConnectionGradient(_draggedOutput, null);
                float thickness = graphEditor.GetConnectionThickness(_draggedOutput, null);
                ConnectionPath path = graphEditor.GetConnectionPath(_draggedOutput, null);
                ConnectionStroke stroke = graphEditor.GetConnectionStroke(_draggedOutput, null);

                Rect fromRect;
                if (!_portConnectionPoints.TryGetValue(_draggedOutput, out fromRect))
                {
                    return;
                }

                List<Vector2> gridPoints = new List<Vector2>();
                gridPoints.Add(fromRect.center);
                for (int i = 0; i < _draggedOutputReroutes.Count; i++)
                {
                    gridPoints.Add(_draggedOutputReroutes[i]);
                }

                if (_draggedOutputTarget != null)
                {
                    gridPoints.Add(portConnectionPoints[_draggedOutputTarget].center);
                }
                else gridPoints.Add(WindowToGridPosition(Event.current.mousePosition));

                DrawConnection(gradient, path, stroke, thickness, gridPoints);

                GUIStyle portStyle = current.graphEditor.GetPortStyle(_draggedOutput);
                Color bgcol = Color.black;
                Color frcol = gradient.colorKeys[0].color;
                bgcol.a = 0.6f;
                frcol.a = 0.6f;

                // Loop through reroute points again and draw the points
                for (int i = 0; i < _draggedOutputReroutes.Count; i++)
                {
                    // Draw reroute point at position
                    Rect rect = new Rect(_draggedOutputReroutes[i], new Vector2(16, 16));
                    rect.position = new Vector2(rect.position.x - 8, rect.position.y - 8);
                    rect = GridToWindowRect(rect);

                    NodeEditorGUILayout.DrawPortHandle(rect, bgcol, frcol, portStyle.normal.background,
                        portStyle.active.background);
                }
            }
        }

        protected virtual bool IsHoveringTitle(Node node)
        {
            Vector2 mousePos = Event.current.mousePosition;
            //Get node position
            Vector2 nodePos = GridToWindowPosition(node.position);
            float width;
            Vector2 size;
            if (nodeSizes.TryGetValue(node, out size)) width = size.x;
            else width = 200;
            Rect windowRect = new Rect(nodePos, new Vector2(width / zoom, 30 / zoom));
            return windowRect.Contains(mousePos);
        }

        /// <summary> Attempt to connect dragged output to target node </summary>
        public virtual void AutoConnect(Node node)
        {
            if (_autoConnectOutput == null) return;

            // Find compatible input port
            NodePort inputPort =
                node.Ports.FirstOrDefault(x => x.IsInput && graphEditor.CanConnect(_autoConnectOutput, x));
            if (inputPort != null)
            {
                _autoConnectOutput.Connect(inputPort);
            }

            // Save changes
            EditorUtility.SetDirty(graph);
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }

            _autoConnectOutput = null;
        }
    }
}