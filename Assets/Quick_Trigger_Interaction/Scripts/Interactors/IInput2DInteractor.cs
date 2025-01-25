// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Triggers.Physics2D;
using UnityEngine;

namespace AstralShift.QTI.Interactors
{
    public interface IInput2DInteractor : IInteractor
    {
        public abstract Input2DTrigger GetInteraction();

        public abstract bool TryInteract();

        public new virtual Vector3 GetFacingDirection()
        {
            return Transform.up;
        }

        /// <summary>
        /// Gets Facing Direction (transform.forward) on ZX Plane
        /// </summary>
        /// <param name="direction"></param>
        public new virtual Vector2 GetFacingDirection2D()
        {
            return Transform.up;
        }

        /// <summary>
        /// Gets Position
        /// </summary>
        /// <param name="direction"></param>
        public new virtual Vector3 GetPosition()
        {
            return Transform.position;
        }

        /// <summary>
        /// Gets Position on ZX Plane
        /// </summary>
        /// <param name="direction"></param>
        public new virtual Vector2 GetPosition2D()
        {
            return Transform.position;
        }
    }
}