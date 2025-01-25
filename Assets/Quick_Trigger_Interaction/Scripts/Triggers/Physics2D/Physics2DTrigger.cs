// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEngine;
using AstralShift.QTI.Settings;
using AstralShift.QTI.Interactors;
using AstralShift.QTI.Helpers;

#if UNITY_EDITOR
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
#endif

namespace AstralShift.QTI.Triggers.Physics2D
{
    public abstract class Physics2DTrigger : InteractionTrigger
    {
        public LayerMask layerMask;
        public bool useTag = false;
        [HideInInspector] public string targetTag = "Untagged";

        protected virtual void Reset()
        {
            layerMask = UnityEngine.Physics.AllLayers;
        }

        protected Collider2D _collider;

        public virtual void RefreshCollider()
        {
            if (_collider == null)
            {
                _collider = gameObject.GetComponent<Collider2D>();
                if (_collider == null) return; // on instantiate scenario
            }

            _collider.isTrigger = true;
        }

        protected bool FilterInteractor(GameObject go, out IInteractor interactor)
        {
            interactor = null;
            if (!enabled || !PhysicsHelper.ContainsLayer(go.layer, layerMask) ||
                useTag && !go.CompareTag(targetTag) || !go.TryGetComponent(out interactor))
            {
                return false;
            }

            return true;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshCollider();
            RefreshInteractionLayer();
        }
        
        protected virtual void RefreshInteractionLayer()
        {
#if UNITY_2022_3_OR_NEWER
            gameObject.layer = InteractionsSettings.Instance.GetCollisionTriggerLayer(gameObject.layer);
#else
            EditorApplication.delayCall += UpdateInteractionLayerLegacy;
#endif
        }

#if !UNITY_2022_3_OR_NEWER
        private void  UpdateInteractionLayerLegacy()
        {
            gameObject.layer = InteractionsSettings.Instance.GetCollisionTriggerLayer(gameObject.layer);
            EditorApplication.delayCall -= UpdateInteractionLayerLegacy;
        }
#endif

        private void OnDrawGizmosSelected()
        {
            // Save the default color to restore later
            Color defaultColor = Handles.color;

            // Ensure the collider and layers are refreshed
            RefreshCollider();
            RefreshInteractionLayer();
            
            if (_collider == null)
            {
                return;
            }

            Handles.color = _currentStateColor;

            // Draw for BoxCollider2D
            if (_collider is BoxCollider2D boxCollider2D)
            {
                using (new Handles.DrawingScope(boxCollider2D.transform.localToWorldMatrix))
                {
                    Vector3 size = boxCollider2D.size;
                    Vector2 offset = boxCollider2D.offset;

                    // Apply scaling to size and calculate the world position
                    Vector2 bottomLeft = new Vector2(-size.x / 2 + offset.x, -size.y / 2 + offset.y);
                    Rect boxColliderRect = new Rect(bottomLeft, size);
                    Handles.DrawSolidRectangleWithOutline(boxColliderRect, _currentStateColor, _currentStateColor);
                }
            }

            // Draw for CircleCollider2D
            if (_collider is CircleCollider2D circleCollider2D)
            {
                Vector3 lossyScale = _collider.transform.lossyScale;
                Vector3 offset = circleCollider2D.offset;

                // Scale radius by the largest axis on the XY Plane
                float radius = circleCollider2D.radius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                Handles.DrawSolidDisc(circleCollider2D.transform.position + offset, Vector3.forward, radius);
            }

            // Draw for CapsuleCollider2D
            if (_collider is CapsuleCollider2D capsuleCollider2D)
            {
                Vector2 offset = capsuleCollider2D.offset;
                Vector2 size = capsuleCollider2D.size;
                DrawCapsuleGizmo(capsuleCollider2D.transform, offset, size, capsuleCollider2D.direction);
            }

            // Draw for PolygonCollider2D
            if (_collider is PolygonCollider2D polygonCollider2D)
            {
                using (new Handles.DrawingScope(polygonCollider2D.transform.localToWorldMatrix))
                {
                    // Store the points in world space
                    Vector3[] worldPoints = new Vector3[polygonCollider2D.points.Length];
                    for (int i = 0; i < polygonCollider2D.points.Length; i++)
                    {
                        worldPoints[i] = polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[i]);
                    }

                    if (worldPoints.Length >= 3)
                    {
                        Handles.DrawAAConvexPolygon(worldPoints);
                    }
                }
            }

            // Draw for EdgeCollider2D
            if (_collider is EdgeCollider2D edgeCollider2D)
            {
                Transform transform = edgeCollider2D.transform;
                Vector3 position = transform.position;
                Vector3 rotationEuler = transform.eulerAngles;
                Vector3 lossyScale = transform.lossyScale;

                Vector2 offset = edgeCollider2D.offset;

                // Convert the EdgeCollider2D points Vector2 -> Vector3
                // Whe handle Scale and Rotation here to avoid gizmo distortion.
                // Multiply by Scale on X & Z-Axis and Rotation
                Vector3[] points = new Vector3[edgeCollider2D.points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = edgeCollider2D.points[i] + offset;
                    points[i].x *= lossyScale.x;
                    points[i].y *= lossyScale.y;
                    points[i] = Quaternion.Euler(rotationEuler) * points[i];
                }

                // Get the Local-to-World Matrix without scaling
                // Set Position (inverted)
                // Rotation set to Quaternion.identity (don't rotate)
                // Scale factor of 1 to ignore scaling
                Matrix4x4 worldToTransformMatrix = Matrix4x4.TRS(
                    -position,
                    Quaternion.identity,
                    Vector3.one
                ).inverse;

                // Apply transformation matrix
                using (new Handles.DrawingScope(worldToTransformMatrix))
                {
                    float radius = Mathf.Clamp(edgeCollider2D.edgeRadius, 0.01f, Mathf.Infinity);

                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        DrawEdgeGizmo(points[i], points[i + 1],
                            radius, i == 0, true);
                    }
                }
            }

