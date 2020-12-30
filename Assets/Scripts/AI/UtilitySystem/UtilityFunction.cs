using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using UnityEngine;

public class UtilityFunction
{
    public PerceptionNode perception;
    private Func<float,float> onEvaluateUtility;
    public ActionNode node;

    public UtilityFunction(ActionNode node, PerceptionNode perception)
    {
        this.node = node;
        this.perception = perception;
    }

    public float EvaluateUtility()
    {
        return onEvaluateUtility.Invoke(perception.Evaluate());
    }

    private UtilityFunction OnEvaluateUtility(Func<float, float> act)
    {
        onEvaluateUtility = act;
        return this;
    }
  

}
