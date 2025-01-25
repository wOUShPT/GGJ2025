// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.FPPuzzleDemo
{
    public class PlatformInteractor : MonoBehaviour, IInteractor
    {
        public Transform GetTransform()
        {
            return transform;
        }
    }
}