// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

#if UNITY_EDITOR
using UnityEditor;
using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactors;

#endif

namespace AstralShift.QTI.Triggers
{
    public class OnDistance2DTrigger : OnDistanceTrigger
    {
        protected override float CalculateDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(a, b);
        }

#if UNITY_EDITOR

        protected new const float GizmoThickness = 4;

        protected override void OnDrawGizmosSelected()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            Camera camera = sceneView ? sceneView.camera : Camera.current;

            Vector3 interactionPosition = transform.position;
            interactionPosition.z = 0;
            using (new Handles.DrawingScope(_currentStateColor))
            {
                Handles.DrawWireDisc(interactionPosition, Vector3.forward, distance, GizmoThickness);
            }

            if (_targets == null)
            {
                return;
            }

            foreach (var key in _targets.Keys)
            {
                IInteractor interactor = key;
                Vector3 targetPosition = interactor.GetTransform().position;
                targetPosition.z = 0;
                Vector3 direction = targetPosition - interactionPosition;

                GizmosHelpers.DrawArrow(interactionPosition, direction, Vector3.forward,
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
#endif
    }
}