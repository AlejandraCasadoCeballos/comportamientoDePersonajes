using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM : MonoBehaviour
{
    protected List<FSM_Node> myNodes;
    protected FSM_Node currentNode;
    private float standardReevaluationRate;

    public FSM(FSM_Node[] Nodes)
    {
        this.myNodes = Nodes != null ? new List<FSM_Node>(Nodes) : new List<FSM_Node>();
    }

    public void AddNode(FSM_Node Node)
    {
        myNodes.Add(Node);
    }

    public void SetRoot(FSM_Node node)
    {
        currentNode = node;
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
       foreach (var edge in currentNode.myEdges)
        {
            if (edge.CheckConditions())
            {
                currentNode?.End();
                currentNode = edge.dst_node;
                currentNode.Begin();
            }
        }
    }
    IEnumerator EvaluateCoroutine()
    {
        while (true)
        {
            if(currentNode != null && currentNode.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(currentNode.reevaluationRate);
            }
            else
            {
                yield return new WaitForSeconds(standardReevaluationRate);
            }
            Evaluate();
        }
    }




}
