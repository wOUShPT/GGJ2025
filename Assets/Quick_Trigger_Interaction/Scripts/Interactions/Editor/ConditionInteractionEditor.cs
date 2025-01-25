// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(ConditionInteraction), true), CanEditMultipleObjects]
    public class ConditionInteractionEditor : InteractionEditor
    {
        protected override void OnInspectorGUIHeader()
        {
            EditorUtils.Generic.SetHighlighterIdentifier(target);
            ;
            DrawGraphViewButton();
            Helpers.EditorHelpers.DrawDefaultScriptReadonlyObject(target);

            serializedObject.Update();

            SerializedProperty onTrueInteractions = serializedObject.FindProperty("onTrueInteractions");
            SerializedProperty onFalseInteractions = serializedObject.FindProperty("onFalseInteractions");

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.green;
            labelStyle.active.textColor = Color.green;
            labelStyle.focused.textColor = Color.green;
            labelStyle.hover.textColor = Color.green;

            // Draw the property field next to the label
            EditorGUILayout.PropertyField(onTrueInteractions, GUIContent.none, true);

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(""), new GUIStyle(EditorStyles.label));
            rect.y -= EditorGUI.GetPropertyHeight(onTrueInteractions, true);
            rect.size = new Vector2(EditorGUIUtility.labelWidth, rect.size.y);
            EditorGUI.LabelField(rect, new GUIContent("On True"), labelStyle);

            labelStyle.normal.textColor = Color.red;
            labelStyle.active.textColor = Color.red;
            labelStyle.focused.textColor = Color.red;
            labelStyle.hover.textColor = Color.red;

            EditorGUILayout.PropertyField(onFalseInteractions, GUIContent.none);

            rect = GUILayoutUtility.GetRect(new GUIContent(""), new GUIStyle(EditorStyles.label));
            rect.y -= EditorGUI.GetPropertyHeight(onFalseInteractions, true);
            rect.size = new Vector2(EditorGUIUtility.labelWidth, rect.size.y);
            EditorGUI.LabelField(rect, new GUIContent("On False"), labelStyle);

            serializedObject.ApplyModifiedProperties();
        }

        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty useConditionClass = serializedObject.FindProperty("useConditionClass");
            EditorGUILayout.PropertyField(useConditionClass);

            SerializedProperty condition = serializedObject.FindProperty("condition");
            EditorGUILayout.PropertyField(condition);

            SerializedProperty isTrue = serializedObject.FindProperty("isTrue");
            EditorGUILayout.PropertyField(isTrue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}