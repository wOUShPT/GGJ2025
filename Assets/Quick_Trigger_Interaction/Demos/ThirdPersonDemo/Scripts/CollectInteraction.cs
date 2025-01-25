// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class CollectInteraction : Interaction
    {
        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            if (interactor is PlayerControllerDemo &&
                (interactor as PlayerControllerDemo).TryGetComponent(out Collector collector))
            {
                collector.Collect();
            }

            OnEnd();
        }
    }
}