// Copyright (c) AstralShift. All rights reserved.

using System;
using UnityEngine;

namespace AstralShift.QTI.NodeEditor
{
    [CustomNodeGraphEditor(typeof(InteractionsNodeGraph))]
    public class InteractionsNodeGraphEditor : NodeGraphEditor
    {
        public override NodeEditorPreferences.Settings GetDefaultPreferences()
        {
            NodeEditorPreferences.Settings newSettings = base.GetDefaultPreferences();
            return newSettings;
        }

        public override Color GetPortBackgroundColor(NodePort port)
        {
            return Color.grey;
        }

        public override Color GetTypeColor(Type type)
        {
            return Color.white;
        }
    }
}