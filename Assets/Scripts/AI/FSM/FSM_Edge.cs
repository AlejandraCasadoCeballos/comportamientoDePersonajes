using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_Edge
{
    protected FSM_Node src_node;
    public FSM_Node dstNode;
    protected List<Func<bool>> conditions; 

   public FSM_Edge(FSM_Node nodeOrigin, FSM_Node nodeDestiny, List<Func<bool>> nodeCondition)
   {
        this.src_node = nodeOrigin;
        this.dstNode = nodeDestiny;
        this.conditions = nodeCondition;
   }

   public void AddCondition(Func<bool> condition)
   {
        conditions.Add(condition);
   }
    
    public bool CheckConditions()
    {
        foreach (var condition in conditions)
        {
            if (!condition())
            {
                return false;
            }
        }
        return true;
    }
}
