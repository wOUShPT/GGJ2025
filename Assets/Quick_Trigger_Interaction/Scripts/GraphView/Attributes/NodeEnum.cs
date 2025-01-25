﻿// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;


namespace AstralShift.QTI.NodeEditor
{
    /// <summary> Draw enums correctly within nodes. Without it, enums show up at the wrong positions. </summary>
    /// <remarks> Enums with this attribute are not detected by EditorGui.ChangeCheck due to waiting before executing </remarks>
    public class NodeEnumAttribute : PropertyAttribute
    {
    }
}