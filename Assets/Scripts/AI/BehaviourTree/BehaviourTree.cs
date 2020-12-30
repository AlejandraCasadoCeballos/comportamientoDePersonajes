using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    ParentNode root;
    ActionNode currentAction;

    public void SetParentNode(ParentNode root)
    {
        this.root = root;
    }

    public void Evaluate()
    {
        root.Evaluate();
        ActionNode newAction = root.lastNode as ActionNode;
        if(newAction != null && newAction != currentAction)
        {
            switch (currentAction.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentAction.onActionEnded -= Evaluate;
                    break;
                case ActionNode.Reevaluation.atFixedRate:
                    StopCoroutine("EvaluateAtFixedRate");
                    break;
            }
            currentAction?.End();
            currentAction = newAction;
            currentAction?.Begin();
            switch (currentAction.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentAction.onActionEnded += Evaluate;
                    break;
                case ActionNode.Reevaluation.atFixedRate:
                    StartCoroutine(EvaluateAtFixedRate(currentAction.reevaluationRate));
                    break;
            }
        }
    }

    IEnumerator EvaluateAtFixedRate(float rate)
    {
        while (true)
        {
            yield return new WaitForSeconds(rate);
            Evaluate();
        }
    }

    public void Update()
    {
        currentAction?.Update();
    }
}
