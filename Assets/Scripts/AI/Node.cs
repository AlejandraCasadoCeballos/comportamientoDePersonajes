using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{ 
    public virtual float Evaluate()
    {
        return 1;
    }

    public bool EvaluateBool()
    {
        return Evaluate() > 0.5f;
    }
}
