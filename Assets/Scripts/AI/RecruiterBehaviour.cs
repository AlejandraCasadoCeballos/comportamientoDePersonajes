using System;
using System.Collections;
using System.Collections.Generic;
using Data.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class RecruiterBehaviour : DronBehaviour
{
    
    HashSet<DronADCACBehaviour> recruits = new HashSet<DronADCACBehaviour>();

    [SerializeField] private int maxAllies;
    [SerializeField] float waitPointRange;
    private DronBehaviour dronBehaviour;
    Evaluator evaluator;
    private void Start()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponent<DronBehaviour>();
        CreateFSM();
    }

    public void RecruitAlly(DronADCACBehaviour ally)
    {
        ally.recruiter = this;
        recruits.Add(ally);
    }

    private void CreateFSM()
    {
        var fsm = new FSM();

        var dieState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);

        var approachToAllyState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        approachToAllyState.SetOnBegin(()=> dronBehaviour.hasRespawned = false);

        var goToEnemyBaseState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        goToEnemyBaseState.SetOnBegin(() =>
        {
            float minDist=999f;
            BaseBehaviour closestBase;
            foreach (var b in BaseBehaviour.bases)
            {
                if ((b.transform.position - this.transform.position).magnitude < minDist && (b.team!=this.team))
                {
                    closestBase = b;
                }
            }
            //dronBehaviour.ai.SetDestination(closestBase.transform.position);
            //dronBehaviour.stopDistance();

        });

        var recruitAllyState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        
        var waitRecruitAgentsState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        waitRecruitAgentsState.SetOnBegin(() =>
        {
            dronBehaviour.ai.isStopped = true;
            foreach (var r in recruits) r.PushRecruiterIsWaiting();
        });
        var attackEnemyBaseState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        attackEnemyBaseState.SetOnBegin(() =>
        {
            foreach (var r in recruits) r.PushRecruiterIsConquering();
        });

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies });
        var goBaseToWaitAgentsEdge = new FSM_Edge(goToEnemyBaseState, waitRecruitAgentsState, new List<Func<bool>>() { CheckNearBase });
        var waitAgentsToApproachEdge = new FSM_Edge(waitRecruitAgentsState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var waitAgentsToAttackEdge = new FSM_Edge(waitRecruitAgentsState, attackEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies });
        var attackToApproachEdge = new FSM_Edge(attackEnemyBaseState, approachToAllyState, new List<Func<bool>>() { CheckConqueredBase });
        var anyToDie = new FSM_Edge(fsm.anyState, dieState, new List<Func<bool>>() { () => dronBehaviour.life <= 0f });

        dieState.AddEdge(dieToApproachToAllyEdge);
        approachToAllyState.AddEdge(approachToRecruitEdge);
        recruitAllyState.AddEdge(recruitToApproachEdge);
        recruitAllyState.AddEdge(recruitToGoBaseEdge);
        goToEnemyBaseState.AddEdge(goBaseToWaitAgentsEdge);
        waitRecruitAgentsState.AddEdge(waitAgentsToApproachEdge);
        waitRecruitAgentsState.AddEdge(waitAgentsToAttackEdge);
        attackEnemyBaseState.AddEdge(attackToApproachEdge);

        fsm.anyState.AddEdge(anyToDie);


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
        return dronBehaviour.hasRespawned;
    } 
    private bool CheckNoMoreLifes()
    {
        bool alive=true;
        return alive;
    } 




}

    


