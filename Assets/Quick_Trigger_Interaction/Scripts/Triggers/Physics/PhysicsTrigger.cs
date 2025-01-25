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

namespace AstralShift.QTI.Triggers.Physics
{
    public abstract class PhysicsTrigger : InteractionTrigger
    {
        public LayerMask layerMask;
        public bool useTag = false;
        [HideInInspector] public string targetTag = "Untagged";

        protected virtual void Reset()
        {
            layerMask = UnityEngine.Physics.AllLayers;
        }

        protected Collider _collider;

        public virtual void RefreshCollider()
        {
            if (_collider == null)
            {
                _collider = gameObject.GetComponent<Collider>();
                if (_collider == null) return; // on instantiate scenario
            }

            if (_collider is MeshCollider meshCollider)
            {
                meshCollider.convex = true;
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

        /// <summary>
        /// Multiply scale by an offset to prevent gizmo overlapping when mesh and collider are matching
        /// </summary>
        private const float ScaleOffsetValue = 1.0001f;

        private CapsuleCollider _capsuleCollider;
        private Mesh _capsuleMesh;
        private float _capsuleColliderRadius;
        private float _capsuleColliderHeight;
        private Vector3 _capsuleColliderCenter;
        private Vector3 _lastScale;

        private const int CapsuleSegments = 10;
        private const int CapsuleRings = 10;

        private void OnDrawGizmosSelected()
        {
            Color defaultColor = Gizmos.color;

            RefreshCollider();
            RefreshInteractionLayer();
            
            if (_collider == null)
            {
                return;
            }

            Vector3 lossyScale = _collider.transform.lossyScale * ScaleOffsetValue;

            // Draw for BoxCollider
            if (_collider is BoxCollider boxCollider)
            {
                Gizmos.color = _currentStateColor;
                var defaultMatrix = Gizmos.matrix;
                Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
                Gizmos.matrix = defaultMatrix;
            }

            // Draw for SphereCollider
            if (_collider is SphereCollider sphereCollider)
            {
                Gizmos.color = _currentStateColor;
                float radius = sphereCollider.radius * Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
                Gizmos.DrawSphere(transform.position + sphereCollider.center, radius);
            }

            // Draw for CapsuleCollider
            if (_collider is CapsuleCollider capsuleCollider)
            {
                Gizmos.color = _currentStateColor;
                _capsuleCollider = capsuleCollider;
                RefreshCapsule();
                Gizmos.DrawMesh(_capsuleMesh, transform.position + _capsuleCollider.center, transform.rotation);
            }

            // Draw for MeshCollider
            if (_collider is MeshCollider meshCollider)
            {
                Gizmos.color = _currentStateColor;
                var defaultMatrix = Gizmos.matrix;
                Gizmos.DrawMesh(meshCollider.sharedMesh, transform.position, meshCollider.transform.rotation,
                    lossyScale);
                Gizmos.matrix = defaultMatrix;
            }

            Gizmos.color = defaultColor;
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

        #region CapsuleColliderGizmo

        private void RefreshCapsule()
        {
            if (_capsuleCollider == null)
            {
                return;
            }

            if (_capsuleColliderRadius != _capsuleCollider.radius ||
                _capsuleColliderHeight != _capsuleCollider.height ||
                _capsuleColliderCenter != _capsuleCollider.center ||
                _lastScale != _capsuleCollider.transform.lossyScale)
            {
                GenerateNewCapsuleMesh();
            }
        }

        private void GenerateNewCapsuleMesh()
        {
            Vector3 lossyScale = _capsuleCollider.transform.lossyScale;
            float largestXZPlaneAxis = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
            _capsuleMesh = Helpers.GizmosHelpers.GenerateCapsuleMesh(_capsuleCollider.radius * largestXZPlaneAxis,
                _capsuleCollider.height * Mathf.Abs(lossyScale.y), CapsuleSegments, CapsuleRings);
            _capsuleColliderRadius = _capsuleCollider.radius;
            _capsuleColliderHeight = _capsuleCollider.height;
            _capsuleColliderCenter = _capsuleCollider.center;
            _lastScale = _capsuleCollider.transform.lossyScale;
        }

        #endregion

#endif
    }
}