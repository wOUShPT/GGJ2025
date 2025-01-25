// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AstralShift.QTI.Triggers.Physics;

namespace AstralShift.QTI.Triggers.Physics2D
{
    [CustomEditor(typeof(Physics2DTrigger), true), CanEditMultipleObjects]
    public class Physics2DTriggerEditor : InteractionTriggerEditor
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
        
        protected virtual void TryDestroyPhysicsTrigger()
        {
            Physics2DTrigger currentTrigger = target as Physics2DTrigger;
            if (currentTrigger.TryGetComponent(out PhysicsTrigger physicsTrigger))
            {
                DestroyImmediate(physicsTrigger);
                if (currentTrigger.TryGetComponent(out Collider collider))
                {
                    DestroyImmediate(collider);
                }
            }
        }
        

        protected virtual void CheckIfColliderAttached()
        {
            if (!(target as Physics2DTrigger).TryGetComponent(out Collider2D collider))
            {
                ShowColliderSelectionPopup((target as Physics2DTrigger));
                TryDestroyPhysicsTrigger();
            }
        }

        protected virtual void DrawTagSelector()
        {
            Physics2DTrigger physics2DTrigger = (Physics2DTrigger)target;
            if (physics2DTrigger.useTag)
            {
                // Get all tags
                string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
                // Get the index of the current tag selected
                int currentIndex = System.Array.IndexOf(tags, physics2DTrigger.targetTag);

                // Show a dropdown for the tags
                int selectedTagIndex = EditorGUILayout.Popup("Tag", currentIndex, tags);
                selectedTagIndex = Mathf.Clamp(selectedTagIndex, 0, tags.Length);

                // Set the selected tag
                physics2DTrigger.targetTag = tags[selectedTagIndex];

                // Save any changes to the target object
                EditorUtility.SetDirty(target);
            }
        }

        private void ShowColliderSelectionPopup(MonoBehaviour script)
        {
            int option = EditorUtility.DisplayDialogComplex("Select Collider2D Type",
                "Current Trigger requires a Collider2D.\n" +
                "Please select the type of Collider2D to add:",
                "BoxCollider2D",
                "CapsuleCollider2D",
                "CircleCollider2D");

            Collider2D newCollider;
            System.Type newColliderType;

            switch (option)
            {
                case 0:

                    newColliderType = typeof(BoxCollider2D);
                    break;

                case 1:

                    newColliderType = typeof(CapsuleCollider2D);
                    break;

                case 2:

                    newColliderType = typeof(CircleCollider2D);
                    break;

                default:

                    newColliderType = typeof(BoxCollider2D);
                    break;
            }

            if (script.TryGetComponent(out Collider collider))
            {
                Undo.DestroyObjectImmediate(collider);
            }

            newCollider = Undo.AddComponent(script.gameObject, newColliderType) as Collider2D;


            var components = script.GetComponents<Component>();
            int targetIndex = Array.IndexOf(components, script);

            (script as Physics2DTrigger).RefreshCollider();

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