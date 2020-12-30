using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using UnityEngine;

public class UtilityFunctions
{
    private Action onPerception;
    private Func<float,float> onEdge;
    private Action onNodeAction;

    public void PerceptionFunc()
    {
        onPerception.Invoke();
    }
    public void EdgeFunc()
    {
        onEdge?.Invoke();
    }
    public void NodeActionFunc()
    {
        onNodeAction?.Invoke();
    }

    public UtilityFunctions OnPerception(Action act)
    {
        onPerception = act;
        return this;
    }
    public UtilityFunctions OnEdge(Func<float, float> act)
    {
        onEdge = act;
        return this;
    }
    public UtilityFunctions OnNodeAction(Action act)
    {
        onNodeAction = act;
        return this;
    }

}
