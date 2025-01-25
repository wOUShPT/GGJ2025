// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    public class SetRotationInteraction : Interaction
    {
        public Transform targetObject;

        public enum Mode
        {
            Euler,
            AngleAxis
        }

        public Mode mode;

        public enum Axis
        {
            Up,
            Right,
            Front
        }

        public Axis axis;

        public float angle;

        public Vector3 newRotation;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            if (targetObject != null)
            {
                switch (mode)
                {
                    case Mode.Euler:
                        targetObject.rotation = Quaternion.Euler(newRotation);
                        break;
                    case Mode.AngleAxis:
                        switch (axis)
                        {
                            case Axis.Up:
                                targetObject.rotation =
                                    Quaternion.AngleAxis(targetObject.rotation.eulerAngles.y + angle, Vector3.up);
                                break;
                            case Axis.Right:
                                targetObject.rotation =
                                    Quaternion.AngleAxis(targetObject.rotation.eulerAngles.x + angle, Vector3.right);
                                break;
                            case Axis.Front:
                                targetObject.rotation =
                                    Quaternion.AngleAxis(targetObject.rotation.eulerAngles.z + angle, Vector3.forward);
                                break;
                        }

                        break;
                    default:
                        break;
                }
            }
            else
                Debug.LogError(nameof(SetRotationInteraction) + ": "
                                                              + nameof(targetObject) + " is null!");

            OnEnd();
        }
    }
}