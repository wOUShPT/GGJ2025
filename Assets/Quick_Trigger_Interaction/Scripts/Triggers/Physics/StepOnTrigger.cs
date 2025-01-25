// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Physics
{
    [AddComponentMenu("QTI/Triggers/Physics/StepOnTrigger")]
    public class StepOnTrigger : PhysicsTrigger
    {
        private void OnTriggerEnter(Collider otherCollider)
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