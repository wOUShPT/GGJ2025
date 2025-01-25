// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers.Attributes;
using AstralShift.QTI.Interactors;
using System.Collections;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    /// <summary>
    /// Delays all onEnd Interactions by a given time (seconds)
    /// </summary>
    [AddComponentMenu("QTI/Interactions/DelayInteraction")]
    public class DelayInteraction : Interaction
    {
        [Tooltip("Wait time to execute the onEnd interactions (seconds)")]
        [SerializeField]
        protected float waitTime = 1;

        [Tooltip("Defines if wait time is <Time.timescale> dependent \n \n(Default: false)")]
        [SerializeField]
        protected bool unscaledTime;

        public bool intVector = false;
        [ConditionalHide(nameof(intVector))]
        public Vector2Int intRange;
        [ConditionalHide(nameof(intVector), false)]
        public Vector2 range;

        public float WaitTime
        {
            get { return waitTime; }
            set { waitTime = value; }
        }

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            if (intVector)
            {
                waitTime = Random.Range(intRange.x, intRange.y);

            }
            else
            {
                waitTime = Random.Range(range.x, range.y);
            }

            if (unscaledTime)
            {
                StartCoroutine(UnscaledWait(WaitTime));
            }
            else
            {
                StartCoroutine(Wait(WaitTime));
            }
        }


        public IEnumerator UnscaledWait(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            OnEnd();
        }

        public IEnumerator Wait(float time)
        {
            yield return new WaitForSeconds(time);
            OnEnd();
        }
    }
}