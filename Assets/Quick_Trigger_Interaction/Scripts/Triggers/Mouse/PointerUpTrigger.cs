// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    /// <summary>
    /// Fires when the button is released on the any object, see PointerClickTrigger for a different Behaviour
    /// </summary>
    [AddComponentMenu("QTI/Triggers/Mouse/PointerUpTrigger")]
    public class PointerUpTrigger : InteractionTrigger, IPointerUpHandler, IPointerDownHandler
    {
        public IInteractor interactor;

        public void OnPointerDown(PointerEventData eventData)
        {
            // should be implemented in order for OnPointerUp to work
            return;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}