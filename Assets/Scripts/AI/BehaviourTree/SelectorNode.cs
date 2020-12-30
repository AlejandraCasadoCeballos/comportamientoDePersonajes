using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : ParentNode
{
    public SelectorNode(Node[] children) : base(children)
    {
        
    }

    public override bool Evaluate()
    {
        int count = children.Count;
        Node node;
        for(int i = 0; i < count; i++)
        {
            node = children[i];
            if (node.Evaluate())
            {
                return true;
            }
        }
        return false;
    }
}
