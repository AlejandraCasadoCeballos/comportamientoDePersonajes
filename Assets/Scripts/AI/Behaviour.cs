using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour
{
    public ActionNode currentNode;
    virtual public void Evaluate()
    {

    }

    protected void EndCurrentNode()
    {
        if (currentNode != null)
        {
            switch (currentNode.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentNode.onActionEnded -= Evaluate;
                    break;
            }
            currentNode.End();
        }
    }

    protected void BeginCurrentNode()
    {
        currentNode?.Begin();
        switch (currentNode.reevaluationMode)
        {
            case ActionNode.Reevaluation.onlyOnEnd:
                currentNode.onActionEnded += Evaluate;
                break;
        }
    }
}
