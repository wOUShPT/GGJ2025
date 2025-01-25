// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactors
{
    public interface IInteractor
    {
        public Transform Transform
        {
            get { return GetTransform(); }
        }

        public abstract Transform GetTransform();

        /// <summary>
        /// Gets Facing Direction (transform.forward)
        /// </summary>
        /// <param name="direction"></param>
        public virtual Vector3 GetFacingDirection()
        {
            return Transform.forward;
        }

        /// <summary>
        /// Gets Facing Direction (transform.forward) on ZX Plane
        /// </summary>
        /// <param name="direction"></param>
        public virtual Vector2 GetFacingDirection2D()
        {
            Vector3 direction = Vector3.ProjectOnPlane(Transform.forward, Vector3.up).normalized;
            return new Vector2(direction.x, direction.z);
        }

        /// <summary>
        /// Gets Position
        /// </summary>
        /// <param name="direction"></param>
        public virtual Vector3 GetPosition()
        {
            return Transform.position;
        }

        /// <summary>
        /// Gets Position on ZX Plane
        /// </summary>
        /// <param name="direction"></param>
        public virtual Vector2 GetPosition2D()
        {
            return new Vector2(Transform.position.x, Transform.position.z);
        }
    }
}