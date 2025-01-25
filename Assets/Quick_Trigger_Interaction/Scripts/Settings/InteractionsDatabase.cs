// Copyright (c) AstralShift. All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Interaction = AstralShift.QTI.Interactions.Interaction;
using InteractionTrigger = AstralShift.QTI.Triggers.InteractionTrigger;

namespace AstralShift.QTI.Settings
{
    public class InteractionsDatabase : AssetPostprocessor
    {
        private static List<Type> interactions;
        private static List<Type> triggers;
        private static string folderPath = "Assets/"; // Replace with your folder path
        private static bool updateInteractions = false;
        private static bool updateTriggers = false;
        private static bool logComponents = false;

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Check if any of the imported assets are new scripts containing "Interaction"
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".cs"))
                {
                    string fileContent = System.IO.File.ReadAllText(assetPath);
                    if (fileContent.Contains("class"))
                    {
                        if (fileContent.Contains("Interaction"))
                        {
                            // Log the file that triggered the process
                            //Debug.Log($"Detected new Interaction class: {assetPath}");

                            updateInteractions = true;
                            break;
                        }
                        else if (fileContent.Contains("Trigger"))
                        {
                            // Log the file that triggered the process
                            //Debug.Log($"Detected new Trigger class: {assetPath}");

                            updateTriggers = true;
                            break;
                        }
                    }
                }
            }

            // Check if any of the imported assets are new scripts containing "Interaction"
            foreach (string assetPath in deletedAssets)
            {
                if (assetPath.EndsWith(".cs"))
                {
                    // Log the file that triggered the process
                    //Debug.Log($"Deleted Script: {assetPath}");

                    updateInteractions = true;
                    break;
                }
            }

            if (updateInteractions)
            {
                interactions = FindComponentsInFolder(typeof(Interaction));
            }

            if (updateTriggers)
            {
                triggers = FindComponentsInFolder(typeof(InteractionTrigger));
            }
        }

        public static List<Type> GetComponentList(Type componentType)
        {
            if (componentType == typeof(InteractionTrigger))
            {
                if (triggers == null || triggers.Count == 0)
                {
                    triggers = FindComponentsInFolder(componentType);
                }

                return triggers;
            }
            else if (componentType == typeof(Interaction))
            {
                if (interactions == null || interactions.Count == 0)
                {
                    interactions = FindComponentsInFolder(componentType);
                }

                return interactions;
            }
            else return FindComponentsInFolder(componentType);
        }

        private static List<Type> FindComponentsInFolder(Type componentType)
        {
            List<Type> components = new List<Type>();
            // Folder to search (relative to the Assets folder)

            // Find all script assets in the specified folder
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { folderPath });

            // List to store the MonoBehaviour types

            foreach (string guid in guids)
            {
                // Get the path to the asset
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // Load the script asset
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                // Get the class type of the script
                Type scriptType = monoScript.GetClass();

                // Check if the class is a MonoBehaviour and is not abstract
                if (scriptType != null && scriptType.IsSubclassOf(componentType) && !scriptType.IsAbstract)
                {
                    components.Add(scriptType);
                }
            }

            if (logComponents)
            {
                // Log the found MonoBehaviour types
                Debug.Log("Found " + componentType.ToString());
                foreach (Type component in components)
                {
                    Debug.Log(component.FullName);
                }
            }

            return components;
        }
    }
}
#endif