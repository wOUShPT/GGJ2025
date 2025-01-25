// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics2D
{
    [AddComponentMenu("QTI/Triggers/Physics2D/StepOn2DTrigger")]
    public class StepOn2DTrigger : Physics2DTrigger
    {
        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (FilterInteractor(otherCollider.gameObject, out IInteractor interactor))
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
    }
}