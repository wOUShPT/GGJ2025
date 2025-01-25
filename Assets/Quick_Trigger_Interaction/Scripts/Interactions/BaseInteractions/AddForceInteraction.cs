// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Linq;
using AstralShift.QTI.Helpers;
using AstralShift.QTI.Interactors;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace AstralShift.QTI.Interactions
{
    [AddComponentMenu("QTI/Interactions/AddForceInteraction")]
    public class AddForceInteraction : Interaction
    {
        public enum Mode
        {
            _3D = 0,
            _2D = 1
        }

        public Mode mode;

        public enum ForceType
        {
            world = 0,
            local = 1,
            relative = 2
        }

        public ForceType forceType;

        [Tooltip("Rigidbody to add force to.")]
        public Rigidbody body;

        public Vector2 orientation;
        public ForceMode forceMode = ForceMode.Force;

        [Tooltip("Rigidbody2D to add force to.")]
        public Rigidbody2D body2D;

        public float angle;
        public ForceMode2D forceMode2D = ForceMode2D.Force;
        public float magnitude = 1;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            switch (mode)
            {
                case Mode._3D:

                    Vector3 direction = Quaternion.Euler(orientation) * Vector3.forward;
                    direction.Normalize();
                    if (body)
                    {
                        switch (forceType)
                        {
                            case ForceType.world:

                                body.AddForce(direction * magnitude, forceMode);
                                break;

                            case ForceType.local:

                                body.AddRelativeForce(direction * magnitude, forceMode);
                                break;

                            case ForceType.relative:

                                direction = body.transform.position - transform.position;
                                direction.Normalize();
                                body.AddForce(direction * magnitude, forceMode);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(nameof(AddForceInteraction) + ": no Rigidbody assigned!");
                    }

                    break;

                case Mode._2D:

                    Vector2 direction2D = Quaternion.Euler(new Vector3(0, 0, angle)) * Vector3.right;
                    direction2D.Normalize();
                    if (body2D)
                    {
                        switch (forceType)
                        {
                            case ForceType.world:

                                body2D.AddForce(direction2D * magnitude, forceMode2D);
                                break;

                            case ForceType.local:

                                body2D.AddRelativeForce(direction2D * magnitude, forceMode2D);
                                break;

                            case ForceType.relative:

                                direction = body2D.transform.position - transform.position;
                                direction.Normalize();
                                body2D.AddForce(direction * magnitude, forceMode2D);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(nameof(AddForceInteraction) + ": no Rigidbody2D assigned!");
                    }

                    break;
            }

            OnEnd();
        }

        private bool GetStartPoint(out Vector3 startPoint)
        {
            switch (forceType)
            {
                case ForceType.world:

                    if (mode == Mode._3D)
                    {
                        startPoint = transform.position;
                        return true;
                    }

                    startPoint = transform.position;
                    return true;

                case ForceType.local:

                    if (mode == Mode._3D)
                    {
                        if (!body)
                        {
                            startPoint = new Vector3(0, 0, 0);
                            return false;
                        }

                        startPoint = body.transform.position;
                        return true;
                    }

                    if (!body2D)
                    {
                        startPoint = new Vector3(0, 0, 0);
                        return false;
                    }

                    startPoint = body2D.transform.position;
                    return true;


                case ForceType.relative:

                    if (mode == Mode._3D)
                    {
                        if (!body)
                        {
                            startPoint = new Vector3(0, 0, 0);
                            return false;
                        }

                        startPoint = body.transform.position;
                        return true;
                    }

                    if (!body2D)
                    {
                        startPoint = new Vector3(0, 0, 0);
                        return false;
                    }

                    startPoint = body2D.transform.position;
                    return true;
            }

            startPoint = new Vector3(0, 0, 0);
            return false;
        }

        private Vector3 GetDirection()
        {
            Vector3 direction;
            switch (forceType)
            {
                case ForceType.world:

                    if (mode == Mode._3D)
                    {
                        direction = Quaternion.Euler(orientation) * Vector3.forward;
                        direction.Normalize();
                        return direction;
                    }

                    direction = Quaternion.Euler(new Vector3(0, 0, angle)) * Vector3.right;
                    direction.Normalize();
                    return direction;

                case ForceType.local:

                    if (mode == Mode._3D)
                    {
                        direction = Quaternion.Euler(orientation) * Vector3.forward;
                        direction.Normalize();
                        return direction;
                    }

                    direction = Quaternion.Euler(new Vector3(0, 0, angle)) * Vector3.right;
                    direction.Normalize();
                    return direction;

                case ForceType.relative:

                    if (mode == Mode._3D)
                    {
                        if (!body)
                        {
                            return Vector3.zero;
                        }

                        direction = body.transform.position - transform.position;
                        direction.Normalize();
                        return direction;
                    }

                    if (!body2D)
                    {
                        return Vector3.zero;
                    }

                    direction = body2D.transform.position - transform.position;
                    direction.Normalize();
                    return direction;
            }

            direction = Vector3.zero;
            return direction;
        }

#if UNITY_EDITOR

        public float padding = 1000f; // Padding from the edge of the screen

        protected virtual void OnDrawGizmosSelected()
        {
            using (new Handles.DrawingScope(Color.magenta))
            {
                if (!GetStartPoint(out Vector3 startPoint))
                {
                    return;
                }

                Vector3 direction = GetDirection();

                Vector3 endPoint = startPoint + direction * magnitude;
                string labelText = magnitude + " units";

                Handles.DrawLine(startPoint, endPoint, 3);
                Handles.ConeHandleCap(0, endPoint, Quaternion.LookRotation(direction),
                    HandleUtility.GetHandleSize(startPoint) * 0.25f, EventType.Repaint);
                GizmosHelpers.DrawTextBox(labelText, 14, FontStyle.Normal, startPoint + direction * magnitude / 2,
                    Color.white, new Color(0, 0, 0, 0.4f));
            }
        }

        protected virtual void OnGUI()
        {
            if (!Selection.gameObjects.Contains(gameObject) || !Handles.ShouldRenderGizmos())
            {
                return;
            }

            using (new Handles.DrawingScope(Color.magenta))
            {
                if (!GetStartPoint(out Vector3 startPoint))
                {
                    return;
                }

                Vector3 direction = GetDirection();
                string labelText = magnitude + " units";

                GizmosHelpers.DrawTextBoxGUI(labelText, (int)(14 * GizmosHelpers.GetResolutionMultiplier()),
                    FontStyle.Normal, startPoint + direction * magnitude / 2, Color.white, new Color(0, 0, 0, 0.4f));
            }
        }

#endif
    }
}