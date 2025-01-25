// Copyright (c) AstralShift. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Triggers
{
    [CustomEditor(typeof(KeyTrigger)), CanEditMultipleObjects]
    public class KeyTriggerEditor : InteractionTriggerEditor
    {
        public override void DrawProperties()
        {
            serializedObject.Update();

            // Fetch the current target object
            KeyTrigger script = (KeyTrigger)target;
            SerializedProperty interactionProp = serializedObject.FindProperty("interaction");
            EditorGUILayout.PropertyField(interactionProp);

            SerializedProperty inputType = serializedObject.FindProperty("inputType");
            EditorGUILayout.PropertyField(inputType);
            SerializedProperty pressType = serializedObject.FindProperty("pressType");
            EditorGUILayout.PropertyField(pressType);

            if (script.inputType == KeyTrigger.InputType.Key)
            {
                SerializedProperty keyCode = serializedObject.FindProperty("keyCode");
                EditorGUILayout.PropertyField(keyCode);
            }
            else
            {
                string[] axNames = GetAllAxes();

                int currentIndex = System.Array.IndexOf(axNames, script.inputAxes);

                int selectedIndex = EditorGUILayout.Popup("Axes ", currentIndex, axNames);
                selectedIndex = Mathf.Clamp(selectedIndex, 0, axNames.Length - 1);

                script.inputAxes = axNames[selectedIndex];
            }

            EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();
        }


        public static string[] GetAllAxes()
        {
            SerializedObject inputManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");
            string[] axisNames = new string[axesProperty.arraySize];

            for (int i = 0; i < axesProperty.arraySize; ++i)
            {
                SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
                string axisName = axis.FindPropertyRelative("m_Name").stringValue;
                axisNames[i] = axisName;
            }

            return axisNames;
        }
    }
}