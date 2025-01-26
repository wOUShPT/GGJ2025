using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactors;
using AstralShift.QTI.Triggers;
using UnityEngine;

public class CounterInteraction : Interaction
{
    public int increment = 1;
    public override void Interact(IInteractor interactor)
    {
        base.Interact(interactor);
        GameManager.Instance.IncreaseCounter(increment);
        OnEnd();
    }
}
