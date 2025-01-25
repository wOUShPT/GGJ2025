// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class RotateObject : MonoBehaviour
    {
        public float rotationX = 0.0f;
        public float rotationY = 20.0f;
        public float rotationZ = 0.0f;

        void Update()
        {
            transform.Rotate(new Vector3(rotationX, rotationY, rotationZ) * Time.deltaTime);
        }
    }
}