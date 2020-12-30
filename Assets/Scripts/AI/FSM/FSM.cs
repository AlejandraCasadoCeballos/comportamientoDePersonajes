using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM : Behaviour
{
    protected List<FSM_Node> nodes;
    protected FSM_Node currentFSMNode;

    public void SetNodes(FSM_Node[] nodes)
    {
        this.nodes = nodes != null ? new List<FSM_Node>(nodes) : new List<FSM_Node>();
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
       foreach (var edge in currentFSMNode.myEdges)
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
