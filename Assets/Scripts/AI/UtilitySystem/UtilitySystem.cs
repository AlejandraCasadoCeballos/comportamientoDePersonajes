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

    [SerializeField] private float standByReevaluationRate;
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
                max = utilityFunction.EvaluateUtility();
                newNode = utilityFunction.node;
            }
        }

        if (currentNode != newNode)
        {
            
            if (currentNode != null)
            {
                switch (currentNode.reevaluationMode)
                {
                    case ActionNode.Reevaluation.onlyOnEnd: currentNode.onActionEnded -= Evaluate;
                        break;
                }
                currentNode.End();
            }
            currentNode = newNode;
            currentNode.Begin();
            switch (currentNode.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentNode.onActionEnded += Evaluate;
                    break;
            }
        }
        
    }

    IEnumerator EvaluateCoroutine()
    {
        while (true)
        {
            if (currentNode != null && currentNode.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(currentNode.reevaluationRate);
                Evaluate();
            }
            else
            {
                yield return new WaitForSeconds(standByReevaluationRate);
            }
        }
    }

    

}
