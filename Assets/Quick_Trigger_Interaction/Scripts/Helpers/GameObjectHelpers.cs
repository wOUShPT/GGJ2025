// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.Helpers
{
    public static class GameObjectHelpers
    {
        /// <summary>
        /// Finds all components of a given type in a given GameObject's scene
        /// </summary>
        /// <param name="firstReference"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetAllComponentsOfTypeInScene<T>(GameObject firstReference, bool includeInactive = false)
            where T : Component
        {
            List<GameObject> sceneRootGameObjects = new List<GameObject>();
            sceneRootGameObjects.AddRange(firstReference.gameObject.scene.GetRootGameObjects());

            List<T> components = new List<T>();
            foreach (var sceneRootGameObject in sceneRootGameObjects)
            {
                components.AddRange(sceneRootGameObject.GetComponentsInChildren<T>(includeInactive));
            }

            return components.ToArray();
        }
    }
}