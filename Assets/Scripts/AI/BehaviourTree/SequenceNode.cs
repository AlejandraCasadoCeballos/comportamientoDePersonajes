using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : ParentNode
{
    public SequenceNode(Node[] children) : base(children)
    {

    }

    public override bool Evaluate()
    {
        int count = children.Count;
        bool result = true;
        int i = 0;
        while(result && i < count)
        {
            result = children[i].Evaluate();
            i++;
        }
        return result;
    }
}
