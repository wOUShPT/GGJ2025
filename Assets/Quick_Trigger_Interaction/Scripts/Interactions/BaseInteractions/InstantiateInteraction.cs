// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    [AddComponentMenu("QTI/Interactions/InstantiateInteraction")]
    public class InstantiateInteraction : Interaction
    {
        public List<SpawnOptions> toInstantiate;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);
            for (int i = 0; i < toInstantiate.Count; i++)
            {
                //Check position and instantiate
                SpawnOptions options = toInstantiate[i];

                GameObject go = null;

                switch (options.transformMode)
                {
                    case InstantiateInteractionTransformMode.Original:

                        Transform prefabTransform = options.ToSpawn.transform;
                        if (options.parentMode == InstantiateInteractionParentMode.Transform)
                        {
                            go = Instantiate(options.ToSpawn, prefabTransform.position, prefabTransform.rotation,
                                options.spawnParent);
                        }
                        else
                        {
                            go = Instantiate(options.ToSpawn, prefabTransform.position, prefabTransform.rotation);
                        }

                        break;

                    case InstantiateInteractionTransformMode.Transform:

                        Transform t = options.spawnTransform;
                        if (options.parentMode == InstantiateInteractionParentMode.Transform)
                        {
                            go = Instantiate(options.ToSpawn, t.position + options.spawnPosition,
                                Quaternion.Euler(t.localRotation.eulerAngles + options.spawnRotation),
                                options.spawnParent);
                        }
                        else
                        {
                            go = Instantiate(options.ToSpawn, t.position, t.rotation);
                        }

                        go.transform.localScale = new Vector3(options.spawnScale.x * t.localScale.x,
                            options.spawnScale.y * t.localScale.y, options.spawnScale.z * t.localScale.z);
                        break;

                    case InstantiateInteractionTransformMode.Manual:

                        if (options.parentMode == InstantiateInteractionParentMode.Transform)
                        {
                            go = Instantiate(options.ToSpawn, options.spawnPosition,
                                Quaternion.Euler(options.spawnRotation), options.spawnParent);
                        }
                        else
                        {
                            go = Instantiate(options.ToSpawn, options.spawnPosition,
                                Quaternion.Euler(options.spawnRotation));
                        }

                        go.transform.localScale = options.spawnScale;
                        break;
                }

                go.name = options.ToSpawn.name;
            }

            OnEnd();
        }

        [Serializable]
        public class SpawnOptions
        {
            public GameObject ToSpawn;
            public InstantiateInteractionTransformMode transformMode;

            public Vector3 spawnPosition;
            public Vector3 spawnRotation;
            public Vector3 spawnScale;
            public Transform spawnTransform;
            public InstantiateInteractionParentMode parentMode;
            public Transform spawnParent;

            public SpawnOptions()
            {
                Reset();
            }

            public void Reset()
            {
                ToSpawn = null;
                transformMode = InstantiateInteractionTransformMode.Original;
                spawnPosition = default;
                spawnRotation = default;
                spawnScale = Vector3.one;
                spawnTransform = null;
                parentMode = InstantiateInteractionParentMode.Root;
                spawnParent = null;
            }
        }

        public enum InstantiateInteractionTransformMode
        {
            Original,
            Transform,
            Manual
        }

        public enum InstantiateInteractionParentMode
        {
            Root,
            Transform
        }
    }
}