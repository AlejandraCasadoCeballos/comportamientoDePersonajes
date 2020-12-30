using UnityEngine;

public class DecisionTree : Behaviour
{
    ParentNode root;

    public void SetParentNode(ParentNode root)
    {
        this.root = root;
    }

    override public void Evaluate()
    {
        Debug.Log("Reevaluating tree");
        root.Evaluate();
        ActionNode newAction = root.lastNode as ActionNode;
        if(newAction != null && newAction != currentNode)
        {
            EndCurrentNode();
            currentNode = newAction;
            BeginCurrentNode();
        }
    }
}
