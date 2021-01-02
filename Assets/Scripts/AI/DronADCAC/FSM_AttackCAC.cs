﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_AttackCAC : FSM_Attack
{
    [SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float idleDisplacement = 3f;

    bool hasAttacked = false;

    [SerializeField] string state;
    

    private void Awake()
    {
        evaluator = GetComponent<Evaluator>();
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
            state = "approach";
            dronBehaviour.hasRespawned = false;
            dronBehaviour.ai.isStopped = false;

            Vector3 randomDir = new Vector3(UnityEngine.Random.value, 0f, UnityEngine.Random.value).normalized;
            Vector3 dst = dronBehaviour.transform.forward * idleDisplacement + randomDir * idleDisplacement * 0.5f;

            //dronBehaviour.ai.destination = dronBehaviour.transform.position + dronBehaviour.transform.forward * 5f;
            dronBehaviour.ai.SetDestination(dronBehaviour.transform.position + dst);
        });
        approachEnemyState.SetOnUpdate(() =>
        {
            if (dronBehaviour.closestEnemy != null)
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
            state = "attack";
            dronBehaviour.ai.isStopped = true;

            hasAttacked = true;


        });

        var approachToAttack = new FSM_Edge(approachEnemyState, attackState, new List<Func<bool>>()
        {
            ()=>{
                Debug.Log("checking");
                if(dronBehaviour.closestEnemy ==  null) return false;
                Debug.Log("checking: " + ((dronBehaviour.transform.position-dronBehaviour.closestEnemy.transform.position).magnitude <= attackRange));
                return (dronBehaviour.transform.position-dronBehaviour.closestEnemy.transform.position).magnitude <= attackRange;
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
