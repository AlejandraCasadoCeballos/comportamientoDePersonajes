using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Node : ActionNode
{
    public List<FSM_Edge> edges;

    public FSM_Node(float rate, ActionNode.Reevaluation reev) : base(rate, reev)
    {
        edges = new List<FSM_Edge>();
    }

    public FSM_Node(FSM_Edge[] edges, float rate, ActionNode.Reevaluation reev) : base(rate, reev)
    {
        this.edges = new List<FSM_Edge>(edges);
    }

    public FSM_Node(FSM_Edge[] edges) : base()
    {
        this.edges =new List<FSM_Edge>(edges); //Si no me equivoco aquí puedo crear un nodo que necesariamente no tenga aristas
    }

    public FSM_Node(FSM_Edge oneEdge)  : base()
    {
        edges = new List<FSM_Edge>();
        this.edges.Add(oneEdge); 
    }

    public FSM_Node() : base()
    {
        edges = new List<FSM_Edge>();
    }
    public void AddEdge(FSM_Edge edge)
    {
        edges.Add(edge);
    }


}
