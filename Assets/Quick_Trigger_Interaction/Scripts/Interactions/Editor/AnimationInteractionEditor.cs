// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerParameter = UnityEngine.AnimatorControllerParameter;
using AnimatorControllerParameterType = UnityEngine.AnimatorControllerParameterType;

namespace AstralShift.QTI.Interactions
{
    [CustomEditor(typeof(AnimationInteraction))]
    public class AnimationInteractionEditor : InteractionEditor
    {
        private AnimationInteraction _targetScript;

        private string[] _layersNames;
        private string[] _statesNames;
        private int _selectedParameterIndex;
        private ReorderableList _currentParameterList;

        public override VisualElement CreateInspectorGUI()
        {
            _targetScript = target as AnimationInteraction;

            if (_targetScript.Animator == null)
            {
                return base.CreateInspectorGUI();
            }

            if (_targetScript.Animator.runtimeAnimatorController == null)
            {
                return base.CreateInspectorGUI();
            }

            GetAnimatorStatesIfChanged();
            _layersNames = GetLayersNames();
            _statesNames = GetCurrentLayerStatesNames();
            GetAnimatorParametersIfChanged();
            CreateParametersDrawer();
            return base.CreateInspectorGUI();
        }

        public override void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty animatorProp = serializedObject.FindProperty("animator");
            EditorGUILayout.PropertyField(animatorProp);

            serializedObject.ApplyModifiedProperties();

            if (_targetScript.Animator == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("No Animator assigned!"));
                return;
            }

