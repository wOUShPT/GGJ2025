// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Lifecycle
{
    [AddComponentMenu("QTI/Triggers/Lifecycle/OnUpdateTrigger")]
    public class UpdateTrigger : InteractionTrigger
    {
        public IInteractor interactor;

        void Update()
        {
            base.Interact(interactor);
        }
    }
}