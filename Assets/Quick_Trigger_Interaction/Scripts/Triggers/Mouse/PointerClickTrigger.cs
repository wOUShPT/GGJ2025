// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstralShift.QTI.Triggers.Mouse
{
    /// <summary>
    /// Fires when the button was pressed and released on the same object, containing this script,
    /// see PointerClickTrigger for a different Behaviour
    /// </summary>
    /// </summary>
    [AddComponentMenu("QTI/Triggers/Mouse/PointerClickTrigger")]
    public class PointerClickTrigger : InteractionTrigger, IPointerClickHandler
    {
        public IInteractor interactor;

        public void OnPointerClick(PointerEventData eventData)
        {
            base.Interact(interactor);
        }
    }
}