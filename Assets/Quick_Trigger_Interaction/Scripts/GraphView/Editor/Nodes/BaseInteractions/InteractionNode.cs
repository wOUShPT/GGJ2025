// Copyright (c) AstralShift. All rights reserved.

using Interaction = AstralShift.QTI.Interactions.Interaction;

namespace AstralShift.QTI.NodeEditor
{
    [InteractionComponent(typeof(Interaction))]
    public class InteractionNode : InteractionBaseNode
    {
        [Input(ShowBackingValue.Never)] public int entry;
        [Output] public int exit;
    }
}