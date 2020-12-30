using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : ParentNode
{
    public SelectorNode(Node[] children) : base(children)
    {
        
    }

    public override float Evaluate()
    {
        base.Evaluate();
        Node node;
        int count = children.Count;
        ParentNode castedNode;
        for(int i = 0; i < count; i++)
        {
            node = children[i];
            if (node.EvaluateBool())
            {
                castedNode = node as ParentNode;
                if (castedNode != null) lastNode = castedNode.lastNode;
                else lastNode = node;
                return 1f;
            }
        }
        return 0f;
    }
}
