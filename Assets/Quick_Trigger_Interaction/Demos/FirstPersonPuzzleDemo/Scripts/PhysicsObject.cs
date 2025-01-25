// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;
using AstralShift.QTI.Interactors;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AstralShift.QTI.Interactions.Demos.FPPuzzleDemo
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class PhysicsObject : MonoBehaviour, IGravityField, IInteractor
    {
        public Rigidbody rb;
        private Collider _collider;

        [Header("Ground Detection")] public LayerMask layerMask = -1;
        public float rayDistance = 1;
        private RaycastHit[] _groundHits;

        private bool _inGravityField;
        private Vector3 _gravityFieldVelocity;
        private Vector3 _gravityVelocity;
        private const float GravityConst = -9.81f;

        public void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            _collider = GetComponent<Collider>();
            _groundHits = new RaycastHit[2];
        }

        public void FixedUpdate()
        {
            DetectGround();
        }

        private void Update()
        {
            ApplyGravity();
        }

        private void ApplyGravity()
        {
            if (_inGravityField)
            {
                rb.useGravity = false;
                rb.MovePosition(rb.position + _gravityFieldVelocity);
            }
            else
            {
                rb.useGravity = true;
            }
        }

        private void DetectGround()
        {
            int numberOfHits = Physics.RaycastNonAlloc(_collider.bounds.center, Vector3.down, _groundHits, rayDistance,
                layerMask, QueryTriggerInteraction.Ignore);
            if (numberOfHits == 0)
            {
                _inGravityField = false;
                return;
            }

            foreach (var hit in _groundHits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider == _collider)
                {
                    continue;
                }

                if (hit.collider.TryGetComponent(out IGravityField gravityField))
                {
                    _inGravityField = true;
                    _gravityFieldVelocity = gravityField.GetMovementDelta();
                    return;
                }
            }

            _gravityFieldVelocity = Vector3.zero;
            _inGravityField = false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }

            Color defaultColor = Handles.color;
            Handles.color = Color.red;
            Handles.DrawLine(_collider.bounds.center, _collider.bounds.center + Vector3.down * rayDistance, 2);
            Handles.color = defaultColor;
        }

#endif
        public Transform GetTransform()
        {
            return transform;
        }
    }
}