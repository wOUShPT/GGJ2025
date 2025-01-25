// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class Enemy : MonoBehaviour, IInteractor, IDamageable
    {
        [SerializeField] private int hp;

        public int HP
        {
            get { return hp; }
            protected set { hp = value; }
        }

        public Interaction deathInteraction;

        public Transform GetTransform()
        {
            return transform;
        }

        public void TakeDamage(int dmg)
        {
            HP -= dmg;
            if (HP <= 0)
            {
                deathInteraction.Interact(this);
            }
        }
    }
}