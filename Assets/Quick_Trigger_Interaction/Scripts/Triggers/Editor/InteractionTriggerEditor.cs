// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactions;
using AstralShift.QTI.NodeEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Interaction = AstralShift.QTI.Interactions.Interaction;

namespace AstralShift.QTI.Triggers
{
    [CustomEditor(typeof(InteractionTrigger), true), CanEditMultipleObjects]
    public class InteractionTriggerEditor : Editor
    {
        protected InteractionTrigger _targetScript;

        public override VisualElement CreateInspectorGUI()
        {
            if (_targetScript == null)
            {
                _targetScript = target as InteractionTrigger;
            }

            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUIHeader();
            DrawProperties();
            DrawFooter();
        }

        /// <summary>
        /// Draw Inspector GUI Header
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="script"></param>
        protected virtual void OnInspectorGUIHeader()
        {
            EditorUtils.Generic.SetHighlighterIdentifier(target);
            DrawBanner();
            DrawGraphViewButton();
            EditorHelpers.DrawDefaultScriptReadonlyObject(target);
        }

        public virtual void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty copiedProperty = serializedObject.GetIterator().Copy();
            bool visitChild = true;
            copiedProperty.NextVisible(visitChild);
            visitChild = false;

            do
            {
                // Ignore 
                if (copiedProperty.displayName == "Script")
                {
                    continue;
                }

                NodeEditorGUILayout.PropertyField(copiedProperty);
            } while (copiedProperty.NextVisible(visitChild));

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawBanner()
        {
            float height = 40;
            Rect newRect = new Rect(0, 10, EditorGUIUtility.currentViewWidth, height);
            GUI.DrawTexture(newRect, QTIEditorResources.General.FullLogo, ScaleMode.ScaleToFit);
            GUILayoutUtility.GetRect(height + 25, height + 15, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Draw View Graph Button
        /// </summary>
        protected virtual void DrawGraphViewButton()
        {
            if (targets.Length > 1)
            {
                return;
            }

            // Draw "View Graph" button
            if (GUILayout.Button(QTIEditorResources.General.Styles.GraphViewButtonContent))
            {
                QTIGraphViewLauncher.OpenEditor(target as InteractionTrigger);
            }
        }

        protected const int ButtonHeight = 24;

        protected virtual void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();

            DrawAddButton();
            DrawAddInBetweenButton();
            DrawRemoveButton();
            DrawReplaceButton();

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawAddButton()
        {
            var addImage = QTIEditorResources.General.AddIcon;
            GUIContent content = new GUIContent(addImage,
                "Add a new interaction. Attaches it to the trigger if there's no interaction attached.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(), typeof(Interaction),
                    (context) => { EditorUtils.Triggers.AddInteraction(target as InteractionTrigger, context); },
                    "Interaction",
                    "Interactions");

                var rect = new Rect(Event.current.mousePosition, Vector2.zero);
                dropdown.Show(rect);
            }
        }

        protected virtual void DrawAddInBetweenButton()
        {
            var addBetweenImage = QTIEditorResources.General.AddBetweenIcon;
            GUIContent content = new GUIContent(addBetweenImage,
                "Add a new interaction in between the trigger and the current interaction.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(),
                    typeof(Interaction),
                    (context) =>
                    {
                        EditorUtils.Triggers.AddInteractionInBetween(target as InteractionTrigger, context);
                    },
                    "Interaction",
                    "Interactions");

                var rect = new Rect(Event.current.mousePosition, Vector2.zero);
                dropdown.Show(rect);
            }
        }

        protected virtual void DrawRemoveButton()
        {
            var removeImage = QTIEditorResources.General.RemoveIcon;
            GUIContent content = new GUIContent(removeImage, "Remove an interaction attached to this game object.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                EditorUtils.Triggers.RemoveInteraction(target as InteractionTrigger);
            }
        }

        protected virtual void DrawReplaceButton()
        {
#if !UNITY_2022_3_OR_NEWER
            if (PrefabUtility.GetPrefabInstanceStatus(target) != PrefabInstanceStatus.Connected)
            {
#endif
            var replaceImage = QTIEditorResources.General.ReplaceIcon;
            GUIContent content = new GUIContent(replaceImage, "Replace this trigger with a different one.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(),
                    typeof(InteractionTrigger),
                    (context) => { EditorUtils.Triggers.ReplaceTrigger(target as InteractionTrigger, context); },
                    "Trigger",
                    "Triggers");

                var rect = new Rect(Event.current.mousePosition, Vector2.zero);
                dropdown.Show(rect);
            }

#if !UNITY_2022_3_OR_NEWER
            }
#endif
        }
    }
}