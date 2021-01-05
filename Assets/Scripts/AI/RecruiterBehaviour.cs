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
    [SerializeField] private float timeLimit;

    private float waitingDistance;
    private DronBehaviour dronBehaviour;
    private Evaluator evaluator;
    private DronADCACBehaviour closestAlly;
    [SerializeField] private float safeDistance = 4f;

    private float waitingTimer=0f;

    [Header("Display")]
    public BaseBehaviour closestBase;
    [SerializeField] string state;
    FSM fsm;

    private void Start()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponent<DronBehaviour>();
        CreateFSM();

    }

    private void OnEnable()
    {
        fsm?.Restart();
    }


    public void RecruitAlly(DronADCACBehaviour ally)
    {
        ally.recruiter = this;
        recruits.Add(ally);
    }

    private void CreateFSM()
    {
        fsm = new FSM(0.1f);

        var dieState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        dieState.SetOnBegin(() =>
        {
            recruits.Clear();
            if(dronBehaviour.life <= 0)
                TeamManager.AddRecruiterToQueue(this);
        });

        var approachToAllyState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        approachToAllyState.SetOnBegin(() =>
        {
            state = "approach ally";
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.hasRespawned = false;
            closestAlly = null;
            

        });
        approachToAllyState.SetOnUpdate(() =>
        {
            if (closestAlly != null)
            {
                dronBehaviour.ai.isStopped = false;
                dronBehaviour.ai.SetDestination(closestAlly.transform.position);
                Debug.DrawLine(transform.position, closestAlly.transform.position, Color.blue);
            }
        });

        var goToEnemyBaseState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        goToEnemyBaseState.SetOnBegin(() =>
        {
            state = "goToEnemyBase";
            waitingDistance = TurretBehaviour.distToBase + TurretBehaviour.attackRangeStatic;
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.ai.stoppingDistance = waitingDistance + safeDistance;
            dronBehaviour.ai.SetDestination(closestBase.transform.position);
        });

        goToEnemyBaseState.SetOnUpdate(() =>
        {
            dronBehaviour.ai.isStopped = false;
            dronBehaviour.ai.stoppingDistance = waitingDistance + safeDistance;
        });

        var recruitAllyState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        recruitAllyState.SetOnBegin(() =>
        {
            state = "recruiting";
            dronBehaviour.ai.isStopped = true;
            RecruitAlly(closestAlly);
        });
        
        var waitRecruitAgentsState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        waitRecruitAgentsState.SetOnBegin(() =>
        {
            waitingTimer = 0f;
            state = "waiting";
            dronBehaviour.ai.isStopped = true;
            dronBehaviour.ai.stoppingDistance = 0;
            try
            {
                foreach (var r in recruits) r.PushRecruiterIsWaiting(transform.position);
            } catch(System.Exception e)
            {

            }
            
        });
        waitRecruitAgentsState.SetOnEnd(() =>
        {
            foreach (var r in recruits) r.recruiterIsWaiting=false;
        });
        waitRecruitAgentsState.SetOnUpdate(() => { waitingTimer += Time.deltaTime; });
        var attackEnemyBaseState = new FSM_Node(1f, ActionNode.Reevaluation.atFixedRate);
        attackEnemyBaseState.SetOnBegin(() =>
        {
            state = "conquering";
            dronBehaviour.ai.stoppingDistance = 0f;
            dronBehaviour.ai.isStopped = false;
            foreach (var r in recruits) r.PushRecruiterIsConquering();
        });
        attackEnemyBaseState.SetOnEnd(() =>
        {
            recruits.Clear();
        });

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies, CheckClosestBase });
        var goBaseToWaitAgentsEdge = new FSM_Edge(goToEnemyBaseState, waitRecruitAgentsState, new List<Func<bool>>() { CheckInWaitingPoint });
        var waitAgentsToApproachEdge = new FSM_Edge(waitRecruitAgentsState, approachToAllyState, new List<Func<bool>>() { ()=> waitingTimer > timeLimit });
        var waitAgentsToAttackEdge = new FSM_Edge(waitRecruitAgentsState, attackEnemyBaseState, new List<Func<bool>>() { CheckEnoughAlliesToConquer });
        var attackToApproachEdge = new FSM_Edge(attackEnemyBaseState, approachToAllyState, new List<Func<bool>>() { CheckConqueredBase , () => recruits.Count==0});
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
        float magnitude = dronBehaviour.ai.remainingDistance;
        bool isStopped = magnitude < dronBehaviour.ai.stoppingDistance;
        dronBehaviour.ai.isStopped = isStopped;
        return isStopped;
    }

    private bool CheckAllyInRecruitRange()
    {
        float minDist = 999f;
        foreach (var dron in DronADCACBehaviour.dronADCACSet)
        {
            if ((dron.transform.position - transform.position).magnitude < minDist && (dron.team == team) && !recruits.Contains(dron))
            {
                minDist = (dron.transform.position - transform.position).magnitude;
                closestAlly = dron;
            }
        }
        return closestAlly!=null && (closestAlly.transform.position-transform.position).magnitude<recruitRange;
    } 
    private bool CheckEnoughAllies()
    {
        return recruits.Count==maxAllies;
    }
    
    private bool CheckEnoughAlliesToConquer()
    {
        int numRecruitsInPosition=0;
        foreach (var dron in recruits)
        {
            if ((dron.transform.position - transform.position).magnitude < waitPointRange)
                numRecruitsInPosition++;

        }
        return numRecruitsInPosition>=(maxAllies*minPercentageOfAllies/100f);
    }

    private bool CheckClosestBase()
    {
        closestBase = null;
        float minDist = 999f;
        foreach (var b in BaseBehaviour.bases)
        {
            if ((b.transform.position - transform.position).magnitude < minDist && (b.team != team))
            {
                minDist = (b.transform.position - transform.position).magnitude;
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
