// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactors
{
    [AddComponentMenu("QTI/Interactor")]
    public class Interactor : MonoBehaviour, IInteractor
    {
        public Transform GetTransform()
        {
            return transform;
        }
    }
}