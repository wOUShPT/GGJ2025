// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.NodeEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(Interaction), true), CanEditMultipleObjects]
    public class InteractionEditor : Editor
    {
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
            DrawGraphViewButton();
            EditorHelpers.DrawDefaultScriptReadonlyObject(target);
            serializedObject.Update();
            SerializedProperty onEndProp = serializedObject.FindProperty("onEndInteractions");
            EditorGUILayout.PropertyField(onEndProp);
            serializedObject.ApplyModifiedProperties();
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

            if (GUILayout.Button(QTIEditorResources.General.Styles.GraphViewButtonContent))
            {
                QTIGraphViewLauncher.OpenEditor(target as Interaction);
            }
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
                if (copiedProperty.displayName == "On End Interactions" ||
                    copiedProperty.displayName == "Script")
                {
                    continue;
                }

                NodeEditorGUILayout.PropertyField(copiedProperty);
            } while (copiedProperty.NextVisible(visitChild));

            serializedObject.ApplyModifiedProperties();
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
            GUIContent content = new GUIContent(addImage, "Add a new interaction that runs after this one.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(),
                    typeof(Interaction),
                    (context) => { EditorUtils.Interactions.AddInteraction(target as Interaction, context); },
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
                "Add a new interaction in between this one and the next ones in the chain.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(),
                    typeof(Interaction),
                    (context) => { EditorUtils.Interactions.AddInteractionInBetween(target as Interaction, context); },
                    "Interaction",
                    "Interactions");

                var rect = new Rect(Event.current.mousePosition, Vector2.zero);
                dropdown.Show(rect);
            }
        }

        protected virtual void DrawRemoveButton()
        {
            var removeImage = QTIEditorResources.General.RemoveIcon;
            GUIContent content = new GUIContent(removeImage,
                "Remove an interaction that runs after this one. If no such interaction exists, removes itself.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                EditorUtils.Interactions.RemoveInteraction(target as Interaction);
            }
        }

        protected virtual void DrawReplaceButton()
        {
#if !UNITY_2022_3_OR_NEWER
            if (PrefabUtility.GetPrefabInstanceStatus(target) != PrefabInstanceStatus.Connected)
            {
#endif
            var replaceImage = QTIEditorResources.General.ReplaceIcon;
            GUIContent content = new GUIContent(replaceImage, "Replaces this interaction with a different one.");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(ButtonHeight)))
            {
                var dropdown = new ComponentDropdown(new AdvancedDropdownState(),
                    typeof(Interaction),
                    (context) => { EditorUtils.Interactions.ReplaceInteraction(target as Interaction, context); },
                    "Interaction",
                    "Interactions");

                var dropdownPosition = new Rect(Event.current.mousePosition, Vector2.zero);

                dropdown.Show(dropdownPosition);
            }
#if !UNITY_2022_3_OR_NEWER
            }
#endif
        }
    }
}