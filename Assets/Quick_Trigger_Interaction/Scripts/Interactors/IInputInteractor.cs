// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Triggers.Physics;

namespace AstralShift.QTI.Interactors
{
    public interface IInputInteractor : IInteractor
    {
        public abstract InputTrigger GetInteraction();

        public abstract bool TryInteract();
    }
}