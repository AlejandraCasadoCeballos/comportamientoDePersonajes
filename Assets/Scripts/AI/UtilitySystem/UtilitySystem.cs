﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class UtilitySystem : Behaviour
{
    private HashSet<UtilityFunction> utilityFunctions=new HashSet<UtilityFunction>();
    
    public void SetUtilityFunctions(HashSet<UtilityFunction> utilityFunctions)
    {
        this.utilityFunctions = utilityFunctions;
    }

    override public void Evaluate()
    {
        float max=-1.0f;
        ActionNode newNode=currentNode;
        float utility;
        foreach (var utilityFunction in utilityFunctions)
        {
            utility = utilityFunction.EvaluateUtility();
            if(utility > max)
            {
                max = utility;
                newNode = utilityFunction.node;
            }
        }
        
        if (currentNode != newNode)
        {
            EndCurrentNode();
            currentNode = newNode;
            BeginCurrentNode();
        }
    }
}
