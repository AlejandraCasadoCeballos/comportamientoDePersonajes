using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    ParentNode root;
    ActionNode currentAction;
    [SerializeField] float standByReevaluationRate = 1f;

    public void SetParentNode(ParentNode root)
    {
        this.root = root;
        Evaluate();
        StopAllCoroutines();
        StartCoroutine(EvaluateAtFixedRate());
    }

    public void Evaluate()
    {
        Debug.Log("Reevaluating tree");
        root.Evaluate();
        ActionNode newAction = root.lastNode as ActionNode;
        if(newAction != null && newAction != currentAction)
        {
            if(currentAction != null)
            {
                switch (currentAction.reevaluationMode)
                {
                    case ActionNode.Reevaluation.onlyOnEnd:
                        currentAction.onActionEnded -= Evaluate;
                        break;
                }
                currentAction.End();
            }
            currentAction = newAction;
            currentAction?.Begin();
            switch (currentAction.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentAction.onActionEnded += Evaluate;
                    break;
            }
        }
    }

    IEnumerator EvaluateAtFixedRate()
    {
        while (true)
        {
            if (currentAction != null && currentAction.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(currentAction.reevaluationRate);
            }
            else yield return new WaitForSeconds(standByReevaluationRate);
            Evaluate();
        }
    }

    public void Update()
    {
        currentAction?.Update();
    }
}
