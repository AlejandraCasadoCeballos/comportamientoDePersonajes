using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class UtilitySystem : MonoBehaviour 
{
    private HashSet<UtilityFunction> utilityFunctions;
    public ActionNode currentNode;
    public UtilitySystem(HashSet<UtilityFunction> utilityFunctions)
    {
        this.utilityFunctions = utilityFunctions;
    }

    public void Start()
    {
        StartCoroutine(EvaluateCoroutine());
    }

    public void Update()
    {
        currentNode?.Update();
    }

    public void Evaluate()
    {
        float max=-1.0f;
        ActionNode newNode=currentNode;
        foreach (var utilityFunction in utilityFunctions)
        {
            if (max < utilityFunction.perception.Evaluate())
            {
                max = utilityFunction.EdgeFunc();
                newNode = utilityFunction.node;
            }
        }

        if (currentNode != newNode)
        {
            currentNode.End();
            newNode.Begin();
            currentNode = newNode;
        }
        
    }

    IEnumerator EvaluateCoroutine()
    {
        yield return new WaitForSeconds(1);
        Evaluate();
    }

    

}
