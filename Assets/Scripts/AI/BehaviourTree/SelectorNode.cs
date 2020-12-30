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
        Node node;
        int count = children.Count;
        for(int i = 0; i < count; i++)
        {
            node = children[i];
            if (node.Evaluate())
            {
                lastNode = node;
                
                return true;
            }
        }
        return false;
    }
}
