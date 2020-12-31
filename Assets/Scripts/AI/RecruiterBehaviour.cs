using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class RecruiterBehaviour : DronBehaviour
{
    Evaluator evaluator;
    private void Start()
    {
        evaluator = GetComponent<Evaluator>();
        CreateFSM();
    }

    private void CreateFSM()
    {
        var fsm = new FSM();

        var dieState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        var approachToAllyState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        var goToEnemyBaseState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        var recruitAllyState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        var waitRecruitAgentsState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        var attackEnemyBaseState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies });
        var goBaseToWaitAgentsEdge = new FSM_Edge(goToEnemyBaseState, waitRecruitAgentsState, new List<Func<bool>>() { CheckNearBase });
        var waitAgentsToApproachEdge = new FSM_Edge(waitRecruitAgentsState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var waitAgentsToAttackEdge = new FSM_Edge(waitRecruitAgentsState, attackEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies });
        var attackToApproachEdge = new FSM_Edge(attackEnemyBaseState, approachToAllyState, new List<Func<bool>>() { CheckConqueredBase });
        var attackToDieEdge = new FSM_Edge(attackEnemyBaseState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });
        var waitAgentsToDieEdge = new FSM_Edge(waitRecruitAgentsState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });
        var goBaseToDieEdge = new FSM_Edge(goToEnemyBaseState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });
        var approachToDieEdge = new FSM_Edge(approachToAllyState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });
        var recruitToDieEdge = new FSM_Edge(recruitAllyState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });
        

        
        dieState.AddEdge(dieToApproachToAllyEdge);
        approachToAllyState.AddEdge(approachToRecruitEdge);
        recruitAllyState.AddEdge(recruitToApproachEdge);
        recruitAllyState.AddEdge(recruitToGoBaseEdge);
        goToEnemyBaseState.AddEdge(goBaseToWaitAgentsEdge);
        waitRecruitAgentsState.AddEdge(waitAgentsToApproachEdge);
        waitRecruitAgentsState.AddEdge(waitAgentsToAttackEdge);
        attackEnemyBaseState.AddEdge(attackToApproachEdge);
        
        //From all to die
        approachToAllyState.AddEdge(approachToDieEdge);
        recruitAllyState.AddEdge(recruitToDieEdge);
        goToEnemyBaseState.AddEdge(goBaseToDieEdge);
        waitRecruitAgentsState.AddEdge(waitAgentsToDieEdge);
        attackEnemyBaseState.AddEdge(attackToDieEdge);


        fsm.SetNodes(new FSM_Node[] { dieState, approachToAllyState, recruitAllyState, goToEnemyBaseState, waitRecruitAgentsState, attackEnemyBaseState });
        fsm.SetRoot(approachToAllyState);

        evaluator.SetBehaviour(fsm);
    }

    private bool CheckAllyInRecruitRange()
    {
        bool inRange = true;
        return inRange;
    } 
    private bool CheckEnoughAllies()
    {
        bool enoughAgents = true;
        return enoughAgents;
    } 
    private bool CheckNearBase()
    {
        bool nearBase = true;
        return nearBase;
    } 
    private bool CheckConqueredBase()
    {
        bool conquered = true;
        return conquered;
    } 
    private bool CheckHasRespawned()
    {
        bool respawned = true;
        return respawned;
    } 
    private bool CheckNoMoreLifes()
    {
        bool alive=true;
        return alive;
    } 




}

    


