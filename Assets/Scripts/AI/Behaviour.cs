using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour
{
    public ActionNode currentNode;
    virtual public void Evaluate()
    {
        
    }

    public void EndCurrentNode()
    {
        if (currentNode != null)
        {
            if (!currentNode.hasEnded)
            {
                currentNode.End();
            }
            
            switch (currentNode.reevaluationMode)
            {
                case ActionNode.Reevaluation.onlyOnEnd:
                    currentNode.onActionEnded -= Evaluate;
                    break;
            }
        }
    }

    public void BeginCurrentNode()
    {
        switch (currentNode.reevaluationMode)
        {
            case ActionNode.Reevaluation.onlyOnEnd:
                currentNode.onActionEnded += Evaluate;
                break;
        }
        currentNode?.Begin();
    }
}
