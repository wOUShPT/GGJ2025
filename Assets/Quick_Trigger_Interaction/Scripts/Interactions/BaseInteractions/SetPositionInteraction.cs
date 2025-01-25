// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [AddComponentMenu("QTI/Interactions/SetPositionInteraction")]
    public class SetPositionInteraction : Interaction
    {
        public enum Mode
        {
            transform = 0,
            position = 1,
        }

        public Mode mode;
        public Transform targetObject;
        public Transform newPositionTransform;
        public Vector3 newPosition;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            if (targetObject != null)
            {
                targetObject.TryGetComponent(out CharacterController characterController);

                switch (mode)
                {
                    case Mode.transform:

                        if (newPositionTransform != null)
                        {
                            // If the target has a CharacterController, reset it to avoid inconsistent behaviour
                            if (characterController != null)
                            {
                                characterController.enabled = false;
                                targetObject.position = newPositionTransform.position;
                                characterController.enabled = true;
                            }
                            else
                            {
                                targetObject.position = newPositionTransform.position;
                            }
                        }
                        else
                            Debug.LogError(nameof(SetPositionInteraction) + ": "
                                                                          + nameof(newPositionTransform) + " is null!");

                        break;

                    case Mode.position:

                        // If the target has a CharacterController, reset it to avoid inconsistent behaviour
                        if (characterController != null)
                        {
                            characterController.enabled = false;
                            targetObject.position = newPosition;
                            characterController.enabled = true;
                        }
                        else
                        {
                            targetObject.position = newPosition;
                        }

                        break;

                    default:
                        break;
                }
            }
            else
                Debug.LogError(nameof(SetPositionInteraction) + ": "
                                                              + nameof(targetObject) + " is null!");

            OnEnd();
        }
    }
}