// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Helpers.Attributes;
using AstralShift.QTI.Settings;
using AstralShift.QTI.Triggers.Physics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AstralShift.QTI.Interactors
{
    public class InteractionFinder : MonoBehaviour, IInputInteractor
    {
        protected const int MaxHits = 10;
        public bool showLayers = false;
        [ConditionalHide("showLayers", true)] public LayerMask layer;
        public float radius;
        public float forwardDirection;
        public float height = 2;

        protected Collider[] _hitResults;

        protected List<InputTrigger> _nearbyInteractions;
        protected InputTrigger _nearestInteraction;

        public int SearchFrameCount = 2;

        protected IInputInteractor iit;

        private void Awake()
        {
            _hitResults = new Collider[MaxHits];
            iit = this;
        }

        private void OnValidate()
        {
            showLayers = !InteractionsSettings.Instance.ForceInputTriggerLayer;
            layer = InteractionsSettings.Instance.AssignInputTriggerLayerMask(layer);
        }

        public bool TryInteract()
        {
            if (_nearestInteraction)
            {
                _nearestInteraction.Interact(this);
            }

            return _nearestInteraction != null;
        }

        public virtual InputTrigger GetInteraction()
        {
            int numberOfHits = GetNearbyInteractions();

            // if there are interactions 
            if (numberOfHits <= 0)
            {
                return null;
            }

            _nearbyInteractions = new List<InputTrigger>();
            for (int i = 0; i < numberOfHits; i++)
            {
                //search for first interactionTrigger instance found
                if (_hitResults[i].TryGetComponent(out InputTrigger interactionKeyTrigger))
                {
                    if (interactionKeyTrigger.CanInteract(iit.GetFacingDirection2D(), iit.GetPosition2D()))
                    {
                        _nearbyInteractions.Add(interactionKeyTrigger);
                        interactionKeyTrigger.ClosestInteractor = this;
                    }
                }
            }

            //No Interactions nearby
            if (_nearbyInteractions.Count == 0)
            {
                return null;
            }

            // Only keep interactions with maxPriority
            int maxPriority = _nearbyInteractions.Max(iKT => iKT.priority.selectedIndex);
            _nearbyInteractions.RemoveAll(i => i.priority.selectedIndex < maxPriority);

            float lowestDistance = Mathf.Infinity;
            int nearestInteractionIndex = 0;
            for (var i = 0; i < _nearbyInteractions.Count; i++)
            {
                Vector2 forwardPosition = (iit.GetFacingDirection2D() * forwardDirection + iit.GetPosition2D());
                float distance = Vector2.Distance(forwardPosition, _nearbyInteractions[i].GetPosition2D());
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    nearestInteractionIndex = i;
                }
            }

            return _nearbyInteractions[nearestInteractionIndex];
        }

        protected virtual int GetNearbyInteractions()
        {
            //// Cast an Overlap Capsule around player and cache results
            int numberOfResults = Physics.OverlapCapsuleNonAlloc(transform.position,
                transform.position + Vector3.up * height, radius, _hitResults, layer.value);
            return numberOfResults;
        }

        private void FixedUpdate()
        {
            if (Time.frameCount % SearchFrameCount == 0)
            {
                var interaction = GetInteraction();


                if (interaction is null)
                {
                    _nearestInteraction?.ResetVisuals();
                    _nearestInteraction = null;
                    return;
                }

                if (interaction != null && _nearestInteraction == null)
                {
                    _nearestInteraction = interaction;
                    _nearestInteraction.HighlightVisuals();
                    return;
                }

                if (interaction != _nearestInteraction) //different interaction caught 
                {
                    _nearestInteraction.ResetVisuals();
                    _nearestInteraction = interaction;
                    _nearestInteraction.HighlightVisuals();
                }
            }
        }

        public Transform GetTransform()
        {
            return transform;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Color stateColor = Color.red;
            stateColor.a = 0.75f;

            IInputInteractor thisInteractor = this;

            Vector3 pos = thisInteractor.GetFacingDirection() * forwardDirection + thisInteractor.GetPosition();

            Gizmos.DrawSphere(new Vector3(pos.x, pos.y + 1, pos.z), 0.25f);

            if (_nearestInteraction == null)
            {
                GizmosHelpers.DrawWireCapsule(transform.position, transform.position + Vector3.up * height, radius,
                    stateColor, 1.5f);
                return;
            }

            bool canInteract =
                _nearestInteraction.CanInteract(thisInteractor.GetFacingDirection2D(), thisInteractor.GetPosition2D());
            stateColor = canInteract ? Color.green : Color.blue;
            stateColor.a = 0.75f;

            GizmosHelpers.DrawWireCapsule(transform.position, transform.position + Vector3.up * height, radius,
                stateColor, 1.5f);
        }
#endif
    }
}