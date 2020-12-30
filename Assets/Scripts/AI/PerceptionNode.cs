using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PerceptionNode : Node
{
    Func<float> evaluationFunc;

    public PerceptionNode(Func<float> func)
    {
        evaluationFunc = func;
    }

    public override float Evaluate()
    {

        return evaluationFunc != null ? evaluationFunc() : 1;
    }
}
