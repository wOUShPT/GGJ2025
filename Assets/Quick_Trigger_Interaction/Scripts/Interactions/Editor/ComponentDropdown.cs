// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace AstralShift.QTI
{
    public class ComponentDropdown : AdvancedDropdown
    {
        protected Action<Type> onComponentSelected;
        protected Type componentType;
        List<Type> components = new List<Type>();
        protected string toRemoveFromName;
        protected string componentNamespace;
        protected Action getList;

        public ComponentDropdown(AdvancedDropdownState state, Type componentType,
            Action<Type> onComponentSelected, string toRemoveFromName, string componentNamespace) : base(state)
        {
            this.onComponentSelected = onComponentSelected;
            this.componentType = componentType;
            this.toRemoveFromName = toRemoveFromName;
            this.componentNamespace = componentNamespace;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(componentNamespace);

            components = new List<Type>();
            components.AddRange(InteractionsDatabase.GetComponentList(componentType));

            foreach (var component in components)
            {
                string componentName = GetComponentName(component.ToString(), toRemoveFromName);
                string componentPath = GetComponentPath(component.ToString());
                AdvancedDropdownItem newPath = null;
                if (componentPath != "")
                {
                    if (!root.children.Any(n => n.name == componentPath))
                    {
                        newPath = new AdvancedDropdownItem(componentPath);
                        root.AddChild(newPath);
                    }
                    else
                    {
                        newPath = root.children.ToList().Find(n => n.name == componentPath);
                    }
                }
                else
                {
                    newPath = root;
                }

                var newItem = new ComponentDropdownItem(componentName, component);
                newPath.AddChild(newItem);
            }

            return root;
        }

        private string GetComponentName(string scriptName, string toRemoveFromName)
        {
            int lastDotIndex = scriptName.LastIndexOf('.');

            // Extract the substring starting after the last '.'
            string afterLastDot = scriptName.Substring(lastDotIndex + 1);

            // Find the position of the next occurrence of "Interaction"
            int componentIndex = afterLastDot.IndexOf(toRemoveFromName);

            // If "Interaction" is not found, return the current string
            if (componentIndex == -1)
            {
                return afterLastDot;
            }

            // Extract the substring up to the "Component"
            string result = afterLastDot.Substring(0, componentIndex);

            return result;
        }

        private string GetComponentPath(string scriptName)
        {
            int lastDotIndex = scriptName.LastIndexOf('.');

            if (lastDotIndex == -1)
            {
                return "Misc";
            }

            // Extract the substring starting after the last '.'
            string afterLastDot = scriptName.Substring(0, lastDotIndex);

            int secondLastDotIndex = afterLastDot.LastIndexOf('.');

            if (secondLastDotIndex == -1)
            {
                return "Misc";
            }

            string afterSecondLastDot = afterLastDot.Substring(secondLastDotIndex + 1);

            if (afterSecondLastDot == componentNamespace)
                afterSecondLastDot = "";

            return afterSecondLastDot;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            if (item is ComponentDropdownItem customItem)
            {
                onComponentSelected?.Invoke(customItem.ComponentType);
            }
        }
    }
}