// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using AstralShift.QTI.Interactors;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Threading;
using System.Threading.Tasks;
using AstralShift.QTI.Helpers;

#endif

namespace AstralShift.QTI.Triggers
{
    /// <summary>
    /// Only works if there's only one object with given tag
    /// </summary>
    public class OnDistanceTrigger : InteractionTrigger
    {
        public float distance = 1;
        public string selectedTag = "Untagged"; // This will hold the selected tag from the dropdown

        protected Dictionary<IInteractor, bool> _targets;

        public enum Mode
        {
            OnEnter,
            OnExit,
            OnStay
        }

        public Mode mode;

        protected virtual float CalculateDistance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        protected override void Awake()
        {
            base.Awake();
            _targets ??= new Dictionary<IInteractor, bool>();
        }

        private void Update()
        {
            var gameObjectsWithTag = GameObject.FindGameObjectsWithTag(selectedTag);

            if (gameObjectsWithTag.Length == 0)
            {
                return;
            }

            List<IInteractor> validTargets = new List<IInteractor>();
            foreach (var element in gameObjectsWithTag)
            {
                if (element.TryGetComponent(out IInteractor interactor))
                {
                    _targets.TryAdd(interactor, false);
                    validTargets.Add(interactor);
                }
            }

            // Get a list of invalid dictionary keys and remove them
            List<IInteractor> invalidKeys = _targets.Keys.Except(validTargets).ToList();
            for (int i = 0; i < invalidKeys.Count; i++)
            {
                _targets.Remove(invalidKeys[i]);
            }

            if (_targets.Count == 0)
            {
                return;
            }

            // Get a list of the keys to iterate over
            List<IInteractor> keys = new List<IInteractor>(_targets.Keys);
            foreach (var key in keys)
            {
                IInteractor interactor = key;
                bool inRange = _targets[key];
                switch (mode)
                {
                    case Mode.OnEnter:

                        if (CalculateDistance(transform.position, interactor.GetTransform().position) > distance)
                        {
                            inRange = false;
                        }

                        if (!inRange)
                        {
                            if (CalculateDistance(transform.position, interactor.GetTransform().position) <= distance)
                            {
                                base.Interact(interactor);
                                inRange = true;

#if UNITY_EDITOR
                                
#pragma warning disable 4014
                                TriggerInRangeColor();
#pragma warning restore 4014

#endif
                            }
                            else
                            {
                                inRange = false;
                            }
                        }

                        break;

                    case Mode.OnExit:

                        if (inRange)
                        {
                            if (CalculateDistance(transform.position, interactor.GetTransform().position) > distance)
                            {
                                base.Interact(interactor);
                                inRange = false;

#if UNITY_EDITOR

#pragma warning disable 4014
                                TriggerInRangeColor();
#pragma warning restore 4014

#endif
                            }
                            else
                            {
                                inRange = true;
                            }
                        }
                        else if (CalculateDistance(transform.position, interactor.GetTransform().position) <= distance)
                        {
                            inRange = true;
                        }

                        break;

                    case Mode.OnStay:

                        if (CalculateDistance(transform.position, interactor.GetTransform().position) <= distance)
                        {
                            base.Interact(interactor);

#if UNITY_EDITOR

                            SetInRangeColor();

#endif

                            return;
                        }


#if UNITY_EDITOR

                        ResetInRangeColor();

#endif

                        break;
                }

                _targets[interactor] = inRange;
            }
        }

#if UNITY_EDITOR

        protected const int ReferenceFontSize = 14;

        protected virtual void OnDrawGizmosSelected()
        {
            Vector3 interactionPosition = transform.position;
            using (new Handles.DrawingScope(_currentStateColor))
            {
                Handles.DrawWireDisc(interactionPosition, Vector3.forward, distance, GizmoThickness);
                Handles.DrawWireDisc(interactionPosition, Vector3.right, distance, GizmoThickness);
                Handles.DrawWireDisc(interactionPosition, Vector3.up, distance, GizmoThickness);
            }

            if (_targets == null)
            {
                return;
            }

            foreach (var key in _targets.Keys)
            {
                IInteractor interactor = key;
                Vector3 targetPosition = interactor.GetTransform().position;
                Vector3 direction = targetPosition - interactionPosition;

                GizmosHelpers.DrawArrow(interactionPosition, direction, Vector3.up,
                    direction.magnitude <= this.distance ? _inRangeStateColor : _defaultStateColor,
                    GizmoThickness);

                Vector3 labelPosition = interactionPosition + direction / 2;
                string labelText = direction.magnitude.ToString("F2") + " units";

                if (Camera.current == Camera.main)
                {
                    continue;
                }

                GizmosHelpers.DrawTextBox(labelText, ReferenceFontSize, FontStyle.Normal, labelPosition, Color.white,
                    new Color(0, 0, 0, 0.4f));
            }
        }

        protected virtual void OnGUI()
        {
            if (!Selection.gameObjects.Contains(gameObject) || !Handles.ShouldRenderGizmos())
            {
                return;
            }

            if (_targets == null)
            {
                return;
            }

            Vector3 interactionPosition = transform.position;

            foreach (var key in _targets.Keys)
            {
                IInteractor interactor = key;
                Vector3 targetPosition = interactor.GetTransform().position;
                Vector3 direction = targetPosition - interactionPosition;

                Vector3 labelPosition = interactionPosition + direction / 2;
                string labelText = direction.magnitude.ToString("F2") + " units";

                float resolutionScale = GizmosHelpers.GetResolutionMultiplier();

                // Scale the font size
                int fontSize = Mathf.RoundToInt(ReferenceFontSize * resolutionScale);

                GizmosHelpers.DrawTextBoxGUI(labelText, fontSize, FontStyle.Normal, labelPosition, Color.white,
                    new Color(0, 0, 0, 0.4f));
            }
        }

        protected readonly Color _defaultStateColor = new Color(1, 0, 0, 0.5f);
        protected readonly Color _inRangeStateColor = new Color(0, 1, 0, 0.5f);
        protected Color _currentStateColor = new Color(1, 0, 0, 0.5f);
        protected CancellationTokenSource _inRangeColorCTS;
        protected const float InRangeColorDuration = 0.5f;
        protected const float GizmoThickness = 2;

        protected void SetInRangeColor()
        {
            _currentStateColor = _inRangeStateColor;
        }

        protected void ResetInRangeColor()
        {
            _currentStateColor = _defaultStateColor;
        }

        protected async Task TriggerInRangeColor()
        {
            // Cancel any previous trigger color operation
            _inRangeColorCTS?.Cancel();

            // Create a new CancellationTokenSource
            _inRangeColorCTS = new CancellationTokenSource();
            var token = _inRangeColorCTS.Token;

            try
            {
                // Set CurrentStateColor to CollisionStateColor
                SetInRangeColor();

                // Wait for the specified duration or until cancelled
                await Task.Delay(TimeSpan.FromSeconds(InRangeColorDuration), token);

                // Set CurrentStateColor to DefaultColor
                ResetInRangeColor();
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation
                ResetInRangeColor();
            }
        }

#endif
    }
}