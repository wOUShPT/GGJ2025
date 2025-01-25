// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Interaction = AstralShift.QTI.Interactions.Interaction;
using InteractionTrigger = AstralShift.QTI.Triggers.InteractionTrigger;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    public class QTIGraphViewWindow : NodeEditorWindow
    {
        private Interaction _targetInteraction;
        private Interaction _inspectedInteraction;
        private List<InteractionTrigger> _interactionTriggers;
        private InteractionsNodeGraphEditor _graphEditor;
        private QTIGraphViewWindow _window;

        private static Interaction[] _allInteractions;
        private static InteractionTrigger[] _allInteractionTriggers;
        private InteractionBaseNode _inspectedNode;
        private List<List<InteractionBaseNode>> _nodeTree;
        private List<Object> _selectedObjects = new List<Object>();
        private int _OnGUIUpdateCounter;

        #region Init

        public void Setup(Interaction interaction)
        {
            _inspectedInteraction = interaction;
            _targetInteraction = interaction;
            _allInteractions =
                GameObjectHelpers.GetAllComponentsOfTypeInScene<Interaction>(_inspectedInteraction.gameObject, true);
            if (_allInteractions == null)
            {
                return;
            }

            _allInteractionTriggers =
                GameObjectHelpers.GetAllComponentsOfTypeInScene<InteractionTrigger>(_inspectedInteraction.gameObject,
                    true);
            _targetInteraction = BacktraceInteraction(_inspectedInteraction, new List<int>());
            _interactionTriggers = FindInteractionTriggers(_targetInteraction);
            PopulateGraph(_interactionTriggers.ToArray(), null, 0);
            SetInspectedNode(interaction);
        }

        /// <summary>
        /// Creates all trigger nodes from a given InteractionTrigger array if they exist and recursively creates all Interaction children nodes.
        /// Connects each child node to its parent node.
        /// </summary>
        /// <param name="triggers"></param>
        /// <param name="previousNodePort"></param>
        /// <param name="treeLevel"></param>
        private void PopulateGraph(InteractionTrigger[] triggers, NodePort previousNodePort, int treeLevel)
        {
            // Creates graph if doesnt exist an populates with all trigger triggers nodes, creates a node for the first trigger and then connects it to the previous ones
            if (graph == null)
            {
                graph = CreateInstance<InteractionsNodeGraph>();
            }

            if (triggers == null || triggers.Length == 0)
            {
                PopulateGraph(_targetInteraction, previousNodePort, treeLevel);
                return;
            }

            //Creates triggers level
            InteractionTriggerNode currentNode = null;
            List<InteractionBaseNode> triggerNodes = new List<InteractionBaseNode>();
            for (int i = 0; i < _interactionTriggers.Count; i++)
            {
                if (TryInstantiateNode(_interactionTriggers[i], out currentNode))
                {
                    triggerNodes.Add(currentNode);
                }

                AddToTree(currentNode, treeLevel);
            }

            treeLevel++;

            //Iterate all triggers and recursively populates the graph starting on the first interactions
            foreach (var node in triggerNodes)
            {
                NodePort interactionPort = node.GetOutputPort("exit");
                InteractionTrigger trigger = node.component as InteractionTrigger;
                PopulateGraph(trigger.interaction, interactionPort, treeLevel);
            }
        }

        /// <summary>
        /// Follows recursively the Interactions chain and creates an Interaction Node to each Interaction
        /// </summary>
        /// <param name="interaction" Current trigger></param>
        /// <param name="previousNodePort">Previous Node Exit Port</param>
        /// <param name="treeLevel">Current Tree Level</param>
        private void PopulateGraph(Interaction interaction, NodePort previousNodePort, int treeLevel)
        {
            if (interaction == null)
            {
                return;
            }

            InteractionNode currentNode = null;

            if (TryInstantiateNode(interaction, out currentNode))
            {
                ConnectNodeToParent(previousNodePort, currentNode);
                AddToTree(currentNode, treeLevel);
                treeLevel++;

                // Handle ConditionInteraction Exception
                if (interaction is AstralShift.QTI.Interactions.ConditionInteraction conditionInteraction)
                {
                    previousNodePort = currentNode.GetOutputPort("OnTrue");
                    for (int i = 0; i < conditionInteraction.onTrueInteractions.Count; i++)
                    {
                        PopulateGraph(conditionInteraction.onTrueInteractions[i], previousNodePort, treeLevel);
                    }

                    previousNodePort = currentNode.GetOutputPort("OnFalse");
                    for (int i = 0; i < conditionInteraction.onFalseInteractions.Count; i++)
                    {
                        PopulateGraph(conditionInteraction.onFalseInteractions[i], previousNodePort, treeLevel);
                    }
                }
                else
                {
                    previousNodePort = currentNode.GetOutputPort("exit");
                    for (int i = 0; i < interaction.onEndInteractions.Count; i++)
                    {
                        PopulateGraph(interaction.onEndInteractions[i], previousNodePort, treeLevel);
                    }
                }
            }
            else if (currentNode != null)
            {
                ConnectNodeToParent(previousNodePort, currentNode);
            }
        }

        /// <summary>
        /// Finds recursively the first trigger in chain
        /// </summary>
        /// <param name="referenceInteraction"></param>
        /// <param name="occurrencesTracker"></param>
        private Interaction BacktraceInteraction(Interaction referenceInteraction, List<int> occurrencesTracker)
        {
            if (referenceInteraction == null)
            {
                return null;
            }

            for (int i = 0; i < _allInteractions.Length; i++)
            {
                if (_allInteractions[i] == null || _allInteractions[i] is not Interaction interaction)
                {
                    continue;
                }

                if (occurrencesTracker.Contains(interaction.GetInstanceID()))
                {
                    return referenceInteraction;
                }

                // Handle ConditionInteraction Branch Exception
                if (interaction is AstralShift.QTI.Interactions.ConditionInteraction conditionInteraction)
                {
                    if (conditionInteraction.onTrueInteractions == null &&
                        conditionInteraction.onFalseInteractions == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < conditionInteraction.onTrueInteractions.Count; j++)
                    {
                        if (conditionInteraction.onTrueInteractions[j] == null)
                        {
                            continue;
                        }

                        if (conditionInteraction.onTrueInteractions[j].GetInstanceID() ==
                            referenceInteraction.GetInstanceID())
                        {
                            occurrencesTracker.Add(interaction.GetInstanceID());
                            return BacktraceInteraction(interaction, occurrencesTracker);
                        }
                    }

                    for (int j = 0; j < conditionInteraction.onFalseInteractions.Count; j++)
                    {
                        if (conditionInteraction.onFalseInteractions[j] == null)
                        {
                            continue;
                        }

                        if (conditionInteraction.onFalseInteractions[j].GetInstanceID() ==
                            referenceInteraction.GetInstanceID())
                        {
                            occurrencesTracker.Add(interaction.GetInstanceID());
                            return BacktraceInteraction(interaction, occurrencesTracker);
                        }
                    }
                }
                else
                {
                    if (interaction.onEndInteractions == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < interaction.onEndInteractions.Count; j++)
                    {
                        if (interaction.onEndInteractions[j] == null)
                        {
                            continue;
                        }

                        if (interaction.onEndInteractions[j].GetInstanceID() == referenceInteraction.GetInstanceID())
                        {
                            occurrencesTracker.Add(interaction.GetInstanceID());
                            return BacktraceInteraction(interaction, occurrencesTracker);
                        }
                    }
                }
            }

            return referenceInteraction;
        }

        /// <summary>
        /// Finds the Interaction Trigger linked to a given trigger
        /// </summary>
        /// <param name="referenceInteraction"></param>
        private List<InteractionTrigger> FindInteractionTriggers(Interaction referenceInteraction)
        {
            List<InteractionTrigger> interactionTriggers = new List<InteractionTrigger>();

            foreach (var interactionTrigger in _allInteractionTriggers)
            {
                if (interactionTrigger.interaction == null)
                {
                    continue;
                }

                if (interactionTrigger.interaction.GetInstanceID() == referenceInteraction.GetInstanceID())
                {
                    interactionTriggers.Add(interactionTrigger);
                }
            }

            return interactionTriggers;
        }

        /// <summary>
        /// Adds a new Interaction Node to the Interaction Graph
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool TryInstantiateNode<T>(Interaction interaction, out T node) where T : InteractionNode
        {
            if (interaction == null)
            {
                node = default;
                return false;
            }

            // Dont create if already exists, outputs the existing node and returns false
            foreach (var graphNode in graph.nodes)
            {
                if (graphNode is InteractionBaseNode interactionBaseNode &&
                    interactionBaseNode.component.GetInstanceID() == interaction.GetInstanceID())
                {
                    node = graphNode as T;
                    return false;
                }
            }

            Type type = QTIGraphViewLauncher.GetNodeType(interaction.GetType());
            var newNode = graph.AddNode(type) as T;
            newNode.Initialize(interaction);
            node = newNode;

            return true;
        }

        /// <summary>
        /// Adds a new Interaction Node to the Interaction Graph and connects it to a given previous Node
        /// </summary>
        /// <param name="interactionTrigger"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool TryInstantiateNode<T>(InteractionTrigger interactionTrigger, out T node)
            where T : InteractionTriggerNode
        {
            if (interactionTrigger == null)
            {
                node = null;
                return false;
            }

            // Dont create node if already exists
            foreach (var graphNode in graph.nodes)
            {
                if (graphNode is InteractionBaseNode interactionBaseNode &&
                    interactionBaseNode.component.GetInstanceID() == interactionTrigger.GetInstanceID())
                {
                    node = graphNode as T;
                    return false;
                }
            }

            Type type = QTIGraphViewLauncher.GetNodeType(interactionTrigger.GetType());
            var newNode = graph.AddNode(type) as T;
            newNode.Initialize(interactionTrigger);
            node = newNode;

            return true;
        }

        private bool ConnectNodeToParent(NodePort parentNodePort, Node currentNode)
        {
            if (parentNodePort == null)
            {
                return false;
            }

            int parentToChildConnectionsNumber = parentNodePort.ConnectionCount;

            for (int i = 0; i < parentToChildConnectionsNumber; i++)
            {
                if (parentNodePort.GetConnection(i).node == currentNode)
                {
                    return false;
                }
            }

            NodePort currentNodePort = currentNode.GetInputPort("entry");
            parentNodePort.Connect(currentNodePort);
            return true;
        }

        public void CleanTree()
        {
            graph?.Clear();
            graph = null;
            _allInteractions = null;
            _allInteractionTriggers = null;
            _interactionTriggers = null;
            _portConnectionPoints?.Clear();
            nodeSizes?.Clear();
            _nodeTree?.Clear();
            _nodeTree = null;
        }

        public void AddToTree<T>(T node, int level) where T : InteractionBaseNode
        {
            if (_nodeTree == null)
            {
                _nodeTree = new List<List<InteractionBaseNode>>();
            }

            if (level > _nodeTree.Count - 1)
            {
                _nodeTree.Add(new List<InteractionBaseNode>());
            }

            if (!_nodeTree[level].Contains(node))
            {
                _nodeTree[level].Add(node);
            }
        }

        public List<List<InteractionBaseNode>> GetTree()
        {
            return _nodeTree;
        }

        public void SetInspectedNode(Interaction interaction)
        {
            foreach (var node in graph.nodes)
            {
                if (node is InteractionBaseNode interactionNode &&
                    interactionNode.component.GetInstanceID() == interaction.GetInstanceID())
                {
                    _inspectedNode = node as InteractionBaseNode;
                }
            }
        }

        public void SetInspectedNode(InteractionTrigger trigger)
        {
            foreach (var node in graph.nodes)
            {
                if (node is InteractionBaseNode interactionNode &&
                    interactionNode.component.GetInstanceID() == trigger.GetInstanceID())
                {
                    _inspectedNode = node as InteractionBaseNode;
                }
            }
        }

        #endregion

        #region GUI

        protected override void OnGUI()
        {
            Matrix4x4 defaultMatrix = GUI.matrix;
            if (graph == null)
            {
                //return;
            }

            ValidateGraphEditor();
            Controls();

            DrawGrid(position, zoom, panOffset);
            DrawNodes();
            DrawConnections();

            RunAndResetOnLateGUI();
            GUI.matrix = defaultMatrix;

            if (_OnGUIUpdateCounter <= 3)
            {
                _OnGUIUpdateCounter++;
                if (_OnGUIUpdateCounter == 3)
                {
                    SetNodesPosition();
                    //FocusOnInspectedNode();
                    FocusOnMidPoint();
                }
            }
        }


        /// <summary>
        /// Controls base method override.
        /// Nodes dragging/selection removed
        /// </summary>
        protected override void Controls()
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
                        if (currentActivity == NodeActivity.HoldNode)
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
                            for (int i = 0; i < _selectedObjects.Count; i++)
                            {
                                if (_selectedObjects[i] is Node)
                                {
                                    Node node = _selectedObjects[i] as Node;
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
                                Vector2 pos = mousePos + dragOffset[_selectedObjects.Count + i];
                                if (gridSnap)
                                {
                                    pos.x = (Mathf.Round(pos.x / 16) * 16);
                                    pos.y = (Mathf.Round(pos.y / 16) * 16);
                                }

                                selectedReroutes[i].SetPoint(pos);
                            }

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

                        if (IsHoveringNode && IsHoveringTitle(_hoveredNode))
                        {
                            // If mousedown on node header, select or deselect
                            if (!_selectedObjects.Contains(_hoveredNode))
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
                        else if (!IsHoveringNode) // If mousedown on grid background, deselect all
                        {
                            currentActivity = NodeActivity.HoldGrid;
                            if (!e.control && !e.shift)
                            {
                                selectedReroutes.Clear();
                                Selection.activeObject = null;
                                Selection.SetActiveObjectWithContext(_inspectedInteraction, _inspectedInteraction);
                            }
                        }
                    }

                    break;

                case EventType.MouseUp:

                    if (e.button == 0)
                    {
                        if (currentActivity == NodeActivity.DragNode)
                        {
                            IEnumerable<Node> nodes = _selectedObjects.OfType<Node>();
                            foreach (Node node in nodes)
                            {
                                EditorUtility.SetDirty(node);
                                DeselectNode(node);
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
                        }

                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    }
                    else if (e.button == 1 || e.button == 2)
                    {
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
                        FocusOnInspectedNode();
                    }

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

        protected override void DrawNodes()
        {
            Event e = Event.current;
            if (e.type == EventType.Layout)
            {
                selectionCache = new List<Object>(_selectedObjects);
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

            Color guiColor = GUI.color;
            Color bgGUIColor = GUI.backgroundColor;

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

                var node = graph.nodes[n];

                // Skip culling before the first 2 updates to avoid visual bugs
                if (_OnGUIUpdateCounter > 2)
                {
                    // Culling
                    if (e.type == EventType.Layout)
                    {
                        // Cull unselected nodes outside view
                        if (!_selectedObjects.Contains(node) && ShouldBeCulled(node))
                        {
                            culledNodes.Add(node);
                            continue;
                        }
                    }
                    else if (culledNodes.Contains(node))
                    {
                        continue;
                    }
                }

                if (e.type == EventType.Repaint)
                {
                    removeEntries.Clear();
                    foreach (var kvp in _portConnectionPoints)
                    {
                        if (kvp.Key.node == node)
                        {
                            removeEntries.Add(kvp.Key);
                        }
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

                // Get node position
                Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

                // Draw node contents
                GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

                // Draw Node Header
                GUIStyle style = new GUIStyle(nodeEditor.GetHeaderStyle());
                GUI.color = nodeEditor.GetHeaderTint();
                GUILayout.BeginVertical(style);
                GUI.color = guiColor;
                EditorGUI.BeginChangeCheck();
                nodeEditor.OnHeaderGUI();
                try
                {
                    GUILayout.EndVertical();
                }
                catch (InvalidOperationException)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                // Draw Node Body
                GUI.color = nodeEditor.GetBodyTint();
                style = new GUIStyle(nodeEditor.GetBodyStyle());
                GUILayout.BeginVertical(style);
                GUI.color = bgGUIColor;
                nodeEditor.OnBodyGUI();
                // If user changed a value, notify other scripts through onUpdateNode
                if (EditorGUI.EndChangeCheck())
                {
                    if (NodeEditor.onUpdateNode != null)
                    {
                        NodeEditor.onUpdateNode(node);
                    }

                    EditorUtility.SetDirty(node);
                    nodeEditor.serializedObject.ApplyModifiedProperties();
                }

                // Prevents layout group methods of throwing exceptions (sometimes)
                try
                {
                    GUILayout.EndVertical();
                }
                catch (InvalidOperationException)
                {
                    GUIUtility.ExitGUI();
                    return;
                }

                // Cache data about the node for next frame
                if (e.type == EventType.Repaint)
                {
                    Vector2 size = GUILayoutUtility.GetLastRect().size;
                    size.x = nodeEditor.GetWidth();
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

                //if (e.type != EventType.Layout)
                //{
                //    //Check if we are hovering this node
                //    Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                //    Rect windowRect = new Rect(nodePos, nodeSize);
                //    if (windowRect.Contains(mousePos))
                //    {
                //        hoveredNode = node;
                //    }
                //
                //    //Check if we are hovering any of this nodes ports
                //    //Check input ports
                //    foreach (global::AstralShift.QTI.Interactions.NodeEditor.NodePort input in node.Inputs)
                //    {
                //        //Check if port rect is available
                //        if (!portConnectionPoints.ContainsKey(input))
                //        {
                //            continue;
                //        }
                //        Rect r = GridToWindowRectNoClipped(portConnectionPoints[input]);
                //        if (r.Contains(mousePos))
                //        {
                //            hoveredPort = input;
                //        }
                //    }
                //    //Check all output ports
                //    foreach (global::AstralShift.QTI.Interactions.NodeEditor.NodePort output in node.Outputs)
                //    {
                //        //Check if port rect is available
                //        if (!portConnectionPoints.ContainsKey(output))
                //        {
                //            continue;
                //        }
                //        Rect r = GridToWindowRectNoClipped(portConnectionPoints[output]);
                //        if (r.Contains(mousePos))
                //        {
                //            hoveredPort = output;
                //        }
                //    }
                //}

                // Prevents layout group methods of throwing exceptions (sometimes)
                try
                {
                    GUILayout.EndArea();
                }
                catch (InvalidOperationException)
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            EndZoomed(position, zoom, topPadding);

            // If a change in is detected in the selected node, call OnValidate method.
            // This is done through reflection because OnValidate is only relevant in editor,
            // and thus, the code should not be included in build.
            if (onValidate != null && EditorGUI.EndChangeCheck())
            {
                onValidate.Invoke(Selection.activeObject, null);
            }

            // Check changes in node linking structure and reset tree if changed
            foreach (var node in graph.nodes)
            {
                if (node is InteractionBaseNode interactionBaseNode && interactionBaseNode.component == null)
                {
                    ResetGraph();
                    return;
                }
            }
        }

        public void ResetGraph()
        {
            CleanTree();
            if (_targetInteraction == null)
            {
                Close();
                return;
            }

            Setup(_targetInteraction);
            _OnGUIUpdateCounter = 0;
        }

        /// <summary>
        /// Forces OnGUI to exit early on
        /// </summary>
        public void ForceExitGUI()
        {
            try
            {
                GUIUtility.ExitGUI();
            }
            catch (ExitGUIException)
            {
            }
        }

        public override void SelectNode(Node node, bool add)
        {
            if (_selectedObjects != null)
            {
                Selection.objects = _selectedObjects.ToArray();
            }

            if (add)
            {
                List<Object> selection = new List<Object>(Selection.objects);
                selection.Add(node);
                Selection.objects = selection.ToArray();
                _selectedObjects = selection;
            }
            else
            {
                Selection.objects = new Object[] { node };
                _selectedObjects = Selection.objects.ToList();
            }

            Selection.activeInstanceID = _inspectedInteraction.GetInstanceID();
        }

        public override void DeselectNode(Node node)
        {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node);
            _selectedObjects = selection;
            Selection.activeInstanceID = _inspectedInteraction.GetInstanceID();
        }

        public void FocusOnInspectedNode()
        {
            Vector2 minPos = _inspectedNode.position;
            Vector2 maxPos = _inspectedNode.position;
            panOffset = -(minPos + (maxPos - minPos) / 2f);
            zoom = 1.5f;
        }

        public void FocusOnMidPoint()
        {
            Vector2 sum = Vector2.zero;
            for (var i = 0; i < graph.nodes.Count; i++)
            {
                Vector2 minPos = graph.nodes[i].position;
                Vector2 maxPos = graph.nodes[i].position;
                sum += -(minPos + (maxPos - minPos) / 2f);
            }

            panOffset = sum / graph.nodes.Count;
            zoom = 1.5f;
        }

        #endregion

        #region Node Positioning

        /// <summary>
        /// Positions all nodes in a horizontal growing tree layout
        /// </summary>
        private void SetNodesPosition()
        {
            InteractionBaseNode firstNode = _nodeTree[0][0];
            AssignPositions(firstNode, 0, 0, out float totalHeight, new List<int>());
        }

        private float AssignPositions(InteractionBaseNode node, float startX, float startY, out float totalHeight,
            List<int> occurences)
        {
            occurences.Add(node.component.GetInstanceID());

            var children = node.GetChildren();
            if (children.Length == 0)
            {
                // Position a leaf node
                node.position.x = startX;
                node.position.y = startY;
                totalHeight = nodeSizes[node].y;
                return totalHeight;
            }

            // Calculate the total height required for all children
            float childrenHeight = 0;
            float maxChildWidth = 0;
            List<float> childHeights = new List<float>();
            List<float> childYPositions = new List<float>();

            float currentY = startY;

            foreach (var child in children)
            {
                if (occurences.Contains(child.component.GetInstanceID()))
                {
                    continue;
                }

                float childX = startX + nodeSizes[node].x + graphEditor.GetNodePadding().x;
                AssignPositions(child, childX, currentY, out float childTotalHeight, occurences);
                childHeights.Add(childTotalHeight);
                childYPositions.Add(currentY);
                childrenHeight += childTotalHeight + graphEditor.GetNodePadding().y;
                maxChildWidth = Mathf.Max(maxChildWidth, nodeSizes[node].x);
                currentY += childTotalHeight + graphEditor.GetNodePadding().y;
            }

            // Center the parent node vertically before its children
            float totalChildrenHeight = childrenHeight - graphEditor.GetNodePadding().y; // Adjust for last space
            float centerY = startY + (totalChildrenHeight - nodeSizes[node].y) / 2;
            node.position.x = startX;
            node.position.y = centerY;

            // Return total height including the node itself
            totalHeight = totalChildrenHeight + nodeSizes[node].y;
            return totalHeight;
        }

        #endregion
    }
}