            // Restore the default color
            Handles.color = defaultColor;
        }


        private void DrawCapsuleGizmo(Transform transform, Vector2 offset, Vector2 size, CapsuleDirection2D direction)
        {
            Vector3 rotationEuler = transform.eulerAngles;
            Vector3 position = transform.position;
            Vector3 lossyScale = transform.lossyScale;

            if (direction == CapsuleDirection2D.Vertical)
            {
                // Set X-Axis to 0
                // We handle Rotation on X-Axis and Scale separately so it rotates and scales accordingly with Unity's CapsuleCollider2D gizmo
                Quaternion rotationYZ = Quaternion.Euler(0, rotationEuler.y, -rotationEuler.z);

                // Get the Local-to-World Matrix without scaling
                // Set Position and Rotation (Y & Z-Axis)
                // Scale factor of 1 to ignore scaling
                Matrix4x4 worldToTransformMatrix = Matrix4x4.TRS(
                    transform.position,
                    rotationYZ,
                    Vector3.one
                ).inverse;

                // Apply transformation matrix
                using (new Handles.DrawingScope(worldToTransformMatrix))
                {
                    float radius = size.x / 2 * Mathf.Abs(lossyScale.x);
                    float height = size.y * Mathf.Abs(lossyScale.y);
                    float xRotationRad = rotationEuler.x * Mathf.Deg2Rad;
                    height *= Mathf.Cos(xRotationRad);
                    GizmosHelpers.DrawCapsule2D(offset, height, radius, direction);
                }
            }
            else
            {
                // Set Y-Axis to 0
                // We handle Rotation on Y-Axis and Scale separately so it rotates and scales accordingly with Unity's CapsuleCollider2D gizmo
                Quaternion rotationXZ = Quaternion.Euler(rotationEuler.x, 0, -rotationEuler.z);

                // Get the Local-to-World Matrix without scaling
                // Set position and rotation (X & Z-Axis)
                // Scale factor of 1 to ignore scaling
                Matrix4x4 worldToTransformMatrix = Matrix4x4.TRS(
                    position,
                    rotationXZ,
                    Vector3.one
                ).inverse;

                // Apply transformation matrix
                using (new Handles.DrawingScope(worldToTransformMatrix))
                {
                    float radius = size.y / 2 * Mathf.Abs(lossyScale.y);
                    float height = size.x * Mathf.Abs(lossyScale.x);
                    float yRotationRad = rotationEuler.y * Mathf.Deg2Rad;
                    height *= Mathf.Abs(Mathf.Cos(yRotationRad));
                    GizmosHelpers.DrawCapsule2D(offset, height, radius, direction);
                }
            }
        }

        private void DrawEdgeGizmo(Vector3 startPoint, Vector3 endPoint, float radius, bool startCap, bool endCap)
        {
            // Calculate the direction and normal for the edge
            Vector3 edgeDirection = (endPoint - startPoint).normalized;
            Vector3 edgeNormal = Vector3.Cross(edgeDirection, Vector3.forward).normalized;

            // Calculate the four vertices of the filled quad
            Vector3 p1 = startPoint + edgeNormal * radius;
            Vector3 p2 = startPoint - edgeNormal * radius;
            Vector3 p3 = endPoint - edgeNormal * radius;
            Vector3 p4 = endPoint + edgeNormal * radius;

            // Draw the filled quad for the edge
            Handles.DrawAAConvexPolygon(p1, p2, p3, p4);

            // Draw the filled circles at the start and end points
            if (startCap)
            {
                Handles.DrawSolidDisc(startPoint, Vector3.forward, radius);
            }

            if (endCap)
            {
                Handles.DrawSolidDisc(endPoint, Vector3.forward, radius);
            }
        }

        private readonly Color _defaultStateColor = new Color(1, 0, 0, 0.25f);
        private readonly Color _collisionStateColor = new Color(0, 1, 0, 0.25f);
        private Color _currentStateColor = new Color(1, 0, 0, 0.25f);
        private CancellationTokenSource _collisionColorCancellationTokenSource;
        private const float CollisionColorDuration = 0.5f;

        protected void SetCollisionColor()
        {
            _currentStateColor = _collisionStateColor;
        }

        protected void ResetCollisionColor()
        {
            _currentStateColor = _defaultStateColor;
        }

        protected async Task TriggerCollisionColor()
        {
            // Cancel any previous trigger color operation
            _collisionColorCancellationTokenSource?.Cancel();

            // Create a new CancellationTokenSource
            _collisionColorCancellationTokenSource = new CancellationTokenSource();
            var token = _collisionColorCancellationTokenSource.Token;

            try
            {
                // Set CurrentStateColor to CollisionStateColor
                SetCollisionColor();

                // Wait for the specified duration or until cancelled
                await Task.Delay(TimeSpan.FromSeconds(CollisionColorDuration), token);

                // Set CurrentStateColor to DefaultColor
                ResetCollisionColor();
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation
                ResetCollisionColor();
            }
        }

#endif
    }
}