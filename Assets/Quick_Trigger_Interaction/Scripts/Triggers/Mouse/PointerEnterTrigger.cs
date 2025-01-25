// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    [AddComponentMenu("QTI/Triggers/Mouse/PointerEnterTrigger")]
    public class PointerEnterTrigger : InteractionTrigger, IPointerEnterHandler
    {
        public IInteractor interactor;

        public void OnPointerEnter(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}