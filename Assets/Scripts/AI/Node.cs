﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{ 
    public virtual float Evaluate()
    {
        return 1;
    }

    public virtual bool EvaluateBool()
    {
        return Evaluate() > 0.5f;
    }
}