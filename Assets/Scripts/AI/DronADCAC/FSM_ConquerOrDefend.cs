using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_ConquerOrDefend : FSM_Attack
{
    [SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float idleDisplacement = 3f;
    [SerializeField] float aimSpeed = 3f;

    float timer;
    bool hasAttacked = false;
    bool hasEnteredBase = false;

    [SerializeField] string state;

    Animator anim;

    private void Awake()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponentInParent<DronADCACBehaviour>();
        CreateFSM();
        anim = GetComponentInParent<Animator>();
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

        var approachBaseState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        approachBaseState.SetOnBegin(() =>
        {
            state = "approachBase";
            dronBehaviour.hasRespawned = false;
            dronBehaviour.ai.isStopped = false;
            hasEnteredBase = false;

            if(dronBehaviour.targetBase != null)
            {
                dronBehaviour.ai.SetDestination(dronBehaviour.targetBase.transform.position);
            }
        });

        var dieToApproachEnemy = new FSM_Edge(dieState, approachBaseState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned,
        });
        dieState.AddEdge(dieToApproachEnemy);

        
        fsm.SetNodes(new FSM_Node[] { dieState, approachBaseState });
        fsm.SetRoot(approachBaseState);
        evaluator.SetBehaviour(fsm);
    }
}
