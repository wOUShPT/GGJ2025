// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Helpers
{
    public static class PhysicsHelper
    {
        public static bool ContainsLayer(int layer, LayerMask layerMask)
        {
            if ((layerMask & (1 << layer)) != 0)
            {
                return true;
            }

            return false;
        }
    }
}