// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;
using ConditionInteraction = AstralShift.QTI.Interactions.ConditionInteraction;

namespace AstralShift.QTI.NodeEditor
{
    [InteractionComponent(typeof(ConditionInteraction))]
    public class ConditionInteractionNode : InteractionNode
    {
        [Output] public int OnTrue;
        [Output] public int OnFalse;

        /// <summary>
        /// Returns this node's children.
        /// Returns an empty array if it doesn't have children.
        /// </summary>
        /// <returns>Array of children</returns>
        public override InteractionBaseNode[] GetChildren()
        {
            List<InteractionBaseNode> nodes = new List<InteractionBaseNode>();

            NodePort trueNodePort = GetOutputPort("OnTrue");
            NodePort falseNodePort = GetOutputPort("OnFalse");

            if (trueNodePort == null)
            {
                return nodes.ToArray();
            }

            for (int i = 0; i < trueNodePort.ConnectionCount; i++)
            {
                InteractionBaseNode childNode = trueNodePort.GetConnection(i).node as InteractionBaseNode;
                nodes.Add(childNode);
            }

            for (int i = 0; i < falseNodePort.ConnectionCount; i++)
            {
                InteractionBaseNode childNode = falseNodePort.GetConnection(i).node as InteractionBaseNode;
                nodes.Add(childNode);
            }

            return nodes.ToArray();
        }
    }
}