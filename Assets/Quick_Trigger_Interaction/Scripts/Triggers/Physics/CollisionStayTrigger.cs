// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics
{
    [AddComponentMenu("QTI/Triggers/Physics/CollisionStayTrigger")]
    public class CollisionStayTrigger : PhysicsTrigger
    {
        [SerializeField] private bool hasCooldown = false;
        [SerializeField] private float cooldownTimer = 1.0f;
        private float _elapsedTime = Mathf.Infinity;
        private bool _canInteract = true;

        protected override void Awake()
        {
            base.Awake();
            RefreshCollider();
        }


        private void OnCollisionStay(Collision otherCollision)
        {
            if (_elapsedTime < cooldownTimer)
            {
                _elapsedTime += Time.deltaTime;
                _canInteract = _elapsedTime >= cooldownTimer;
            }

            if (hasCooldown && !_canInteract)
            {
#if UNITY_EDITOR

                ResetCollisionColor();

#endif

                return;
            }

            if (!enabled || !PhysicsHelper.ContainsLayer(otherCollision.gameObject.layer, layerMask))
            {
                return;
            }

            if (!otherCollision.gameObject.TryGetComponent(out IInteractor interactor))
            {
                return;
            }

            base.Interact(interactor);
            _canInteract = false;
            _elapsedTime = 0;

#if UNITY_EDITOR

            SetCollisionColor();

#endif
        }

        private void OnCollisionExit(Collision otherCollision)
        {
            if (FilterInteractor(otherCollision.gameObject, out IInteractor interactor))
            {
                base.Interact(interactor);
            }
            else return;

            _canInteract = true;
            _elapsedTime = Mathf.Infinity;

#if UNITY_EDITOR

            ResetCollisionColor();

#endif
        }

        public override void RefreshCollider()
        {
            if (_collider == null)
            {
                _collider = gameObject.GetComponent<Collider>();
                if (_collider == null) return; // on instantiate scenario
            }

            _collider.isTrigger = false;
        }
    }
}