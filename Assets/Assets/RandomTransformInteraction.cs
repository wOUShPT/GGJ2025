
// Copyright (c) AstralShift. All rights reserved.
using AstralShift.QTI.Helpers.Attributes;
using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactors;
using UnityEngine;

public class RandomTransformInteraction : Interaction
{
    public Vector2 positionXRange;
    public Vector2 positionYRange;
    public Vector2 scaleRange;

    public Transform targetObject;

    public override void Interact(IInteractor interactor)
    {
        base.Interact(interactor);

        if (targetObject != null)
        {
            float newSize = Random.Range(scaleRange.x, scaleRange.y);
            targetObject.localScale = Vector3.one * newSize;

            float newPositionX = Random.Range(positionXRange.x, positionXRange.y);
            float newPositionY = Random.Range(positionYRange.x, positionYRange.y);
            targetObject.transform.position = new Vector3(newPositionX, newPositionY, targetObject.transform.position.z);
        }
        else
            Debug.LogError(nameof(SetScaleInteraction) + ": "
                                                       + nameof(targetObject) + " is null!");

        OnEnd();
    }
}
