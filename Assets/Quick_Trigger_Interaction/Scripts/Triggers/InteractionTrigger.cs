// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactors;
using UnityEngine;
using Interaction = AstralShift.QTI.Interactions.Interaction;

namespace AstralShift.QTI.Triggers
{
    public abstract class InteractionTrigger : MonoBehaviour
    {
        public Interaction interaction;

        public delegate void TriggerActivation();

        private TriggerActivation _activateTrigger;
        protected int CurrentActivationCount;
        protected int MaxActivationCount;

        protected virtual void Awake()
        {
        }

        public virtual Vector2 GetPosition2D()
        {
            return new Vector2(transform.position.x, transform.position.z);
        }

        public virtual Vector2 GetFacingDirection2D()
        {
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            return new Vector2(direction.x, direction.z);
        }

        public virtual Vector3 GetFacingDirection()
        {
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            return direction;
        }

        /// <summary>
        /// Defines an interaction behaviour, this will be called by an interactor
        /// </summary>
        /// <param name="interactor"></param>
        public virtual void Interact(IInteractor interactor)
        {
            if (interaction == null)
            {
                return;
            }

            MaxActivationCount = GetLeafCount(interaction);
            _activateTrigger = ActivateTrigger;
            enabled = false;
            interaction.Interact(interactor, _activateTrigger);
        }

        protected int GetLeafCount(Interaction interaction)
        {
            if (interaction is ConditionInteraction conditionInteraction)
            {
                int count = 0;
                if (conditionInteraction.onTrueInteractions == null ||
                    conditionInteraction.onTrueInteractions.Count == 1)
                {
                    count = 1;
                }
                else
                {
                    foreach (var onTrueInteraction in conditionInteraction.onTrueInteractions)
                    {
                        count += GetLeafCount(onTrueInteraction);
                    }
                }

                if (conditionInteraction.onFalseInteractions == null ||
                    conditionInteraction.onFalseInteractions.Count == 1)
                {
                    count = 1;
                }
                else
                {
                    foreach (var onFalseInteraction in conditionInteraction.onFalseInteractions)
                    {
                        count += GetLeafCount(onFalseInteraction);
                    }
                }

                return count;
            }
            else
            {
                if (interaction.onEndInteractions == null)
                {
                    return 1;
                }

                if (interaction.onEndInteractions.Count == 0)
                {
                    return 1;
                }

                int count = 0;
                foreach (var interactionOnEnd in interaction.onEndInteractions)
                {
                    count += GetLeafCount(interactionOnEnd);
                }

                return count;
            }
        }

        public void ActivateTrigger()
        {
            CurrentActivationCount++;
            if (CurrentActivationCount == MaxActivationCount)
            {
                CurrentActivationCount = 0;
                enabled = true;
            }
        }
    }
}