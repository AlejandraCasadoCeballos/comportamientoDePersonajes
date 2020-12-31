﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Node : ActionNode
{
    public List<FSM_Edge> myEdges;

    public FSM_Node(FSM_Edge[] Edges) : base()
    {
        this.myEdges = Edges != null ? new List<FSM_Edge>(Edges) : new List<FSM_Edge>(); //Si no me equivoco aquí puedo crear un nodo que necesariamente no tenga aristas
    }

    public FSM_Node(FSM_Edge oneEdge)  : base()
    {
        myEdges = new List<FSM_Edge>();
        this.myEdges.Add(oneEdge); 
    }

    public FSM_Node() : base()
    {
        myEdges = new List<FSM_Edge>();
    }
    public void AddEdge(FSM_Edge edge)
    {
        myEdges.Add(edge);
    }


}