            if (_targetScript.Animator.runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("No AnimatorController assigned to the Animator!"));
                return;
            }

            SerializedProperty modeProp = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(modeProp);

            // Draw Play Animation Mode
            if ((AnimationInteraction.AnimationInteractionMode)modeProp.enumValueIndex ==
                AnimationInteraction.AnimationInteractionMode.Play)
            {
                if (!Application.isPlaying)
                {
                    GetAnimatorStatesIfChanged();
                }

                if (_targetScript.layers.Length == 0)
                {
                    EditorGUILayout.HelpBox(new GUIContent("This Animator Controller doesn't have layers!"));
                    return;
                }

                DrawAnimationSelector();
            }
            else // Draw Set Parameter Mode
            {
                if (!Application.isPlaying)
                {
                    GetAnimatorParametersIfChanged();
                }

                if (_targetScript.parameters != null && _targetScript.parameters.Count > 0)
                {
                    if (_currentParameterList == null)
                    {
                        CreateParametersDrawer();
                    }

                    _currentParameterList.DoLayoutList();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw the animation selection dropdown
        /// </summary>
        private void DrawAnimationSelector()
        {
            if (_targetScript == null)
            {
                return;
            }

            if (_targetScript.layers.Length == 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Animation");
                EditorGUI.BeginChangeCheck();

                int index = EditorGUILayout.Popup(_targetScript.stateIndex, _statesNames);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target,
                        "Changed Animation Dropdown (" + _targetScript.name + "/" + _targetScript.gameObject.name +
                        ")");
                    _targetScript.stateIndex = index;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Layer");
                EditorGUI.BeginChangeCheck();
                int index = EditorGUILayout.Popup(_targetScript.layerIndex, _layersNames);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target,
                        "Changed Layer Dropdown (" + _targetScript.name + "/" + _targetScript.gameObject.name + ")");
                    _targetScript.layerIndex = index;
                    _targetScript.stateIndex = 0;
                    GetCurrentLayerStatesNames();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Animation");
                EditorGUI.BeginChangeCheck();
                index = EditorGUILayout.Popup(_targetScript.stateIndex,
                    _targetScript.layers[_targetScript.layerIndex].states.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target,
                        "Changed Animation Dropdown (" + _targetScript.name + "/" + _targetScript.gameObject.name +
                        ")");
                    _targetScript.stateIndex = index;
                }

                EditorGUILayout.EndHorizontal();
            }

            SerializedProperty waitForAnimationProp = serializedObject.FindProperty("waitForAnimationEnd");
            EditorGUILayout.PropertyField(waitForAnimationProp);
        }

        /// <summary>
        /// Refresh all layers if the animator controller changes
        /// </summary>
        private void GetAnimatorStatesIfChanged()
        {
            _targetScript.animatorController = _targetScript.Animator.runtimeAnimatorController;
            string assetPath = AssetDatabase.GetAssetPath(_targetScript.animatorController);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("Error: No AnimatorController found!"));
                return;
            }

            // Check if Animator Controller states changed, if true refresh states list
            List<string> allStates = AstralShift.QTI.Helpers.Animation.GetAnimatorStates(controller);
            if (AstralShift.QTI.Helpers.Collections.HaveSameElements(allStates.ToArray(), _targetScript.states))
            {
                return;
            }

            _targetScript.states = allStates.ToArray();

            List<string> layerNames = AstralShift.QTI.Helpers.Animation.GetAnimatorLayers(controller);
            _targetScript.layers = new AnimationInteraction.AnimatorLayer[layerNames.Count];

            for (int i = 0; i < _targetScript.layers.Length; i++)
            {
                List<string> states = AstralShift.QTI.Helpers.Animation.GetAnimatorStates(controller, i);
                AnimationInteraction.AnimatorLayer animatorLayer =
                    new AnimationInteraction.AnimatorLayer(layerNames[i], states);
                _targetScript.layers[i] = animatorLayer;
            }

            _targetScript.layerIndex = 0;
            _targetScript.stateIndex = 0;

            _layersNames = GetLayersNames();
            _statesNames = GetCurrentLayerStatesNames();
        }

        /// <summary>
        /// Returns an array of all layers names available in the animator controller
        /// </summary>
        /// <returns></returns>
        private string[] GetLayersNames()
        {
            int count = _targetScript.layers.Length;
            string[] layerNames = new string[count];
            for (int i = 0; i < layerNames.Length; i++)
            {
                layerNames[i] = _targetScript.layers[i].name;
            }

            return layerNames;
        }

        /// <summary>
        /// Returns an array of all states names in the current layer
        /// </summary>
        /// <returns></returns>
        private string[] GetCurrentLayerStatesNames()
        {
            int count = _targetScript.layers[_targetScript.layerIndex].states.Count;
            string[] statesNames = new string[count];
            for (int i = 0; i < statesNames.Length; i++)
            {
                statesNames[i] = _targetScript.layers[_targetScript.layerIndex].states[i];
            }

            return statesNames;
        }

        /// <summary>
        /// Refresh the parameters list if they change
        /// </summary>
        private void GetAnimatorParametersIfChanged()
        {
            if (_targetScript.animatorController != _targetScript.Animator.runtimeAnimatorController)
            {
                _targetScript.animatorController = _targetScript.Animator.runtimeAnimatorController;
                _targetScript.parameters = null;
            }

            string assetPath = AssetDatabase.GetAssetPath(_targetScript.animatorController);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller == null)
            {
                return;
            }

            if (_targetScript.parameters == null)
            {
                if (controller.parameters != null)
                {
                    SetAvailableParametersList(controller.parameters);
                }
                else
                {
                    _targetScript.parameters = new List<AnimationInteraction.AnimatorParameter>();
                }

                _targetScript.currentParameters = new List<AnimationInteraction.AnimatorParameter>();
                CreateParametersDrawer();
            }

            if (controller.parameters != null && controller.parameters.Length > 0)
            {
                List<int> storedParamsHashes = new List<int>();
                foreach (var parameter in _targetScript.parameters)
                {
                    storedParamsHashes.Add(parameter.hash);
                }

                List<int> animatorParamsHashes = new List<int>();
                foreach (var parameter in controller.parameters)
                {
                    animatorParamsHashes.Add(parameter.nameHash);
                }

                bool areEqual = AstralShift.QTI.Helpers.Collections.HaveSameElements(animatorParamsHashes.ToArray(),
                    storedParamsHashes.ToArray());
                if (!areEqual)
                {
                    SetAvailableParametersList(controller.parameters);
                    RemoveParametersIfInvalid();
                }
            }
        }

        /// <summary>
        /// Set an array of the parameters as the current available parameters
        /// </summary>
        /// <param name="parameters"></param>
        private void SetAvailableParametersList(AnimatorControllerParameter[] parameters)
        {
            _targetScript.parameters.Clear();
            foreach (var parameter in parameters)
            {
                AnimationInteraction.AnimatorParameter newParameter =
                    new AnimationInteraction.AnimatorParameter(parameter);
                _targetScript.parameters.Add(newParameter);
            }
        }

        /// <summary>
        /// Add a new parameter to the user list
        /// </summary>
        private void AddParameter()
        {
            Undo.RecordObject(target, "Parameter Added");
            AnimationInteraction.AnimatorParameter newParameter =
                new AnimationInteraction.AnimatorParameter(_targetScript.parameters[0]);
            _targetScript.currentParameters.Add(newParameter);
        }

        /// <summary>
        /// Remove the last parameter of the user list
        /// </summary>
        private void RemoveParameter()
        {
            Undo.RecordObject(target, "Parameter Removed");
            int index = _targetScript.currentParameters.Count - 1;
            _targetScript.currentParameters.RemoveAt(index);
        }

        /// <summary>
        /// Remove a parameter of a given index from the user list
        /// </summary>
        private void RemoveParameter(int index)
        {
            Undo.RecordObject(target, "Parameter Removed");
            _targetScript.currentParameters.RemoveAt(index);
        }

        /// <summary>
        /// Remove all parameters which aren't valid from the user list
        /// </summary>
        private void RemoveParametersIfInvalid()
        {
            _targetScript.currentParameters.RemoveAll(item =>
                _targetScript.parameters.All(element => element.name != item.name));
        }

        /// <summary>
        /// Create the ReordableList which displays the user list
        /// </summary>
        private void CreateParametersDrawer()
        {
            SerializedProperty currentParametersProp = serializedObject.FindProperty("currentParameters");
            _currentParameterList = new ReorderableList(serializedObject, currentParametersProp)
            {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                headerHeight = 0,

                drawElementCallback = DrawAnimatorParameter,

                onAddCallback = (list) => { AddParameter(); },

                onRemoveCallback = (list) => { RemoveParameter(_selectedParameterIndex); }
            };
        }

        /// <summary>
        /// Draws the Animator Parameter within the parameters ReordableList
        /// </summary>
        private void DrawAnimatorParameter(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_targetScript.parameters.Count == 0 || _targetScript.currentParameters.Count == 0)
            {
                return;
            }

            if (isActive)
            {
                _selectedParameterIndex = index;
            }

            Rect position = rect;

            EditorGUILayout.BeginHorizontal();
            string[] names = GetCurrentParametersNames();
            int selectedParam = 0;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == _targetScript.currentParameters[index].name)
                {
                    selectedParam = i;
                }
            }

            EditorGUI.BeginChangeCheck();
            Rect popupRect = position;
            popupRect.size = new Vector2(popupRect.size.x / 2, popupRect.size.y);
            popupRect.y += 4;
            selectedParam = EditorGUI.Popup(popupRect, selectedParam, names);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Parameter Type");
                _targetScript.currentParameters[index].SetParameter(_targetScript.parameters[selectedParam]);
            }

            AnimationInteraction.AnimatorParameter parameter = _targetScript.currentParameters[index];
            EditorGUI.BeginChangeCheck();
            object value = null;
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Int:

                    rect.size = new Vector2(48, 18);
                    rect.x += position.size.x - rect.size.x;
                    value = EditorGUI.IntField(rect, (int)parameter.Value);
                    break;

                case AnimatorControllerParameterType.Float:

                    rect.size = new Vector2(48, 18);
                    rect.x += position.size.x - rect.size.x;
                    value = EditorGUI.FloatField(rect, (float)parameter.Value);
                    break;

                case AnimatorControllerParameterType.Bool:

                    rect.size = new Vector2(16, rect.size.y);
                    rect.x += position.size.x - rect.size.x;
                    value = EditorGUI.Toggle(rect, (bool)parameter.Value);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Parameter Value");
                _targetScript.currentParameters[index].Value = value;
            }

            EditorGUILayout.EndHorizontal();
        }

        private string[] GetCurrentParametersNames()
        {
            if (_targetScript.parameters == null)
            {
                return null;
            }

            List<string> names = new List<string>();
            foreach (var parameter in _targetScript.parameters)
            {
                names.Add(parameter.name);
            }

            return names.ToArray();
        }
    }
}