using System;
using System.Collections;
using System.Collections.Generic;
using Data.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;


public class RecruiterBehaviour : DronBehaviour
{
    
    public HashSet<DronADCACBehaviour> recruits = new HashSet<DronADCACBehaviour>();
    [SerializeField] private int maxAllies;
    [SerializeField] float waitPointRange;
    [SerializeField] float recruitRange;
    [SerializeField] int minPercentageOfAllies;
    [SerializeField] private int safeDistance;
    [SerializeField] float minDistToWaitingPoint = 0.5f;

    private float waitingDistance;
    private DronBehaviour dronBehaviour;
    public BaseBehaviour closestBase;
    private Evaluator evaluator;
    private DronADCACBehaviour closestAlly;

    [SerializeField] string state;

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
        approachToAllyState.SetOnBegin(() =>
        {
            state = "approach ally";
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.hasRespawned = false;
            float minDist = 999f;
            closestAlly = null;
            foreach (var dron in DronADCACBehaviour.dronADCACSet)
            {
                if ((dron.transform.position - transform.position).magnitude < minDist && (dron.team == team) && !recruits.Contains(dron))
                {
                    closestAlly = dron;
                }
            }

        });
        approachToAllyState.SetOnUpdate(() =>
        {
            if (closestAlly != null)
            {
                dronBehaviour.ai.SetDestination(closestAlly.transform.position);
            }
        });

        var goToEnemyBaseState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        goToEnemyBaseState.SetOnBegin(() =>
        {
            state = "goToEnemyBase";
            waitingDistance = (closestBase.transform.position - transform.position).magnitude - (TurretBehaviour.distToBase + TurretBehaviour.attackRangeStatic);
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.ai.SetDestination(closestBase.transform.position);
            dronBehaviour.ai.stoppingDistance = waitingDistance+safeDistance;
        });

        var recruitAllyState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        recruitAllyState.SetOnBegin(() =>
        {
            state = "recruiting";
            dronBehaviour.ai.isStopped = true;
            RecruitAlly(closestAlly);
        });
        
        var waitRecruitAgentsState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        waitRecruitAgentsState.SetOnBegin(() =>
        {
            state = "waiting";
            dronBehaviour.ai.isStopped = true;
            foreach (var r in recruits) r.PushRecruiterIsWaiting(transform.position);
        });
        var attackEnemyBaseState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        attackEnemyBaseState.SetOnBegin(() =>
        {
            state = "conquering";
            dronBehaviour.ai.stoppingDistance = 0f;
            dronBehaviour.ai.isStopped = false;
            foreach (var r in recruits) r.PushRecruiterIsConquering();
        });

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies, CheckClosestBase });
        var goBaseToWaitAgentsEdge = new FSM_Edge(goToEnemyBaseState, waitRecruitAgentsState, new List<Func<bool>>() { CheckInWaitingPoint });
        var waitAgentsToApproachEdge = new FSM_Edge(waitRecruitAgentsState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAlliesToConquer() });
        var waitAgentsToAttackEdge = new FSM_Edge(waitRecruitAgentsState, attackEnemyBaseState, new List<Func<bool>>() { CheckEnoughAlliesToConquer });
        var attackToApproachEdge = new FSM_Edge(attackEnemyBaseState, approachToAllyState, new List<Func<bool>>() { CheckConqueredBase });
        var anyToDie = new FSM_Edge(fsm.anyState, dieState, new List<Func<bool>>() { CheckNoMoreLifes });

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

    private bool CheckInWaitingPoint()
    {
        Vector3 dir = dronBehaviour.transform.position - dronBehaviour.ai.destination;
        float magnitude = dir.magnitude;
        float threshold = dronBehaviour.ai.stoppingDistance + minDistToWaitingPoint;
        bool isStopped = magnitude < threshold;
        return isStopped;
    }

    private bool CheckAllyInRecruitRange()
    {
        return closestAlly!=null && (closestAlly.transform.position-transform.position).magnitude<recruitRange;
    } 
    private bool CheckEnoughAllies()
    {
        return recruits.Count==maxAllies;
    }
    
    private bool CheckEnoughAlliesToConquer()
    {
        return recruits.Count>=(maxAllies*minPercentageOfAllies/100f);
    }

    private bool CheckClosestBase()
    {
        closestBase = null;
        float minDist = 999f;
        foreach (var b in BaseBehaviour.bases)
        {
            if ((b.transform.position - transform.position).magnitude < minDist && (b.team != team))
            {
                closestBase = b;
            }
        }

        return closestBase!=null;
    } 
    private bool CheckConqueredBase()
    {
        return closestBase.team==team;
    } 
    private bool CheckHasRespawned()
    {
        return dronBehaviour.hasRespawned;
    } 
    private bool CheckNoMoreLifes()
    {
        return life<=0;
    } 




}

    


