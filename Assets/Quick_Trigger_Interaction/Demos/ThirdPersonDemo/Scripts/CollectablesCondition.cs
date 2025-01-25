// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class CollectablesCondition : Condition
    {
        [SerializeField] int amount = 5;

        public override bool Verify(IInteractor interactor)
        {
            if (interactor == null)
            {
                return false;
            }

            if (interactor is not PlayerControllerDemo playerController)
            {
                return false;
            }

            if (!playerController.TryGetComponent(out Collector collector))
            {
                return false;
            }

            return collector.CollectedItems >= amount;
        }
    }
}