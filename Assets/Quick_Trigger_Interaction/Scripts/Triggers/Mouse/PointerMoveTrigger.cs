// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    [AddComponentMenu("QTI/Triggers/Mouse/PointerMoveTrigger")]
    public class PointerMoveTrigger : InteractionTrigger, IPointerMoveHandler
    {
        public IInteractor interactor;

        public void OnPointerMove(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}