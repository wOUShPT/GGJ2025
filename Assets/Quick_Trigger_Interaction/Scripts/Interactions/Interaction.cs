// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using AstralShift.QTI.Triggers;
using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    /// <summary>
    /// Base class for the Interactions
    /// </summary>
    public abstract class Interaction : MonoBehaviour
    {
        public List<Interaction> onEndInteractions = new List<Interaction>();

        protected IInteractor _interactor;

        protected InteractionTrigger.TriggerActivation _triggerActivation;

        public virtual void Interact(IInteractor interactor, InteractionTrigger.TriggerActivation triggerActivation)
        {
            _triggerActivation = triggerActivation;
            Interact(interactor);
        }

        public virtual void Interact(IInteractor interactor)
        {
            _interactor = interactor;
        }

        public void OnEnd()
        {
            if (onEndInteractions.Count > 0)
            {
                foreach (var interaction in onEndInteractions)
                {
                    if (!(interaction && interaction.gameObject))
                    {
                        Debug.LogError("Interaction is Null: chain broken!" +
                                       " Remove all Empty elements from the OnEndInteractions of every Interaction in the chain!");
                        _triggerActivation?.Invoke();
                        break;
                    }

                    interaction.Interact(_interactor, _triggerActivation);
                }
            }
            else
            {
                _triggerActivation?.Invoke();
            }
        }

        public virtual bool CanInteract()
        {
            return true;
        }
    }
}