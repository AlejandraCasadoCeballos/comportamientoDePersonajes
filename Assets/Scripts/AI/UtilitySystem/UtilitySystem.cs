using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class UtilitySystem : MonoBehaviour 
{
    private HashSet<UtilityFunction> utilityFunctions=new HashSet<UtilityFunction>();
    public ActionNode currentNode;

    [SerializeField] private float standByReevaluationRate;
    public void SetUtilityFunctions(HashSet<UtilityFunction> utilityFunctions)
    {
        this.utilityFunctions = utilityFunctions;
        Evaluate();
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
            Debug.Log(currentNode);
            currentNode = newNode;
            Debug.Log(currentNode);
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
            Debug.Log(currentNode);
            if (currentNode != null && currentNode.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(currentNode.reevaluationRate);
                Evaluate();
            }
            else
            {
                yield return new WaitForSeconds(standByReevaluationRate);
                Debug.Log("HOLI");
            }
        }
    }

    

}
