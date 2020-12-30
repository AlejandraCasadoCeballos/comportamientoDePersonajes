using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentNode : Node
{
    protected List<Node> children;

    public ParentNode(Node[] children = null)
    {
        this.children = children != null ? new List<Node>(children) : new List<Node>();
    }

    public void AddChild(Node child)
    {
        children.Add(child);
    }
}