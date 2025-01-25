// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactions.Visuals
{
    public class InteractionVisual : MonoBehaviour
    {
        public string HighlightBool = "Highlight";
        public string DisableBool = "Disable";
        public string InteractTrigger = "Interact";

        public Animator animator;

        public virtual void Awake()
        {
            Idle();
        }

        public virtual void Idle()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HighlightBool, false);
        }

        public virtual void Highlight()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HighlightBool, true);
        }

        public virtual void Disable()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(DisableBool, true);
        }

        public virtual void Enable()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(DisableBool, false);
        }

        public virtual void Interact()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(InteractTrigger);
            animator.SetBool(HighlightBool, true);
        }
    }
}