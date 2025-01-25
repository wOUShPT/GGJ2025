// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics2D
{
    [AddComponentMenu("QTI/Triggers/Physics2D/CollisionEnter2DTrigger")]
    public class CollisionEnter2DTrigger : Physics2DTrigger
    {
        protected override void Awake()
        {
            base.Awake();
            RefreshCollider();
        }

        private void OnCollisionEnter2D(Collision2D otherCollision)
        {
            if (FilterInteractor(otherCollision.gameObject, out IInteractor interactor))
            {
                base.Interact(interactor);
            }
            else return;

#if UNITY_EDITOR

#pragma warning disable 4014
            TriggerCollisionColor();
#pragma warning restore 4014

#endif
        }

        public override void RefreshCollider()
        {
            base.RefreshCollider();
            _collider.isTrigger = false;
        }
    }
}