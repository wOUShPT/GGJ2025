// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(InstantiateInteraction))]
    public class InstantiateInteractionEditor : InteractionEditor
    {
        private InstantiateInteraction _targetScript;

        private SerializedProperty _toInstantiateProp;
        private ReorderableList _toInstantiateList;
        private int _selectedToInstantiateListIndex;

        private void OnEnable()
        {
            _targetScript = target as InstantiateInteraction;
            CreateToInstantiateList();
        }

        public override void DrawProperties()
        {
            serializedObject.Update();
            _toInstantiateList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateToInstantiateList()
        {
            _toInstantiateProp = serializedObject.FindProperty("toInstantiate");
            _toInstantiateList = new ReorderableList(serializedObject, _toInstantiateProp)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                headerHeight = EditorGUIUtility.singleLineHeight + 10,

                drawHeaderCallback = DrawToInstantiateHeader,

                drawElementCallback = DrawToInstantiateElement,

                onAddCallback = (list) =>
                {
                    Undo.RecordObject(target, "New Spawn Options Element");
                    InstantiateInteraction.SpawnOptions newElement = new InstantiateInteraction.SpawnOptions();
                    newElement.Reset();
                    _targetScript.toInstantiate.Add(newElement);
                },

                onRemoveCallback = (list) =>
                {
                    Undo.RecordObject(target, "Removed Spawn Options Element");
                    _targetScript.toInstantiate.RemoveAt(_selectedToInstantiateListIndex);
                },

                elementHeightCallback = (index) =>
                {
                    SerializedProperty element = _toInstantiateProp.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing;
                },
            };
        }

        private void DrawToInstantiateHeader(Rect rect)
        {
            EditorGUI.PrefixLabel(rect, new GUIContent("To Instantiate"), new GUIStyle("BoldLabel"));
        }

        private void DrawToInstantiateElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isActive)
            {
                _selectedToInstantiateListIndex = index;
            }

            rect.y += EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(rect, _toInstantiateProp.GetArrayElementAtIndex(index), true);
        }
    }

    [CustomPropertyDrawer(typeof(InstantiateInteraction.SpawnOptions), true)]
    public class SpawnOptionsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty toSpawnProp = property.FindPropertyRelative("ToSpawn");
            SerializedProperty transformModeProp = property.FindPropertyRelative("transformMode");
            SerializedProperty spawnTransformProp = property.FindPropertyRelative("spawnTransform");
            SerializedProperty spawnPositionProp = property.FindPropertyRelative("spawnPosition");
            SerializedProperty spawnRotationProp = property.FindPropertyRelative("spawnRotation");
            SerializedProperty spawnScaleProp = property.FindPropertyRelative("spawnScale");
            SerializedProperty parentModeProp = property.FindPropertyRelative("parentMode");
            SerializedProperty parentProp = property.FindPropertyRelative("spawnParent");

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var toSpawnRect = new Rect(position.x, position.y, position.size.x - 10f,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(toSpawnRect, toSpawnProp, new GUIContent("Prefab"));
            position.y += EditorGUI.GetPropertyHeight(toSpawnProp);

            position.y += 10;
            var modeRect = new Rect(position.x, position.y, position.size.x - 10f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(modeRect, transformModeProp, new GUIContent("Transform Mode"));
            position.y += EditorGUI.GetPropertyHeight(transformModeProp);

            InstantiateInteraction.InstantiateInteractionTransformMode transformModeEnum =
                (InstantiateInteraction.InstantiateInteractionTransformMode)transformModeProp.enumValueIndex;
            EditorGUI.indentLevel++;
            switch (transformModeEnum)
            {
                case InstantiateInteraction.InstantiateInteractionTransformMode.Original:

                    break;

                case InstantiateInteraction.InstantiateInteractionTransformMode.Transform:

                    position.y += 10;

                    var spawnTransformRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnTransformRect, spawnTransformProp, new GUIContent("Transform"));
                    position.y += EditorGUI.GetPropertyHeight(spawnTransformProp);

                    var spawnPositionRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnPositionRect, spawnPositionProp, new GUIContent("Position Offset"));
                    position.y += EditorGUI.GetPropertyHeight(spawnPositionProp);

                    var spawnRotationRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnRotationRect, spawnRotationProp, new GUIContent("Rotation Offset"));
                    position.y += EditorGUI.GetPropertyHeight(spawnRotationProp);

                    var spawnScaleRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnScaleRect, spawnScaleProp, new GUIContent("Scale Offset"));
                    position.y += EditorGUI.GetPropertyHeight(spawnScaleProp);
                    break;

                case InstantiateInteraction.InstantiateInteractionTransformMode.Manual:

                    position.y += 10;

                    spawnPositionRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnPositionRect, spawnPositionProp, new GUIContent("Position"));
                    position.y += EditorGUI.GetPropertyHeight(spawnPositionProp);

                    spawnRotationRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnRotationRect, spawnRotationProp, new GUIContent("Rotation"));
                    position.y += EditorGUI.GetPropertyHeight(spawnRotationProp);

                    spawnScaleRect = new Rect(position.x, position.y, position.size.x - 10f,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(spawnScaleRect, spawnScaleProp, new GUIContent("Scale"));
                    position.y += EditorGUI.GetPropertyHeight(spawnScaleProp);
                    break;
            }

            EditorGUI.indentLevel--;

            position.y += 10;
            var setParentRect = new Rect(position.x, position.y, position.size.x - 10f,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(setParentRect, parentModeProp, new GUIContent("Parent Mode"));
            position.y += EditorGUI.GetPropertyHeight(parentModeProp);

            if (parentModeProp.boolValue)
            {
                position.y += 10;
                EditorGUI.indentLevel++;
                var parentRect = new Rect(position.x, position.y, position.size.x - 10f,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(parentRect, parentProp, new GUIContent("Transform"));
                position.y += EditorGUI.GetPropertyHeight(parentProp);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            position.y += 2;

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty toSpawnProp = property.FindPropertyRelative("ToSpawn");
            SerializedProperty transformModeProp = property.FindPropertyRelative("transformMode");
            SerializedProperty spawnTransformProp = property.FindPropertyRelative("spawnTransform");
            SerializedProperty spawnPositionProp = property.FindPropertyRelative("spawnPosition");
            SerializedProperty spawnRotationProp = property.FindPropertyRelative("spawnRotation");
            SerializedProperty spawnScaleProp = property.FindPropertyRelative("spawnScale");
            SerializedProperty parentModeProp = property.FindPropertyRelative("parentMode");
            SerializedProperty parentProp = property.FindPropertyRelative("spawnParent");

            float height = 0;
            height += EditorGUI.GetPropertyHeight(toSpawnProp);
            height += 10;
            height += EditorGUI.GetPropertyHeight(transformModeProp);
            InstantiateInteraction.InstantiateInteractionTransformMode transformModeEnum =
                (InstantiateInteraction.InstantiateInteractionTransformMode)transformModeProp.enumValueIndex;
            switch (transformModeEnum)
            {
                case InstantiateInteraction.InstantiateInteractionTransformMode.Original:

                    break;

                case InstantiateInteraction.InstantiateInteractionTransformMode.Transform:

                    height += 10;
                    height += EditorGUI.GetPropertyHeight(spawnTransformProp);
                    height += EditorGUI.GetPropertyHeight(spawnPositionProp);
                    height += EditorGUI.GetPropertyHeight(spawnRotationProp);
                    height += EditorGUI.GetPropertyHeight(spawnScaleProp);
                    break;

                case InstantiateInteraction.InstantiateInteractionTransformMode.Manual:

                    height += 10;
                    height += EditorGUI.GetPropertyHeight(spawnPositionProp);
                    height += EditorGUI.GetPropertyHeight(spawnRotationProp);
                    height += EditorGUI.GetPropertyHeight(spawnScaleProp);
                    break;
            }

            height += 10;
            height += EditorGUI.GetPropertyHeight(parentModeProp);
            if (parentModeProp.boolValue)
            {
                height += 10;
                height += EditorGUI.GetPropertyHeight(parentProp);
            }

            height += 2;

            return height;
        }
    }
}