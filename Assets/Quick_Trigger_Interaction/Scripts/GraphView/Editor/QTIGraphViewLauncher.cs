// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Triggers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Interaction = AstralShift.QTI.Interactions.Interaction;

namespace AstralShift.QTI.NodeEditor
{
    [InitializeOnLoad]
    public class QTIGraphViewLauncher
    {
        private static List<QTIGraphViewWindow> _windows;
        private static bool _isRunningPingTimeout;
        private static bool _newPingTimeout;

        static QTIGraphViewLauncher()
        {
            CacheNodeTypes();
        }

        private static Dictionary<Type, Type> componentNodeTypes;
        private static Dictionary<Type, Type> nodeComponentTypes;

        public static void CacheNodeTypes()
        {
            componentNodeTypes = new Dictionary<Type, Type>();
            nodeComponentTypes = new Dictionary<Type, Type>();

            Type[] nodeTypes = typeof(Node).GetDerivedTypes();

            for (int i = 0; i < nodeTypes.Length; i++)
            {
                if (nodeTypes[i].IsAbstract)
                {
                    continue;
                }

                var attribs = nodeTypes[i].GetCustomAttributes(typeof(Node.InteractionComponent), false);
                if (attribs.Length == 0)
                {
                    continue;
                }

                for (int j = 0; j < attribs.Length; j++)
                {
                    if (attribs[j] is Node.InteractionComponent attribute)
                    {
                        componentNodeTypes.Add(attribute.GetComponentType(), nodeTypes[i]);
                        nodeComponentTypes.Add(nodeTypes[i], attribute.GetComponentType());
                    }
                }
            }
        }

        public static Type GetNodeType(Type componentType)
        {
            if (componentNodeTypes.TryGetValue(componentType, out Type result))
            {
                return result;
            }

            if (typeof(Interaction).IsAssignableFrom(componentType))
            {
                return typeof(InteractionNode);
            }

            if (typeof(InteractionTrigger).IsAssignableFrom(componentType))
            {
                return typeof(InteractionTriggerNode);
            }

            return null;
        }

        public static Type GetInteractionType(Type nodeType)
        {
            if (nodeComponentTypes.TryGetValue(nodeType, out Type result))
            {
                return result;
            }

            if (typeof(InteractionNode).IsAssignableFrom(nodeType))
            {
                return typeof(Interaction);
            }

            if (typeof(InteractionTriggerNode).IsAssignableFrom(nodeType))
            {
                return typeof(InteractionTrigger);
            }

            return null;
        }

        /// <summary>
        /// Initialize the Graph View Window
        /// </summary>
        /// <param name="interaction">Entry interaction</param>
        public static void OpenEditor(Interaction interaction)
        {
            if (_windows == null)
            {
                _windows = new List<QTIGraphViewWindow>();
            }

            QTIGraphViewWindow window = GetExistingTree(interaction);

            if (window == null)
            {
                window = CreateWindow(interaction);
            }

            window.Show();

            Selection.SetActiveObjectWithContext(interaction, interaction);
        }

        /// <summary>
        /// Initialize the Graph View Window
        /// </summary>
        /// <param name="trigger">Entry trigger</param>
        public static void OpenEditor(InteractionTrigger trigger)
        {
            if (_windows == null)
            {
                _windows = new List<QTIGraphViewWindow>();
            }

            QTIGraphViewWindow window = GetExistingTree(trigger.interaction);

            if (window == null)
            {
                window = CreateWindow(trigger.interaction);
            }

            window.Show();

            Selection.SetActiveObjectWithContext(trigger.interaction, trigger.interaction);
        }

        private static QTIGraphViewWindow CreateWindow(Interaction interaction)
        {
            var window = ScriptableObject.CreateInstance<QTIGraphViewWindow>();
            window.titleContent.text = "QTI Graph View";
            Vector2 currentResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            Vector2 windowMinSize = new Vector2(currentResolution.x / 3, currentResolution.y / 3);
            Vector2 windowPreferredSize = new Vector2(currentResolution.x / 2, (currentResolution.y / 2));
            window.minSize = new Vector2(windowMinSize.x, windowMinSize.y);
            window.position = new Rect(0.5f, 0.5f, windowPreferredSize.x, windowPreferredSize.y);
            window.Setup(interaction);
            _windows.Add(window);
            return window;
        }

        public static QTIGraphViewWindow GetExistingTree(Interaction interaction)
        {
            // Perform a windows list cleanup (Remove null references)
            for (int i = _windows.Count - 1; i >= 0; i--)
            {
                if (_windows[i] == null)
                {
                    _windows.RemoveAt(i);
                }
            }

            // Search if any of the opened windows has the given interaction
            foreach (var window in _windows)
            {
                var tree = window.GetTree();

                foreach (var level in tree)
                {
                    foreach (var element in level)
                    {
                        if (element.component == interaction)
                        {
                            return window;
                        }
                    }
                }
            }

            return null;
        }

        public static async void PingInteractionComponent(Component component)
        {
            // Exits window GUIs early to prevent layout groups of throwing exceptions
            foreach (var window in _windows)
            {
                window.ForceExitGUI();
            }

            EditorGUIUtility.PingObject(component.GetInstanceID());
            Selection.SetActiveObjectWithContext(component, component);

            // Delay a bit to avoid GUI errors
            await Task.Delay(50);

            Highlighter.Highlight("Inspector", component.GetInstanceID().ToString(), HighlightSearchMode.Identifier);

            if (_isRunningPingTimeout)
            {
                _newPingTimeout = true;
            }
            else
            {
                await StopPingInteractionComponent();
            }
        }

        private static async Task StopPingInteractionComponent()
        {
            _isRunningPingTimeout = true;
            for (int i = 0; i < 80; i++)
            {
                if (_newPingTimeout)
                {
                    i = 0;
                    _newPingTimeout = false;
                    continue;
                }

                await Task.Delay(50);
            }

            Highlighter.Stop();
            _isRunningPingTimeout = false;
        }
    }
}