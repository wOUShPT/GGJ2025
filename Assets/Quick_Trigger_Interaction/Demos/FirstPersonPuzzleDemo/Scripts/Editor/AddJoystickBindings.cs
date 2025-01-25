// Copyright (c) AstralShift. All rights reserved.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos
{
    [InitializeOnLoad]
    public static class AddJoystickBindings
    {
        static AddJoystickBindings()
        {
            Object target = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            if (target == null)
            {
                return;
            }

            SerializedObject inputManager = new SerializedObject(target);
            SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");
            string[] axisNames = new string[axesProperty.arraySize];

            for (int i = 0; i < axesProperty.arraySize; ++i)
            {
                SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
                string axisName = axis.FindPropertyRelative("m_Name").stringValue;
                axisNames[i] = axisName;
            }

            if (!axisNames.Contains("RightHorizontal"))
            {
                axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
                SerializedProperty newAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
                SerializedProperty newAxisName = newAxis.FindPropertyRelative("m_Name");
                newAxisName.stringValue = "RightHorizontal";
                SerializedProperty newAxisDescriptiveName = newAxis.FindPropertyRelative("descriptiveName");
                newAxisDescriptiveName.stringValue = "";
                SerializedProperty newAxisDescriptiveNegativeName =
                    newAxis.FindPropertyRelative("descriptiveNegativeName");
                newAxisDescriptiveNegativeName.stringValue = "";
                SerializedProperty newAxisNegativeButton = newAxis.FindPropertyRelative("negativeButton");
                newAxisNegativeButton.stringValue = "";
                SerializedProperty newAxisPositiveButton = newAxis.FindPropertyRelative("positiveButton");
                newAxisPositiveButton.stringValue = "";
                SerializedProperty newAxisAltNegativeButton = newAxis.FindPropertyRelative("altNegativeButton");
                newAxisAltNegativeButton.stringValue = "";
                SerializedProperty newAxisAltPositiveButton = newAxis.FindPropertyRelative("altPositiveButton");
                newAxisAltPositiveButton.stringValue = "";
                SerializedProperty newAxisGravity = newAxis.FindPropertyRelative("gravity");
                newAxisGravity.floatValue = 0;
                SerializedProperty newAxisDead = newAxis.FindPropertyRelative("dead");
                newAxisDead.floatValue = 0.19f;
                SerializedProperty newAxisSensitivity = newAxis.FindPropertyRelative("sensitivity");
                newAxisSensitivity.floatValue = 1;
                SerializedProperty newAxisSnap = newAxis.FindPropertyRelative("snap");
                newAxisSnap.boolValue = false;
                SerializedProperty newAxisInvert = newAxis.FindPropertyRelative("invert");
                newAxisInvert.boolValue = false;
                SerializedProperty newAxisType = newAxis.FindPropertyRelative("type");
                newAxisType.enumValueIndex = 2;
                SerializedProperty newAxisAxis = newAxis.FindPropertyRelative("axis");
                newAxisAxis.enumValueIndex = 3;
                SerializedProperty newAxisNumber = newAxis.FindPropertyRelative("joyNum");
                newAxisNumber.enumValueIndex = 0;
            }

            if (!axisNames.Contains("RightVertical"))
            {
                axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
                SerializedProperty newAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
                SerializedProperty newAxisName = newAxis.FindPropertyRelative("m_Name");
                newAxisName.stringValue = "RightVertical";
                SerializedProperty newAxisDescriptiveName = newAxis.FindPropertyRelative("descriptiveName");
                newAxisDescriptiveName.stringValue = "";
                SerializedProperty newAxisDescriptiveNegativeName =
                    newAxis.FindPropertyRelative("descriptiveNegativeName");
                newAxisDescriptiveNegativeName.stringValue = "";
                SerializedProperty newAxisNegativeButton = newAxis.FindPropertyRelative("negativeButton");
                newAxisNegativeButton.stringValue = "";
                SerializedProperty newAxisPositiveButton = newAxis.FindPropertyRelative("positiveButton");
                newAxisPositiveButton.stringValue = "";
                SerializedProperty newAxisAltNegativeButton = newAxis.FindPropertyRelative("altNegativeButton");
                newAxisAltNegativeButton.stringValue = "";
                SerializedProperty newAxisAltPositiveButton = newAxis.FindPropertyRelative("altPositiveButton");
                newAxisAltPositiveButton.stringValue = "";
                SerializedProperty newAxisGravity = newAxis.FindPropertyRelative("gravity");
                newAxisGravity.floatValue = 0;
                SerializedProperty newAxisDead = newAxis.FindPropertyRelative("dead");
                newAxisDead.floatValue = 0.19f;
                SerializedProperty newAxisSensitivity = newAxis.FindPropertyRelative("sensitivity");
                newAxisSensitivity.floatValue = 1;
                SerializedProperty newAxisSnap = newAxis.FindPropertyRelative("snap");
                newAxisSnap.boolValue = false;
                SerializedProperty newAxisInvert = newAxis.FindPropertyRelative("invert");
                newAxisInvert.boolValue = true;
                SerializedProperty newAxisType = newAxis.FindPropertyRelative("type");
                newAxisType.enumValueIndex = 2;
                SerializedProperty newAxisAxis = newAxis.FindPropertyRelative("axis");
                newAxisAxis.enumValueIndex = 4;
                SerializedProperty newAxisNumber = newAxis.FindPropertyRelative("joyNum");
                newAxisNumber.enumValueIndex = 0;
            }

            inputManager.ApplyModifiedProperties();
        }
    }
}