using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_AttackCAC : FSM_Attack
{
    [SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float attackSpeed = 1f;

    bool hasAttacked = false;
    

    void Start()
    {
        dronBehaviour = GetComponentInParent<DronADCACBehaviour>();
        CreateFSM();
    }

    private void CreateFSM()
    {
        fsm = new FSM(0.5f);
        var dieState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        dieState.SetOnBegin(() =>
        {
            TeamManager.AddDronToQueue(dronBehaviour);
        });
        var anyToDie = new FSM_Edge(fsm.anyState, dieState, new List<Func<bool>>() { () => dronBehaviour.life <= 0f });
        fsm.anyState.AddEdge(anyToDie);

        var approachEnemyState = new FSM_Node();
        approachEnemyState.SetOnBegin(() =>
        {
            dronBehaviour.hasRespawned = false;
            dronBehaviour.ai.isStopped = false;
            if(dronBehaviour.closestEnemy != null)
            {
                dronBehaviour.ai.SetDestination(dronBehaviour.closestEnemy.transform.position);
            }
            
        });

        var dieToApproachEnemy = new FSM_Edge(dieState, approachEnemyState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned,
        });

        var attackState = new FSM_Node();
        attackState.SetOnBegin(() =>
        {
            dronBehaviour.ai.isStopped = true;

            hasAttacked = true;

        });

        var approachToAttack = new FSM_Edge(approachEnemyState, attackState, new List<Func<bool>>()
        {
            ()=>{
                if(dronBehaviour.closestEnemy ==  null) return false;
                return (transform.position-dronBehaviour.closestEnemy.transform.position).magnitude <= attackRange;
                }
        });
        var attackToApproach = new FSM_Edge(attackState, approachEnemyState, new List<Func<bool>>()
        {
            ()=>hasAttacked,
            ()=>(transform.position-dronBehaviour.closestEnemy.transform.position).magnitude > attackRange
        });

        dieState.AddEdge(dieToApproachEnemy);
        approachEnemyState.AddEdge(approachToAttack);
        attackState.AddEdge(attackToApproach);
        fsm.SetNodes(new FSM_Node[] { dieState, approachEnemyState, attackState });
        fsm.SetRoot(approachEnemyState);
        evaluator.SetBehaviour(fsm);
    }
}
