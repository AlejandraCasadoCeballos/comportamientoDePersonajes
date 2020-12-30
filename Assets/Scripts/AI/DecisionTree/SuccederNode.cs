using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccederNode : ParentNode
{
    private ParentNode childAsParentNode;
    private Node child;

    public SuccederNode(Node child)
    {
        AddChild(child);
        childAsParentNode = child as ParentNode;
        this.child = child;
    }

    public override float Evaluate()
    {
        base.Evaluate();
        child.Evaluate();
        if (childAsParentNode != null) lastNode = childAsParentNode.lastNode;
        else lastNode = child;
        return 1f;
    }
}
