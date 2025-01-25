// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Lifecycle
{
    [AddComponentMenu("QTI/Triggers/Lifecycle/LateUpdateTrigger")]
    public class LateUpdateTrigger : InteractionTrigger
    {
        public IInteractor interactor;

        void LateUpdate()
        {
            base.Interact(interactor);
        }
    }
}