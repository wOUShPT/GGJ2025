// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers.Lifecycle
{
    [AddComponentMenu("QTI/Triggers/Lifecycle/OnEnableTrigger")]
    public class OnEnableTrigger : InteractionTrigger
    {
        public IInteractor interactor;

        private void OnEnable()
        {
            Interact(interactor);
        }

        // overrides Interact method from base class, don't want OnEnableTrigger disabling itself
        public override void Interact(IInteractor interactor)
        {
            if (interaction == null)
            {
                return;
            }

            interaction.Interact(interactor);
        }
    }
}