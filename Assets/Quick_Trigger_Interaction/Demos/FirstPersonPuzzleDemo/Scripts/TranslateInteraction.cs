// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.FPPuzzleDemo
{
    public class TranslateInteraction : Interaction, IGravityField
    {
        public Transform startTransform;
        public Transform endTransform;
        public float speed = 2;
        public int moveCount = -1;
        public bool canReverse = false;
        public bool invertOnInteract = false;
        public bool useFixedUpdate;

        public bool onEndWaitToFinish;

        private int _currentMoveCount;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private float _movementDelta;
        private float _easedLerpFactor;
        private float _velocityDelta;
        private float _sign = 1;

        public bool IsEnabled => _enabled;
        private bool _enabled = false;

        public override bool CanInteract()
        {
            return true;
        }

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            if (invertOnInteract)
            {
                if (_enabled)
                {
                    _sign = -_sign;
                }

                _enabled = true;
                _currentMoveCount = 0;
            }
            else
            {
                _enabled = !_enabled;
                _currentMoveCount = 0;
            }

            if (!onEndWaitToFinish)
            {
                OnEnd();
            }
        }

        public Vector3 GetMovementDelta()
        {
            if (_enabled)
            {
                Vector3 startPosition = startTransform.position;
                Vector3 desiredPosition = Vector3.Lerp(startPosition, endTransform.position, _easedLerpFactor);
                Debug.Log(desiredPosition - _lastPosition);
                return desiredPosition - _lastPosition;
            }

            return Vector3.zero;
        }

        public void FixedUpdate()
        {
            if (!useFixedUpdate)
            {
                return;
            }

            if (!_enabled)
            {
                return;
            }

            if (moveCount != -1 && _currentMoveCount >= moveCount)
            {
                _enabled = false;
                _currentMoveCount = 0;
                if (onEndWaitToFinish)
                {
                    OnEnd();
                }

                return;
            }

            float distance = Vector3.Distance(startTransform.position, endTransform.position);
            _movementDelta += _sign * speed * Time.fixedDeltaTime;
            float lerpFactor = _movementDelta / distance;
            _easedLerpFactor = Mathf.SmoothStep(0, 1, lerpFactor);
            _lastPosition = transform.position;
            _currentPosition = Vector3.Lerp(startTransform.position, endTransform.position, _easedLerpFactor);
            transform.position = _currentPosition;
            if (lerpFactor > 1)
            {
                lerpFactor = 1;
                if (canReverse)
                {
                    _sign = -_sign;
                }

                if (moveCount != -1)
                {
                    _currentMoveCount++;
                }
            }

            if (lerpFactor < 0)
            {
                lerpFactor = 0;
                if (canReverse)
                {
                    _sign = -_sign;
                }

                if (moveCount != -1)
                {
                    _currentMoveCount++;
                }
            }
        }

        public void Update()
        {
            if (useFixedUpdate)
            {
                return;
            }

            if (!_enabled)
            {
                return;
            }

            if (moveCount != -1 && _currentMoveCount >= moveCount)
            {
                _enabled = false;
                _currentMoveCount = 0;
                if (onEndWaitToFinish)
                {
                    OnEnd();
                }

                return;
            }

            float distance = Vector3.Distance(startTransform.position, endTransform.position);
            _movementDelta += _sign * speed * Time.smoothDeltaTime;
            float lerpFactor = _movementDelta / distance;
            _easedLerpFactor = Mathf.SmoothStep(0, 1, lerpFactor);
            _lastPosition = transform.position;
            _currentPosition = Vector3.Lerp(startTransform.position, endTransform.position, _easedLerpFactor);
            transform.position = _currentPosition;
            if (lerpFactor > 1)
            {
                lerpFactor = 1;
                if (canReverse)
                {
                    _sign = -_sign;
                }

                if (moveCount != -1)
                {
                    _currentMoveCount++;
                }
            }

            if (lerpFactor < 0)
            {
                lerpFactor = 0;
                if (canReverse)
                {
                    _sign = -_sign;
                }

                if (moveCount != -1)
                {
                    _currentMoveCount++;
                }
            }
        }
    }
}