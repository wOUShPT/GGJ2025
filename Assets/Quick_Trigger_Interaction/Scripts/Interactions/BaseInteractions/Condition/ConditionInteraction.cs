// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers.Attributes;
using AstralShift.QTI.Interactors;
using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [AddComponentMenu("QTI/Interactions/Condition/ConditionInteraction")]
    public class ConditionInteraction : Interaction
    {
        public bool useConditionClass;

        [SerializeField, ConditionalHide(nameof(useConditionClass), true)]
        private Condition condition;

        [ConditionalHide(nameof(useConditionClass), false)]
        public bool isTrue;

        public bool IsTrue
        {
            get { return isTrue; }
            set { isTrue = value; }
        }

        public List<Interaction> onTrueInteractions = new List<Interaction>();
        public List<Interaction> onFalseInteractions = new List<Interaction>();

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            if (useConditionClass)
            {
                if (condition.Verify(interactor))
                {
                    RunInteractionList(onTrueInteractions);
                }
                else
                {
                    RunInteractionList(onFalseInteractions);
                }
            }
            else
            {
                if (isTrue)
                {
                    RunInteractionList(onTrueInteractions);
                }
                else
                {
                    RunInteractionList(onFalseInteractions);
                }
            }
        }

        private void RunInteractionList(List<Interaction> interactions)
        {
            if (interactions.Count > 0)
            {
                foreach (var interaction in interactions)
                {
                    if (!(interaction && interaction.gameObject)) continue;

                    if (!interaction.gameObject.activeSelf)
                    {
                        Debug.Log("Object not Active: Interaction will be ignored!");
                        continue; //added so only certain interactions are performed, if interactions are being ignored check if the gameobject is active
                    }

                    interaction.Interact(_interactor, _triggerActivation);
                }
            }
            else
            {
                _triggerActivation?.Invoke();
            }
        }
    }
}