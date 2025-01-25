// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform toFollow;
        public float speed = .2f;
        private Vector3 offset;
        private Vector3 currentVelocity = Vector3.zero;

        private Transform _transform;

        // Start is called before the first frame update
        void Start()
        {
            _transform = transform;
            offset = transform.position - toFollow.position;
        }

        private void FixedUpdate()
        {
            _transform.position = Vector3.SmoothDamp(_transform.position, toFollow.position + offset,
                ref currentVelocity, speed);
        }
    }
}