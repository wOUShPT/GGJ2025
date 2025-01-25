// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Triggers
{
    public class KeyTrigger : InteractionTrigger
    {
        public IInteractor interactor;

        public enum InputType
        {
            Key,
            Axes
        }

        public enum PressType
        {
            Press,
            Release,
            Hold
        }

        public InputType inputType;
        public PressType pressType;

        public KeyCode keyCode;

        public string inputAxes;

        public void Update()
        {
            switch (inputType)
            {
                case InputType.Key:
                    switch (pressType)
                    {
                        case PressType.Press:
                            if (Input.GetKeyDown(keyCode))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        case PressType.Release:
                            if (Input.GetKeyUp(keyCode))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        case PressType.Hold:
                            if (Input.GetKey(keyCode))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        default:
                            break;
                    }

                    break;
                case InputType.Axes:
                    switch (pressType)
                    {
                        case PressType.Press:
                            if (Input.GetButtonDown(inputAxes))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        case PressType.Release:
                            if (Input.GetButtonUp(inputAxes))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        case PressType.Hold:
                            if (Input.GetButton(inputAxes))
                            {
                                base.Interact(interactor);
                            }

                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }
        }
    }
}