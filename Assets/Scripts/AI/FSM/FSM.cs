using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM : Behaviour
{
    protected List<FSM_Node> nodes;
    protected FSM_Node currentFSMNode;
    protected FSM_Node rootNode;
    public FSM_Node anyState;

    public FSM()
    {
        this.nodes = new List<FSM_Node>();
        anyState = new FSM_Node();
    }

    public FSM(float anyStateRate)
    {
        this.nodes = new List<FSM_Node>();
        anyState = new FSM_Node(anyStateRate, ActionNode.Reevaluation.atFixedRate);
    }

    public void SetNodes(FSM_Node[] nodes)
    {
        this.nodes = new List<FSM_Node>(nodes);
    }

    public void AddNode(FSM_Node Node)
    {
        nodes.Add(Node);
    }

    public void SetRoot(FSM_Node node)
    {
        currentFSMNode = node;
        currentNode = node;
        rootNode = node;
    }

    public void Restart()
    {
        currentFSMNode = rootNode;
        currentNode = rootNode;
    }

    override public void Evaluate()
    {
       if (currentFSMNode == null) return;
       foreach (var edge in currentFSMNode.edges)
       {
            if (edge.CheckConditions())
            {
                EndCurrentNode();
                currentFSMNode = edge.dstNode;
                currentNode = currentFSMNode;
                BeginCurrentNode();
            }
       }
        foreach (var edge in anyState.edges)
        {
            if (edge.CheckConditions())
            {
                EndCurrentNode();
                currentFSMNode = edge.dstNode;
                currentNode = currentFSMNode;
                BeginCurrentNode();
            }
        }
    }
}
