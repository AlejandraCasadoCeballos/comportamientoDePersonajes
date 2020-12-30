using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : ParentNode
{
    public SequenceNode(Node[] children) : base(children)
    {

    }

    public override float Evaluate()
    {
        base.Evaluate();
        int count = children.Count;
        bool result = true;
        int i = 0;
        Node node;
        ParentNode castedNode;
        while(result && i < count)
        {
            node = children[i];
            result = children[i].EvaluateBool();
            if (result)
            {
                castedNode = node as ParentNode;
                if (castedNode != null) lastNode = castedNode.lastNode;
                else lastNode = node;
            }
            i++;
        }
        return result ? 1f : 0f;
    }
}
