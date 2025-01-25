// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers;
using AstralShift.QTI.Triggers;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AstralShift.QTI.Interactions
{
    public static class EditorUtils
    {
        public static class Generic
        {
            /// <summary>
            /// Create highlighter identifier
            /// </summary>
            public static void SetHighlighterIdentifier(UnityEngine.Object target)
            {
                EditorGUILayout.Space(0);
                Rect componentRect = GUILayoutUtility.GetLastRect();
                componentRect.position = new Vector2(componentRect.position.x,
                    componentRect.position.y - EditorGUIUtility.singleLineHeight - 5);
                componentRect.size = new Vector2(componentRect.size.x - 10, EditorGUIUtility.singleLineHeight);
                Highlighter.HighlightIdentifier(componentRect, target.GetInstanceID().ToString());
            }
        }

        public static class Interactions
        {
            public static void AddInteraction(Interaction currentInteraction, Type newComponentType)
            {
                Interaction newInteraction =
                    Undo.AddComponent(currentInteraction.gameObject, newComponentType) as Interaction;

                currentInteraction.onEndInteractions.Add(newInteraction);
            }

            public static void AddInteractionInBetween(Interaction currentInteraction, Type newComponentType)
            {
                // Get all components on the GameObject
                var components = currentInteraction.gameObject.GetComponents<Component>();
                int targetIndex = Array.IndexOf(components, currentInteraction);

                if (targetIndex == -1)
                {
                    Debug.LogWarning("Component not found on the target GameObject.");
                    return;
                }

                // Add the new component
                Interaction newInteraction =
                    Undo.AddComponent(currentInteraction.gameObject, newComponentType) as Interaction;

                for (int i = components.Length - 1; i > targetIndex; i--)
                {
                    ComponentUtility.MoveComponentUp(newInteraction);
                }

                ;

                // Set onEndInteractions to newInteraction
                foreach (var interaction in currentInteraction.onEndInteractions)
                {
                    newInteraction.onEndInteractions.Add(interaction);
                }

                // Clear onEndInteractions and replace with newInteraction
                currentInteraction.onEndInteractions.Clear();
                currentInteraction.onEndInteractions.Add(newInteraction);
            }

            public static void RemoveInteraction(Interaction currentInteraction)
            {
                if (currentInteraction.onEndInteractions.Count > 0)
                {
                    // Register the current state of the list before removing an item
                    Undo.RecordObject(currentInteraction, "Remove Item From List");

                    var onEndInteraction = currentInteraction.onEndInteractions[^1];
                    currentInteraction.onEndInteractions.Remove(onEndInteraction);

                    // Mark the object as dirty so Unity knows the object has been modified
                    EditorUtility.SetDirty(currentInteraction);
                    Undo.DestroyObjectImmediate(onEndInteraction);
                }
                else
                {
                    Undo.DestroyObjectImmediate(currentInteraction);
                }
            }

            public static void ReplaceInteraction(Interaction currentInteraction, Type newComponentType)
            {
                // Get all components on the GameObject
                var components = currentInteraction.GetComponents<Component>();
                int targetIndex = Array.IndexOf(components, currentInteraction);

                if (targetIndex == -1)
                {
                    Debug.LogWarning("Component not found on the target GameObject.");
                    return;
                }

                // Add the new component
                Interaction newInteraction =
                    Undo.AddComponent(currentInteraction.gameObject, newComponentType) as Interaction;

                for (int i = components.Length - 1; i > targetIndex; i--)
                {
                    ComponentUtility.MoveComponentUp(newInteraction);
                }

                ;

                EditorHelpers.CopyAllInCommonProperties(currentInteraction, newInteraction);

                Interaction[] allInteractions =
                    GameObjectHelpers.GetAllComponentsOfTypeInScene<Interaction>(currentInteraction.gameObject, true);
                InteractionTrigger[] allInteractionTriggers =
                    GameObjectHelpers.GetAllComponentsOfTypeInScene<InteractionTrigger>(currentInteraction.gameObject);
                TraceInteraction(currentInteraction, newInteraction, allInteractions);


                foreach (InteractionTrigger trigger in allInteractionTriggers)
                {
                    if (trigger.interaction != null &&
                        trigger.interaction.GetInstanceID() == currentInteraction.GetInstanceID())
                    {
                        trigger.interaction = newInteraction;
                    }
                }

                // Log the replacement
                Debug.Log($"Replaced component at index {targetIndex} with component: {newComponentType.Name}");

                // Remove current
                Undo.DestroyObjectImmediate(currentInteraction);
            }

            /// <summary>
            /// Finds recursively the first interaction in chain
            /// </summary>
            /// <param name="referenceInteraction"></param>
            public static void TraceInteraction(Interaction referenceInteraction, Interaction newInteraction,
                Interaction[] interactions)
            {
                if (referenceInteraction == null)
                {
                    return;
                }

                for (int i = 0; i < interactions.Length; i++)
                {
                    if (interactions[i] == null || interactions[i] is not Interaction interaction)
                    {
                        continue;
                    }

                    if (interaction.onEndInteractions == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < interaction.onEndInteractions.Count; j++)
                    {
                        if (interaction.onEndInteractions[j] == null)
                        {
                            continue;
                        }

                        if (interaction.onEndInteractions[j].GetInstanceID() == referenceInteraction.GetInstanceID())
                        {
                            interaction.onEndInteractions.Remove(referenceInteraction);
                            interaction.onEndInteractions.Add(newInteraction);
                        }
                    }
                }
            }
        }

        public static class Triggers
        {
            public static void AddInteraction(InteractionTrigger targetTrigger, Type newComponentType)
            {
                Interaction newInteraction =
                    Undo.AddComponent(targetTrigger.gameObject, newComponentType) as Interaction;

                if (targetTrigger.interaction == null)
                {
                    targetTrigger.interaction = newInteraction;
                }
            }

            public static void AddInteractionInBetween(InteractionTrigger targetTrigger, Type newComponentType)
            {
                var components = targetTrigger.gameObject.GetComponents<Component>();
                int targetIndex = Array.IndexOf(components, targetTrigger);

                if (targetIndex == -1)
                {
                    Debug.LogWarning("Component not found on the target GameObject.");
                    return;
                }

                Interaction newInteraction =
                    Undo.AddComponent(targetTrigger.gameObject, newComponentType) as Interaction;

                for (int i = components.Length - 1; i > targetIndex; i--)
                {
                    ComponentUtility.MoveComponentUp(newInteraction);
                }

                ;

                if (targetTrigger.interaction)
                    newInteraction.onEndInteractions.Add(targetTrigger.interaction);

                targetTrigger.interaction = newInteraction;
            }

            public static void RemoveInteraction(InteractionTrigger targetTrigger)
            {
                if (targetTrigger.interaction)
                {
                    Undo.DestroyObjectImmediate(targetTrigger.interaction);
                    targetTrigger.interaction = null;
                }
                else
                {
                    targetTrigger.TryGetComponent(out Interaction toRemove);
                    if (toRemove != null)
                    {
                        Undo.DestroyObjectImmediate(toRemove);
                    }
                    else
                    {
                        Undo.DestroyObjectImmediate(targetTrigger);
                    }
                }
            }

            public static void RemoveAllInteractions(InteractionTrigger targetTrigger)
            {
                var toRemove = targetTrigger.GetComponents<Interaction>();
                foreach (var component in toRemove)
                {
                    Undo.DestroyObjectImmediate(component);
                }

                Undo.DestroyObjectImmediate(targetTrigger);
            }

            public static void ReplaceTrigger(InteractionTrigger targetTrigger, Type newComponentType)
            {
                var components = targetTrigger.gameObject.GetComponents<Component>();
                int targetIndex = Array.IndexOf(components, targetTrigger);

                if (targetIndex == -1)
                {
                    Debug.LogWarning("Component not found on the target GameObject.");
                    return;
                }

                InteractionTrigger newTrigger =
                    Undo.AddComponent(targetTrigger.gameObject, newComponentType) as InteractionTrigger;

                for (int i = components.Length - 1; i > targetIndex; i--)
                {
                    ComponentUtility.MoveComponentUp(newTrigger);
                }

                newTrigger.interaction = targetTrigger.interaction;

                if (targetTrigger != null)
                {
                    EditorHelpers.CopyAllInCommonProperties(targetTrigger, newTrigger);

                    Undo.DestroyObjectImmediate(targetTrigger);
                }
            }
        }
    }
}