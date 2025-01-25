// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class RotationInteraction : Interaction
    {
        public float turnSpeed = 1.0f;
        public Rigidbody rb;
        public PlayerControllerDemo player;
        private IInteractor playerInteractor;
        public bool invertDirection;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            playerInteractor = player;

            Vector3 targetPosition = new Vector3(playerInteractor.GetPosition2D().x, rb.transform.position.y,
                playerInteractor.GetPosition2D().y);

            Vector3 direction = (targetPosition - rb.position).normalized;

            Quaternion toRotation = Quaternion.LookRotation(invertDirection ? -direction : direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, toRotation, turnSpeed * Time.fixedDeltaTime));

            OnEnd();
        }
    }
}