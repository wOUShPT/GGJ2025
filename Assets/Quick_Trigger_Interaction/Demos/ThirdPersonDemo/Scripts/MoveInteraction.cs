// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class MoveInteraction : Interaction
    {
        public float speed = 1.0f;
        public Rigidbody rb;
        public PlayerControllerDemo player;
        private IInteractor playerInteractor;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            playerInteractor = player;

            Vector3 targetPosition = new Vector3(playerInteractor.GetPosition2D().x, rb.transform.position.y,
                playerInteractor.GetPosition2D().y);

            Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            OnEnd();
        }
    }
}