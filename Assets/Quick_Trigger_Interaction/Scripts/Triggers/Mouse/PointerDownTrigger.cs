// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    [AddComponentMenu("QTI/Triggers/Mouse/PointerDownTrigger")]
    public class PointerDownTrigger : InteractionTrigger, IPointerDownHandler
    {
        public IInteractor interactor;

        public void OnPointerDown(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}