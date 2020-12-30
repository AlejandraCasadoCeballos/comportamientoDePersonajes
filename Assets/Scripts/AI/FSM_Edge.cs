using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_Edge : MonoBehaviour
{
    protected FSM_Node src_node;
    public FSM_Node dst_node;
    protected List<Func<bool>> conditions; 

   public FSM_Edge(FSM_Node nodeA, FSM_Node nodeB, List<Func<bool>> nodeCondition)
   {
        this.src_node = nodeA;
        this.dst_node = nodeB;
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
