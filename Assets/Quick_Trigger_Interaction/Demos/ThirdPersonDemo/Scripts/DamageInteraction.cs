// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class DamageInteraction : Interaction, IInteractor
    {
        [SerializeField] int damage;

        public Transform GetTransform()
        {
            return transform;
        }

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            Damage(interactor);
            OnEnd();
        }

        private void Damage(IInteractor interactor)
        {
            if (interactor != null)
            {
                Debug.Log("DAMAGED " + interactor.ToString());
                if (interactor is IDamageable)
                {
                    (interactor as IDamageable).TakeDamage(damage);
                }
            }
        }
    }
}