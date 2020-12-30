using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PerceptionNode : Node
{
    Func<bool> evaluationFunc;

    public PerceptionNode(Func<bool> func)
    {
        evaluationFunc = func;
    }

    override public bool Evaluate()
    {
        return evaluationFunc != null ? evaluationFunc() : true;
    }
}
