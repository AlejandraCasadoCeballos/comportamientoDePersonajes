using System;
using System.Collections;
using System.Collections.Generic;
using Data.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class RecruiterBehaviour : DronBehaviour
{
    
    public HashSet<DronADCACBehaviour> recruits = new HashSet<DronADCACBehaviour>();
    [SerializeField] private int maxAllies;
    [SerializeField] float waitPointRange;
    [SerializeField] float recruitRange;
    [SerializeField] int minPercentageOfAllies;

    private float waitingPoint;
    private DronBehaviour dronBehaviour;
    private BaseBehaviour closestBase;
    private Evaluator evaluator;
    private DronADCACBehaviour closestAlly;

    private void Start()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponent<DronBehaviour>();
        CreateFSM();
    }

    private void Update()
    {
        if(closestAlly) Debug.DrawLine(transform.position,dronBehaviour.ai.destination,Color.blue);
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
        approachToAllyState.SetOnBegin(()=>
        {
            dronBehaviour.hasRespawned = false;
            float minDist = 999f;
            foreach (var dron in DronADCACBehaviour.dronADCACSet)
            {
                if ((dron.transform.position - transform.position).magnitude < minDist && (dron.team == team))
                {
                    closestAlly = dron;
                }
            }
            if (closestAlly != null)
            {
                Debug.Log("mi posicion es: "+transform.position);
                Debug.Log("tengo destino: " +closestAlly.transform.position);
                
                dronBehaviour.ai.SetDestination(closestAlly.transform.position);
                Debug.Log(dronBehaviour.ai.destination);
            }
        });

        var goToEnemyBaseState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);

        var recruitAllyState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        recruitAllyState.SetOnBegin(() =>
        {
            dronBehaviour.ai.isStopped = true;
            RecruitAlly(closestAlly);
        });
        
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
            dronBehaviour.ai.SetDestination(closestBase.transform.position);
        });

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies, CheckNearBase });
        var goBaseToWaitAgentsEdge = new FSM_Edge(goToEnemyBaseState, waitRecruitAgentsState, new List<Func<bool>>() { CheckNearBase });
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

    private bool CheckAllyInRecruitRange()
    {
        return (closestAlly.transform.position-transform.position).magnitude<recruitRange;
    } 
    private bool CheckEnoughAllies()
    {
        return recruits.Count==maxAllies;
    }
    
    private bool CheckEnoughAlliesToConquer()
    {
        return recruits.Count>=(maxAllies*minPercentageOfAllies/100f);
    }

    private bool CheckNearBase()
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

        if (closestBase != null)
        {
            waitingPoint = (closestBase.transform.position - transform.position).magnitude - (TurretBehaviour.distToBase + TurretBehaviour.attackRangeStatic);
            dronBehaviour.ai.SetDestination(closestBase.transform.position);
            dronBehaviour.ai.stoppingDistance=waitingPoint;
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

    


