// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    [AddComponentMenu("QTI/Triggers/Mouse/PointerDragTrigger")]
    public class PointerDragTrigger : InteractionTrigger, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public IInteractor interactor;

        public enum EventType
        {
            BeginDrag,
            Drag,
            EndDrag
        }

        public EventType triggerOn;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (triggerOn == EventType.BeginDrag)
            {
                base.Interact(interactor);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (triggerOn == EventType.Drag)
            {
                base.Interact(interactor);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (triggerOn == EventType.EndDrag)
            {
                base.Interact(interactor);
            }
        }
    }
}