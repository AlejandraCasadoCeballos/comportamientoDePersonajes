using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_GoToWaitingPoint : MonoBehaviour
{
    [HideInInspector] public Evaluator evaluator;
    DronADCACBehaviour dronBehaviour;

    [HideInInspector] public FSM fsm;

    private void Awake()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponentInParent<DronADCACBehaviour>();

        CreateFSM();
    }

    private void CreateFSM()
    {
        fsm = new FSM(0.5f);
        
        //STATES
        var dieState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        dieState.SetOnBegin(() =>
        {
            TeamManager.AddDronToQueue(dronBehaviour);
        });
        var goToWaitingPointState = new FSM_Node();
        goToWaitingPointState.SetOnBegin(() =>
        {
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.hasRespawned = false;
            dronBehaviour.ai.SetDestination(dronBehaviour.recruiterWaitingPoint);
        });
        var waitOrdersState = new FSM_Node();

        //EDGES

        var dieToGoToWaitingPoint = new FSM_Edge(dieState, goToWaitingPointState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned
        });

        var goToWaitingPointToWaitOrders = new FSM_Edge(goToWaitingPointState, waitOrdersState, new List<Func<bool>>()
        {
            ()=>transform.position==dronBehaviour.recruiterWaitingPoint
        });
       

        var anyToDie = new FSM_Edge(fsm.anyState, dieState, new List<Func<bool>>(){ () => dronBehaviour.life <= 0f });
        
        fsm.anyState.AddEdge(anyToDie);

        dieState.AddEdge(dieToGoToWaitingPoint);
        goToWaitingPointState.AddEdge(goToWaitingPointToWaitOrders);

        fsm.SetNodes(new FSM_Node[]
        {
            dieState,
            goToWaitingPointState,
            waitOrdersState,
        });
        fsm.SetRoot(goToWaitingPointState);
        evaluator.SetBehaviour(fsm);
    }
}
