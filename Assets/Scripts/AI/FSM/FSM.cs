using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM : Behaviour
{
    protected List<FSM_Node> nodes;
    protected FSM_Node currentFSMNode;

    public FSM()
    {
        this.nodes = new List<FSM_Node>();
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
    }
}
