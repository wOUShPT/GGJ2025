// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Lifecycle
{
    [AddComponentMenu("QTI/Triggers/Lifecycle/FixedUpdateTrigger")]
    public class FixedUpdateTrigger : InteractionTrigger
    {
        public IInteractor interactor;

        void FixedUpdate()
        {
            base.Interact(interactor);
        }
    }
}