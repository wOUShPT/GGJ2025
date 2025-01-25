// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    /// <summary>
    /// An Interaction that plays a given animation with the sourced animator.
    /// ... allows for an onEnd action to happen even midway through by coroutine.
    /// </summary>
    [AddComponentMenu("QTI/Interactions/AnimationInteraction")]
    public class AnimationInteraction : Interaction
    {
        [SerializeField] private Animator animator;
        public Animator Animator => animator;

        public AnimationInteractionMode mode;

        [HideInInspector] public RuntimeAnimatorController animatorController;

        [HideInInspector] public AnimatorLayer[] layers;

        [HideInInspector] public string[] states;

        [HideInInspector] public int layerIndex;

        [HideInInspector] public int stateIndex;

        [HideInInspector] public List<AnimatorParameter> parameters;

        [HideInInspector] public List<AnimatorParameter> currentParameters;

        private int _currentAnimationHash;
        private int _nextAnimationHash;

        [Tooltip("Only applicable if there's on end actions.")]
        public bool waitForAnimationEnd;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            if (animator != null)
            {
                if (layers == null)
                {
                    Debug.LogWarning("Animation Interaction: No layers found!", this);
                    return;
                }

                if (mode == AnimationInteractionMode.Play)
                {
                    animator.Play(layers[layerIndex].hashes[stateIndex], layerIndex);
                }
                else
                {
                    SetParameters();
                }
            }
            else
            {
                Debug.LogWarning("Animation Interaction: No animator assigned!", this);
            }

            if (mode == AnimationInteractionMode.Play && waitForAnimationEnd)
            {
                StartCoroutine(WaitForAnimationEnd());
                return;
            }

            OnEnd();
        }

        /// <summary>
        /// Send all current parameters to the assigned animator
        /// </summary>
        private void SetParameters()
        {
            if (currentParameters.Count == 0)
            {
                return;
            }

            foreach (var parameter in currentParameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Int:

                        animator.SetInteger(parameter.hash, parameter.intValue);
                        break;

                    case AnimatorControllerParameterType.Float:

                        animator.SetFloat(parameter.hash, parameter.floatValue);
                        break;

                    case AnimatorControllerParameterType.Bool:

                        animator.SetBool(parameter.hash, parameter.boolValue);
                        break;

                    case AnimatorControllerParameterType.Trigger:

                        animator.SetTrigger(parameter.hash);
                        break;
                }
            }
        }

        private IEnumerator WaitForAnimationEnd()
        {
            int targetAnimationHash = Animator.StringToHash(layers[layerIndex].states[stateIndex]);
            yield return new WaitUntil(() =>
                targetAnimationHash == animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash);
            float waitTime = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
            yield return new WaitForSeconds(waitTime);
            OnEnd();
        }

        /// <summary>
        /// This class stores info about an Animator Layer
        /// </summary>
        [Serializable]
        public class AnimatorLayer
        {
            public string name;
            public List<string> states;
            public int[] hashes;

            public AnimatorLayer(string name, List<string> states)
            {
                this.name = name;
                this.states = states;
                hashes = new int[this.states.Count];
                for (int i = 0; i < hashes.Length; i++)
                {
                    hashes[i] = Animator.StringToHash(this.states[i]);
                }
            }

            public AnimatorLayer()
            {
                name = "";
                states = new List<string>();
                hashes = null;
            }
        }

        public enum AnimationInteractionMode
        {
            Play,
            Parameters
        }

        /// <summary>
        /// This class represents an Unity's animator parameter.
        /// We need to do this because UnityEngine.AnimatorControllerParameter is not serializable.
        /// The type enum defines the Value type (int, float or bool) a parameter stores.
        /// </summary>
        [Serializable]
        public class AnimatorParameter
        {
            public string name;
            public int hash;
            public AnimatorControllerParameterType type;
            public int intValue;
            public float floatValue;
            public bool boolValue;

            public AnimatorParameter(AnimatorControllerParameter parameter, object value)
            {
                name = parameter.name;
                hash = Animator.StringToHash(name);
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = (int)value;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = (float)value;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = (bool)value;
                        break;
                }
            }

            public AnimatorParameter(AnimatorControllerParameter parameter)
            {
                name = parameter.name;
                hash = Animator.StringToHash(name);
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = 0;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = 0;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = false;
                        break;
                }
            }

            public AnimatorParameter(AnimatorParameter parameter)
            {
                name = parameter.name;
                hash = parameter.hash;
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = parameter.intValue;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = parameter.floatValue;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = parameter.boolValue;
                        break;
                }
            }

            public object Value
            {
                get
                {
                    switch (type)
                    {
                        case AnimatorControllerParameterType.Int:

                            return intValue;

                        case AnimatorControllerParameterType.Float:

                            return floatValue;

                        case AnimatorControllerParameterType.Bool:

                            return boolValue;
                    }

                    return null;
                }
                set
                {
                    switch (type)
                    {
                        case AnimatorControllerParameterType.Int:

                            intValue = (int)value;
                            break;

                        case AnimatorControllerParameterType.Float:

                            floatValue = (float)value;
                            break;

                        case AnimatorControllerParameterType.Bool:

                            boolValue = (bool)value;
                            break;
                    }
                }
            }

            public void SetParameter(AnimatorControllerParameter parameter, object value)
            {
                name = parameter.name;
                hash = Animator.StringToHash(name);
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = (int)value;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = (float)value;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = (bool)value;
                        break;
                }
            }

            public void SetParameter(AnimatorControllerParameter parameter)
            {
                name = parameter.name;
                hash = Animator.StringToHash(name);
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = 0;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = 0;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = false;
                        break;
                }
            }

            public void SetParameter(AnimatorParameter parameter)
            {
                name = parameter.name;
                hash = parameter.hash;
                type = parameter.type;
                switch (type)
                {
                    case AnimatorControllerParameterType.Int:

                        intValue = parameter.intValue;
                        break;

                    case AnimatorControllerParameterType.Float:

                        floatValue = parameter.floatValue;
                        break;

                    case AnimatorControllerParameterType.Bool:

                        boolValue = parameter.boolValue;
                        break;
                }
            }
        }
    }
}