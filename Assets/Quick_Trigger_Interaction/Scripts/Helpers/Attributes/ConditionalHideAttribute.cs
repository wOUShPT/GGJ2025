// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Helpers.Attributes
{
    public class ConditionalHideAttribute : PropertyAttribute
    {
        public string ConditionalSourceField;
        public bool HideIfFalse;

        public ConditionalHideAttribute(string conditionalSourceField, bool hideIfFalse = true)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideIfFalse = hideIfFalse;
        }
    }
}