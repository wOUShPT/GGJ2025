// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    [AddComponentMenu("QTI/Triggers/Mouse/PointerExitTrigger")]
    public class PointerExitTrigger : InteractionTrigger, IPointerExitHandler
    {
        public IInteractor interactor;

        public void OnPointerExit(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}