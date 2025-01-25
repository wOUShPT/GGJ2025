// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AstralShift.QTI.Triggers.Physics2D;

namespace AstralShift.QTI.Triggers.Physics
{
    [CustomEditor(typeof(PhysicsTrigger), true), CanEditMultipleObjects]
    public class PhysicsTriggerEditor : InteractionTriggerEditor
    {
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            CheckIfColliderAttached();
            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUIHeader();
            DrawTagSelector();
            DrawProperties();
            DrawFooter();
        }

        protected virtual void TryDestroyPhysics2DTrigger()
        {
            PhysicsTrigger currentTrigger = target as PhysicsTrigger;
            if (currentTrigger.TryGetComponent(out Physics2DTrigger physics2DTrigger))
            {
                DestroyImmediate(physics2DTrigger);
                if (currentTrigger.TryGetComponent(out Collider2D collider2D))
                {
                    DestroyImmediate(collider2D);
                }
            }
        }

        protected virtual void CheckIfColliderAttached()
        {
            if (!(target as PhysicsTrigger).TryGetComponent(out Collider collider))
            {
                ShowColliderSelectionPopup((target as PhysicsTrigger));
                TryDestroyPhysics2DTrigger();
            }
        }

        protected virtual void DrawTagSelector()
        {
            PhysicsTrigger physicsTrigger = (PhysicsTrigger)target;
            if (physicsTrigger.useTag)
            {
                // Get all tags
                string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
                // Get the index of the current tag selected
                int currentIndex = System.Array.IndexOf(tags, physicsTrigger.targetTag);

                // Show a dropdown for the tags
                int selectedTagIndex = EditorGUILayout.Popup("Tag", currentIndex, tags);
                selectedTagIndex = Mathf.Clamp(selectedTagIndex, 0, tags.Length);

                // Set the selected tag
                physicsTrigger.targetTag = tags[selectedTagIndex];

                // Save any changes to the target object
                EditorUtility.SetDirty(target);
            }
        }

        private void ShowColliderSelectionPopup(MonoBehaviour script)
        {
            int option = EditorUtility.DisplayDialogComplex("Select Collider Type",
                "Current Trigger requires a Collider.\n" +
                "Please select the type of Collider to add:",
                "BoxCollider",
                "CapsuleCollider",
                "SphereCollider");

            Collider newCollider;
            System.Type newColliderType;

            switch (option)
            {
                case 0:

                    newColliderType = typeof(BoxCollider);
                    break;

                case 1:

                    newColliderType = typeof(CapsuleCollider);
                    break;

                case 2:

                    newColliderType = typeof(SphereCollider);
                    break;

                default:

                    newColliderType = typeof(BoxCollider);
                    break;
            }

            if (script.TryGetComponent(out Collider2D collider))
            {
                Undo.DestroyObjectImmediate(collider);
            }

            newCollider = Undo.AddComponent(script.gameObject, newColliderType) as Collider;


            var components = script.GetComponents<Component>();
            int targetIndex = Array.IndexOf(components, script);

            (script as PhysicsTrigger).RefreshCollider();

            if (targetIndex == -1)
            {
                Debug.LogWarning("Component not found on the target GameObject.");
                return;
            }

            for (int i = components.Length - 1; i > targetIndex; i--)
            {
                ComponentUtility.MoveComponentUp(newCollider);
            }

            ;
        }
    }
}