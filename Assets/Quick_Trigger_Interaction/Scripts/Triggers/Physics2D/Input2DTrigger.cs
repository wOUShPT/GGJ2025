// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEngine;
using AstralShift.QTI.Settings;
using AstralShift.QTI.Interactions.Visuals;
using AstralShift.QTI.Interactors;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AstralShift.QTI.Triggers.Physics2D
{
    [AddComponentMenu("QTI/Triggers/Physics2D/Input2DTrigger")]
    public class Input2DTrigger : Physics2DTrigger
    {
        public PrioritiesEnumSelector priority;
        public bool isFixedAngle = false;
        [Range(0, 360)] public float interactionDirection = 0;
        protected Vector3 _forwardDirection;
        [Range(0, 360)] public float interactionAngle = 210;
        protected float _currentFacingAngle;
        public float CurrentFacingAngle => _currentFacingAngle;
        protected float _currentRelativeAngle;
        public float CurrentRelativeAngle => _currentRelativeAngle;

        public InteractionVisual interactionVisual;

        public IInput2DInteractor ClosestInteractor { get; set; }

#if UNITY_EDITOR
        private void Start()
        {
            RefreshInteractionLayer();
        }
#endif
        public override void Interact(IInteractor interactor)
        {
            if (interaction == null)
            {
                Debug.LogError("No Interaction assigned to Trigger!!");
                return;
            }

            if (enabled && interaction)
            {
                if (!interaction.enabled)
                {
                    return;
                }

                if (CanInteract(GetPosition2D()))
                {
                    base.Interact(interactor);
                    interactionVisual?.Interact(); //show INTERACTED interaction visual for interaction if it has any
                }
                else
                {
                    Debug.Log("Incorrect facing interactionDirection.");
                }
            }
        }

        public virtual bool CanInteract(Vector2 position)
        {
            if (!interaction.enabled || !interaction.CanInteract())
            {
                ResetVisuals();
                return false;
            }

            Vector2 directionToInteractor = Helpers.Math.GetDirectionAtoB(position, GetPosition2D());
            bool result;
            if (isFixedAngle)
            {
                _forwardDirection = Quaternion.AngleAxis(interactionDirection, transform.forward) *
                                    GetFacingDirection2D();
                _currentFacingAngle = Vector2.Angle(_forwardDirection, directionToInteractor);
                _currentRelativeAngle = Vector2.Angle(_forwardDirection, directionToInteractor);
                result = _currentFacingAngle <= interactionAngle / 2 && _currentRelativeAngle <= interactionAngle / 2;
            }
            else
            {
                _currentFacingAngle = Vector2.Angle(directionToInteractor, directionToInteractor);
                result = _currentFacingAngle <= interactionAngle / 2;
            }

            // Sets Gizmo in Editor
#if UNITY_EDITOR

            SetInteractionGizmoState(result ? GizmoState.Available : GizmoState.InRange);

#endif

            if (!result)
            {
                ResetVisuals();
            }

            return result;
        }

        public override Vector3 GetFacingDirection()
        {
            return transform.up;
        }

        public override Vector2 GetFacingDirection2D()
        {
            return transform.up;
        }

        public void HighlightVisuals()
        {
            if (enabled && interaction.enabled)
            {
                interactionVisual?.Highlight();
            }
        }

        public void DisableVisuals()
        {
            interactionVisual?.Disable(); //show DISABLED interaction visual for interaction if it has any
        }

        public void ResetVisuals()
        {
            interactionVisual?.Idle();
        }

        protected void OnDisable()
        {
            DisableVisuals();
        }

        protected void OnEnable()
        {
            interactionVisual?.Enable();
        }

#if UNITY_EDITOR
        protected override void RefreshInteractionLayer()
        {
#if UNITY_2022_3_OR_NEWER
            gameObject.layer = InteractionsSettings.Instance.GetInputTriggerLayer(gameObject.layer);
#else
            if (Application.isPlaying)
            {
                gameObject.layer = InteractionsSettings.Instance.GetInputTriggerLayer(gameObject.layer);
            }
            else
            {
                EditorApplication.delayCall += UpdateInteractionLayerLegacy;
            }
#endif
        }


#if !UNITY_2022_3_OR_NEWER
        private void UpdateInteractionLayerLegacy()
        {
            gameObject.layer = InteractionsSettings.Instance.GetInputTriggerLayer(gameObject.layer);
            EditorApplication.delayCall -= UpdateInteractionLayerLegacy;
        }
#endif


        /// <summary>
        /// Editor Gizmos
        /// </summary>
        protected virtual void DrawAngleGizmo()
        {
            Color defaultColor = Handles.color;
            Handles.color = _triggerGizmoColor;

            if (isFixedAngle || !Application.isPlaying)
            {
                Vector3 startDirection = Quaternion.AngleAxis(-interactionAngle / 2, Vector3.forward) *
                                         (Quaternion.AngleAxis(interactionDirection, Vector3.forward) * -transform.up);
                Handles.DrawSolidArc(transform.position, transform.forward, startDirection, interactionAngle, 0.5f);
                Helpers.GizmosHelpers.DrawArrow(transform.position, _forwardDirection, Vector3.forward, Color.white, 1);
            }
            else if (ClosestInteractor != null)
            {
                Vector3 triggerToPlayerDirection =
                    Helpers.Math.GetDirectionAtoB(GetPosition2D(), ClosestInteractor.GetPosition2D());
                Vector3 startDirection = Quaternion.AngleAxis(-interactionAngle / 2, Vector3.forward) *
                                         triggerToPlayerDirection;
                Handles.DrawSolidArc(transform.position, transform.forward, startDirection, interactionAngle, 0.5f);
            }

            Handles.color = Color.white;
            Handles.DrawLine(transform.position, transform.position, 1.5f);

            Handles.color = defaultColor;

            if (isFixedAngle)
            {
                GUIStyle infoLabelStyle = new GUIStyle("label");
                infoLabelStyle.fontSize = 10;
                Handles.Label(transform.position + Vector3.up * 1.5f, Mathf.Round(CurrentRelativeAngle).ToString(),
                    infoLabelStyle);
            }

            SetInteractionGizmoState(GizmoState.Unavailable);
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                return;
            }

            DrawAngleGizmo();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DrawAngleGizmo();
        }

        protected GizmoState _triggerGizmoState;
        protected Color _triggerGizmoColor = Color.red;

        protected enum GizmoState
        {
            Unavailable,
            InRange,
            Available
        }

        protected void SetInteractionGizmoState(GizmoState state)
        {
            _triggerGizmoState = state;
            switch (_triggerGizmoState)
            {
                case GizmoState.Unavailable:

                    _triggerGizmoColor = Color.red;
                    break;

                case GizmoState.InRange:

                    _triggerGizmoColor = Color.blue;
                    break;

                case GizmoState.Available:

                    _triggerGizmoColor = Color.green;
                    break;
            }

            _triggerGizmoColor.a = 0.5f;
        }
#endif
    }
}