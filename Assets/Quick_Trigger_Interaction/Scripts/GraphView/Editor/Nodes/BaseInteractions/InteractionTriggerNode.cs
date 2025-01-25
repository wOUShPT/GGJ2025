// Copyright (c) AstralShift. All rights reserved.

using InteractionTrigger = AstralShift.QTI.Triggers.InteractionTrigger;

namespace AstralShift.QTI.NodeEditor
{
    [InteractionComponent(typeof(InteractionTrigger))]
    public class InteractionTriggerNode : InteractionBaseNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override)]
        public int exit;
    }
}