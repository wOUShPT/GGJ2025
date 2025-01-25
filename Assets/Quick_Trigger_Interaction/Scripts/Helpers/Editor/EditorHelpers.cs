// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AstralShift.QTI.Helpers
{
    public static class EditorHelpers
    {
        public static void GUILineSeparator(int height = 1)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            rect.position = new Vector2(rect.position.x + EditorGUI.indentLevel * 15f, rect.position.y);
            rect.size = new Vector2(rect.size.x - EditorGUI.indentLevel * 15f, rect.size.y);
            if (EditorGUIUtility.isProSkin)
            {
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1));
            }

            EditorGUILayout.Space();
        }

        public static void DrawMinMaxSlider(string label, ref MaterialProperty minProperty,
            ref MaterialProperty maxProperty, float minValue, float maxValue, int roundPlaces)
        {
            // Calculate Properties Width
            float floatFieldWidth = EditorStyles.label.CalcSize(new GUIContent(maxValue + ".")).x;
            for (int i = 0; i < roundPlaces; i++)
            {
                floatFieldWidth += EditorStyles.label.CalcSize(new GUIContent("#")).x;
            }

            float sliderWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - floatFieldWidth * 3;

            // Get Properties Values
            float currentMinValue = minProperty.floatValue;
            currentMinValue = (float)System.Math.Round(currentMinValue, roundPlaces);
            float currentMaxValue = maxProperty.floatValue;
            currentMaxValue = (float)System.Math.Round(currentMaxValue, roundPlaces);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentMinValue = EditorGUILayout.FloatField("", currentMinValue, GUILayout.Width(floatFieldWidth));
            EditorGUILayout.MinMaxSlider("", ref currentMinValue, ref currentMaxValue, minValue, maxValue,
                GUILayout.Width(sliderWidth));
            currentMaxValue = EditorGUILayout.FloatField("", currentMaxValue, GUILayout.Width(floatFieldWidth));

            // Clamp Slider/Float Field Values to Bounds
            currentMinValue = Mathf.Clamp(currentMinValue, minValue, maxValue);
            currentMaxValue = Mathf.Clamp(currentMaxValue, minValue, maxValue);
            EditorGUILayout.EndHorizontal();

            // Update Values if Changed
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            minProperty.floatValue = currentMinValue;
            maxProperty.floatValue = currentMaxValue;
        }

        public static void DrawWireCapsule(Vector3 _pos, Vector3 _pos2, float _radius, float _height,
            Color _color = default)
        {
            if (_color != default) Handles.color = _color;

            var forward = _pos2 - _pos;
            var _rot = Quaternion.LookRotation(forward);
            var pointOffset = _radius / 2f;
            var length = forward.magnitude;
            var center2 = new Vector3(0f, 0, length);

            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);

            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
                Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.left * pointOffset, -180f, _radius);
                Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.down * pointOffset, -180f, _radius);
                Handles.DrawWireDisc(center2, Vector3.forward, _radius);
                Handles.DrawWireArc(center2, Vector3.up, Vector3.right * pointOffset, -180f, _radius);
                Handles.DrawWireArc(center2, Vector3.left, Vector3.up * pointOffset, -180f, _radius);

                DrawLine(_radius, 0f, length);
                DrawLine(-_radius, 0f, length);
                DrawLine(0f, _radius, length);
                DrawLine(0f, -_radius, length);
            }
        }

        private static void DrawLine(float arg1, float arg2, float forward)
        {
            Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward));
        }

        public static void DrawDefaultScriptReadonlyObject(UnityEngine.Object target)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour),
                target.GetType(), false);
            GUI.enabled = true;
        }

        public static string[] GetVariablesNotInherited(Type classType)
        {
            List<string> variables = new List<string>();
            BindingFlags bindingFlags = BindingFlags.DeclaredOnly | // This flag excludes inherited variables.
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.Instance |
                                        BindingFlags.Static;
            foreach (FieldInfo field in classType.GetFields(bindingFlags))
            {
                variables.Add(field.Name);
            }

            return variables.ToArray();
        }

        public static void CopyAllInCommonProperties(Component oldInteraction, Component newInteraction)
        {
            if (oldInteraction == null)
            {
                return;
            }
            
            SerializedObject oldSerializedObject = new SerializedObject(oldInteraction);
            SerializedObject newSerializedObject = new SerializedObject(newInteraction);

            SerializedProperty copiedProperty = oldSerializedObject.GetIterator().Copy();
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

                if (newSerializedObject.FindProperty(copiedProperty.propertyPath) == null)
                {
                    continue;
                }

                newSerializedObject.CopyFromSerializedProperty(copiedProperty);
                newSerializedObject.ApplyModifiedProperties();
            } while (copiedProperty.NextVisible(visitChild));
        }
    }
}