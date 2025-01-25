// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics2D
{
    [AddComponentMenu("QTI/Triggers/Physics2D/StayOn2DTrigger")]
    public class StayOn2DTrigger : Physics2DTrigger
    {
        [SerializeField] private bool hasCooldown = false;
        [SerializeField] private float cooldownTimer = 1.0f;
        private float _elapsedTime = Mathf.Infinity;
        private bool _canInteract = true;

        private void OnTriggerStay2D(Collider2D otherCollider)
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

            if (FilterInteractor(otherCollider.gameObject, out IInteractor interactor))
            {
                base.Interact(interactor);
            }
            else return;

            _canInteract = false;
            _elapsedTime = 0;

#if UNITY_EDITOR

            SetCollisionColor();

#endif
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!PhysicsHelper.ContainsLayer(other.gameObject.layer, layerMask))
            {
                return;
            }

            _canInteract = true;
            _elapsedTime = Mathf.Infinity;

#if UNITY_EDITOR

            ResetCollisionColor();

#endif
        }
    }
}