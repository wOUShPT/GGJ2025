// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.FPPuzzleDemo
{
    public interface IGravityField
    {
        public virtual Vector3 GetMovementDelta()
        {
            return Vector3.zero;
        }
    }
}