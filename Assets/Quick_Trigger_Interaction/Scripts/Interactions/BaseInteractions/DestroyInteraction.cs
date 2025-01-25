// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [AddComponentMenu("QTI/Interactions/DestroyInteraction")]
    public class DestroyInteraction : Interaction
    {
        public GameObject[] toDestroy;
        public bool alsoDestroyInteractor = false;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            for (int i = 0; i < toDestroy.Length; i++)
            {
                if (toDestroy[i] != null)
                {
                    Destroy(toDestroy[i]);
                }
            }

            if (alsoDestroyInteractor && interactor != null && interactor.Transform != null)
            {
                Destroy(interactor.Transform.gameObject);
            }

            OnEnd();
        }
    }
}