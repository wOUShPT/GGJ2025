// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AstralShift.QTI.NodeEditor
{
    public partial class NodeEditorWindow : EditorWindow
    {
        public static NodeEditorWindow current;

        /// <summary> Stores node positions for all nodePorts. </summary>
        public Dictionary<NodePort, Rect> portConnectionPoints
        {
            get { return _portConnectionPoints; }
        }

        protected Dictionary<NodePort, Rect> _portConnectionPoints = new Dictionary<NodePort, Rect>();
        [SerializeField] protected NodePortReference[] _references = new NodePortReference[0];
        [SerializeField] protected Rect[] _rects = new Rect[0];

        private Func<bool> isDocked
        {
            get
            {
                if (_isDocked == null)
                {
                    _isDocked = this.GetIsDockedDelegate();
                }

                return _isDocked;
            }
        }

        private Func<bool> _isDocked;

        [Serializable]
        protected class NodePortReference
        {
            [SerializeField] private Node _node;
            [SerializeField] private string _name;

            public NodePortReference(NodePort nodePort)
            {
                _node = nodePort.node;
                _name = nodePort.fieldName;
            }

            public NodePort GetNodePort()
            {
                if (_node == null)
                {
                    return null;
                }

                return _node.GetPort(_name);
            }
        }

        protected virtual void OnDisable()
        {
            // Cache portConnectionPoints before serialization starts
            int count = portConnectionPoints.Count;
            _references = new NodePortReference[count];
            _rects = new Rect[count];
            int index = 0;
            foreach (var portConnectionPoint in portConnectionPoints)
            {
                _references[index] = new NodePortReference(portConnectionPoint.Key);
                _rects[index] = portConnectionPoint.Value;
                index++;
            }
        }

        protected virtual void OnEnable()
        {
            // Reload portConnectionPoints if there are any
            int length = _references.Length;
            if (length == _rects.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    NodePort nodePort = _references[i].GetNodePort();
                    if (nodePort != null)
                    {
                        _portConnectionPoints.Add(nodePort, _rects[i]);
                    }
                }
            }
        }

        public Dictionary<Node, Vector2> nodeSizes
        {
            get { return _nodeSizes; }
        }

        private Dictionary<Node, Vector2> _nodeSizes = new Dictionary<Node, Vector2>();
        public NodeGraph graph;

        public Vector2 panOffset
        {
            get { return _panOffset; }
            set
            {
                _panOffset = value;
                Repaint();
            }
        }

        private Vector2 _panOffset;

        public float zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Mathf.Clamp(value, NodeEditorPreferences.GetSettings().minZoom,
                    NodeEditorPreferences.GetSettings().maxZoom);
                Repaint();
            }
        }

        private float _zoom = 1;

        protected void OnFocus()
        {
            current = this;
            ValidateGraphEditor();
            if (graphEditor != null)
            {
                graphEditor.OnWindowFocus();
                if (NodeEditorPreferences.GetSettings().autoSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            _dragThreshold = Math.Max(1f, Screen.width / 1000f);
        }

        protected void OnLostFocus()
        {
            if (graphEditor != null)
            {
                graphEditor.OnWindowFocusLost();
            }
        }

        [InitializeOnLoadMethod]
        protected static void OnLoad()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary> Handle Selection Change events</summary>
        protected static void OnSelectionChanged()
        {
            NodeGraph nodeGraph = Selection.activeObject as NodeGraph;
            if (nodeGraph && !AssetDatabase.Contains(nodeGraph))
            {
                if (NodeEditorPreferences.GetSettings().openOnCreate)
                {
                    Open(nodeGraph);
                }
            }
        }

        /// <summary> Make sure the graph editor is assigned and to the right object </summary>
        protected virtual void ValidateGraphEditor()
        {
            NodeGraphEditor graphEditor = NodeGraphEditor.GetEditor(graph, this);
            if (this.graphEditor != graphEditor && graphEditor != null)
            {
                this.graphEditor = graphEditor;
                graphEditor.OnOpen();
            }
        }

        /// <summary> Create editor window </summary>
        public static NodeEditorWindow Init()
        {
            NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
            w.titleContent = new GUIContent("xNode");
            w.wantsMouseMove = true;
            w.Show();
            return w;
        }

        public void Save()
        {
            if (AssetDatabase.Contains(graph))
            {
                EditorUtility.SetDirty(graph);
                if (NodeEditorPreferences.GetSettings().autoSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                SaveAs();
            }
        }

        public virtual void SaveAs()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
            if (existingGraph != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(graph, path);
            EditorUtility.SetDirty(graph);
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }
        }

        protected virtual void DraggableWindow(int windowID)
        {
            GUI.DragWindow();
        }

        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
        }

        public Vector2 GridToWindowPosition(Vector2 gridPosition)
        {
            return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition / zoom);
        }

        public Rect GridToWindowRectNoClipped(Rect gridRect)
        {
            gridRect.position = GridToWindowPositionNoClipped(gridRect.position);
            return gridRect;
        }

        public Rect GridToWindowRect(Rect gridRect)
        {
            gridRect.position = GridToWindowPosition(gridRect.position);
            gridRect.size /= zoom;
            return gridRect;
        }

        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition)
        {
            Vector2 center = position.size * 0.5f;
            // UI Sharpness complete fix - Round final offset not panOffset
            float xOffset = Mathf.Round(center.x * zoom + (panOffset.x + gridPosition.x));
            float yOffset = Mathf.Round(center.y * zoom + (panOffset.y + gridPosition.y));
            return new Vector2(xOffset, yOffset);
        }

        public virtual void SelectNode(Node node, bool add)
        {
            if (add)
            {
                List<Object> selection = new List<Object>(Selection.objects);
                selection.Add(node);
                Selection.objects = selection.ToArray();
            }
            else
            {
                Selection.objects = new Object[] { node };
            }
        }

        public virtual void DeselectNode(Node node)
        {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node);
            Selection.objects = selection.ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraph;
            if (nodeGraph != null)
            {
                Open(nodeGraph);
                return true;
            }

            return false;
        }

        /// <summary>Open the provided graph in the NodeEditor</summary>
        public static NodeEditorWindow Open(NodeGraph graph)
        {
            if (!graph)
            {
                return null;
            }

            NodeEditorWindow w =
                GetWindow(typeof(NodeEditorWindow), false, "Interaction Graph View", true) as NodeEditorWindow;
            w.wantsMouseMove = true;
            w.graph = graph;
            return w;
        }

        /// <summary> Repaint all open NodeEditorWindows. </summary>
        public static void RepaintAll()
        {
            NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Repaint();
            }
        }
    }
}