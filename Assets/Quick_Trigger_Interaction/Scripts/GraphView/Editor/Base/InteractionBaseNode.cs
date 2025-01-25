// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.QTI.NodeEditor
{
    public class InteractionBaseNode : Node
    {
        public Component component;
        public GameObject gameObject;

        #region Initialization

        public virtual void Initialize(Component interactionComponent)
        {
            component = interactionComponent;
            name = component.GetType().ToString();
            name = name.Replace(component.GetType().Namespace + ".", "");
            gameObject = component.gameObject;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
        }

        public override void OnRemoveConnection(NodePort port)
        {
        }

        #endregion

        #region Utility

        /// <summary>
        /// Returns true if this Node is a leaf (doesn't have children)
        /// </summary>
        /// <returns></returns>
        public bool IsLeaf()
        {
            InteractionBaseNode[] children = GetChildren();
            return children.Length == 0;
        }


        /// <summary>
        /// Returns the node connected to a given node input port (Parent Node).
        /// Returns null if it doesn't have a parent.
        /// </summary>
        /// <returns>Returns the parent node</returns>
        public InteractionBaseNode GetParent()
        {
            NodePort currentNodeInputPort = GetInputPort("entry");

            if (currentNodeInputPort == null || currentNodeInputPort.ConnectionCount == 0)
            {
                return null;
            }

            InteractionBaseNode parentNode = currentNodeInputPort.Connection.node as InteractionBaseNode;
            return parentNode;
        }

        /// <summary>
        /// Returns this node's children.
        /// Returns an empty array if it doesn't have children.
        /// </summary>
        /// <returns>Array of children</returns>
        public virtual InteractionBaseNode[] GetChildren()
        {
            List<InteractionBaseNode> nodes = new List<InteractionBaseNode>();

            NodePort nodePort = GetOutputPort("exit");

            if (nodePort == null)
            {
                return nodes.ToArray();
            }

            for (int i = 0; i < nodePort.ConnectionCount; i++)
            {
                InteractionBaseNode childNode = nodePort.GetConnection(i).node as InteractionBaseNode;
                nodes.Add(childNode);
            }

            return nodes.ToArray();
        }

        /// <summary>
        /// Returns this node top child.
        /// Returns null if it doesn't have children.
        /// </summary>
        /// <returns>Top Child</returns>
        public InteractionBaseNode GetTopMostChild()
        {
            InteractionBaseNode[] childrenNodes = GetChildren();

            if (childrenNodes.Length == 0)
            {
                return null;
            }

            return childrenNodes[0];
        }

        /// <summary>
        /// Returns this node bottom child.
        /// Returns null if it doesn't have children.
        /// </summary>
        /// <returns>Bottom child</returns>
        public InteractionBaseNode GetBottomMostChild()
        {
            InteractionBaseNode[] childrenNodes = GetChildren();

            if (childrenNodes.Length == 0)
            {
                return null;
            }

            return childrenNodes[childrenNodes.Length - 1];
        }

        /// <summary>
        /// Returns this node top sibling.
        /// </summary>
        /// <returns>Top Sibling</returns>
        public InteractionBaseNode GetTopMostSibling()
        {
            InteractionBaseNode parentNode = GetParent();

            if (parentNode == null)
            {
                return null;
            }

            if (IsTopMostSibling())
            {
                return this;
            }

            InteractionBaseNode[] siblingNodes = parentNode.GetChildren();

            if (siblingNodes.Length == 0)
            {
                return null;
            }

            return siblingNodes[0];
        }

        /// <summary>
        /// Returns true if this node is the top sibling.
        /// </summary>
        /// <returns></returns>
        public bool IsTopMostSibling()
        {
            InteractionBaseNode parent = GetParent();

            if (parent == null)
            {
                return true;
            }

            InteractionBaseNode[] siblings = parent.GetChildren();

            if (siblings.Length == 0)
            {
                return true;
            }

            return siblings[0] == this;
        }

        /// <summary>
        /// Returns this node bottom sibling.
        /// </summary>
        /// <returns>Bottom Sibling</returns>
        public InteractionBaseNode GetBottomMostSibling()
        {
            InteractionBaseNode parentNode = GetParent();

            if (parentNode == null)
            {
                return null;
            }

            if (IsTopMostSibling())
            {
                return this;
            }

            InteractionBaseNode[] siblingNodes = parentNode.GetChildren();

            if (siblingNodes.Length == 0)
            {
                return null;
            }

            return siblingNodes[^1];
        }

        /// <summary>
        /// Returns true if this node is the bottom sibling.
        /// </summary>
        /// <returns></returns>
        public bool IsBottomMostSibling()
        {
            InteractionBaseNode parent = GetParent();

            if (parent == null)
            {
                return true;
            }

            InteractionBaseNode[] siblings = parent.GetChildren();

            if (siblings.Length == 0)
            {
                return true;
            }

            return siblings[siblings.Length - 1] == this;
        }

        /// <summary>
        /// Returns this node previous sibling (next on top).
        /// </summary>
        /// <returns>Previous Sibling</returns>
        public InteractionBaseNode GetPreviousSibling()
        {
            InteractionBaseNode parentNode = GetParent();

            if (parentNode == null || IsTopMostSibling())
            {
                return null;
            }

            InteractionBaseNode[] siblingNodes = parentNode.GetChildren();

            int nodeIndex = Array.IndexOf(siblingNodes, this);
            return siblingNodes[nodeIndex--];
        }

        /// <summary>
        /// Returns this node next sibling (next to bottom).
        /// </summary>
        /// <returns>Next Sibling</returns>
        public InteractionBaseNode GetNextSibling()
        {
            InteractionBaseNode parentNode = GetParent();

            if (parentNode == null || IsBottomMostSibling())
            {
                return null;
            }

            InteractionBaseNode[] siblingNodes = parentNode.GetChildren();
            if (siblingNodes == null)
            {
                return null;
            }

            int nodeIndex = Array.IndexOf(siblingNodes, this);
            nodeIndex++;
            if (nodeIndex > siblingNodes.Length - 1)
            {
                return null;
            }

            return siblingNodes[nodeIndex];
        }

        #endregion
    }
}