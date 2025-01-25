// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEditor.IMGUI.Controls;

namespace AstralShift.QTI
{
    public class ComponentDropdownItem : AdvancedDropdownItem
    {
        public Type ComponentType { get; private set; }

        public ComponentDropdownItem(string name, System.Type componentType) : base(name)
        {
            ComponentType = componentType;
        }
    }
}