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
    private Evaluator evaluator;
    
    [SerializeField] private float safeDistance = 4f;

    private float waitingTimer=0f;

    [Header("Display")]
    [SerializeField] bool pathComplete;
    [SerializeField] private float remainingDistance;
    [SerializeField] private DronADCACBehaviour closestAlly;
    public BaseBehaviour closestBase;

    [SerializeField] string state;
    FSM fsm;



    private void Start()
    {
        evaluator = GetComponent<Evaluator>();
        CreateFSM();

    }

    private void Update()
    {
        pathComplete = ai.pathPending;
        remainingDistance = ai.remainingDistance;
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
            if(life <= 0)
                TeamManager.AddRecruiterToQueue(this);
        });

        var approachToAllyState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        approachToAllyState.SetOnBegin(() =>
        {
            state = "approach ally";
            ai.isStopped = false;
            hasRespawned = false;
            closestAlly = null;
        });
        approachToAllyState.SetOnUpdate(() =>
        {
            ai.stoppingDistance = 0f;
            if (closestAlly != null)
            {
                ai.isStopped = false;
                ai.SetDestination(closestAlly.transform.position);
                Debug.DrawLine(transform.position, closestAlly.transform.position, Color.blue);
            }
        });

        var goToEnemyBaseState = new FSM_Node(0.15f, ActionNode.Reevaluation.atFixedRate);
        goToEnemyBaseState.SetOnBegin(() =>
        {
            state = "goToEnemyBase";
            waitingDistance = TurretBehaviour.distToBase + TurretBehaviour.attackRangeStatic;
            ai.isStopped = false;
            ai.stoppingDistance = waitingDistance + safeDistance;
            ai.SetDestination(closestBase.transform.position);
        });

        goToEnemyBaseState.SetOnUpdate(() =>
        {
            //ai.isStopped = false;
            ai.stoppingDistance = waitingDistance + safeDistance;
            Debug.DrawLine(transform.position, closestBase.transform.position+Vector3.up*1.5f, Color.blue);
        });

        var recruitAllyState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        recruitAllyState.SetOnBegin(() =>
        {
            state = "recruiting";
            ai.isStopped = true;
            RecruitAlly(closestAlly);
        });
        
        var waitRecruitAgentsState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        waitRecruitAgentsState.SetOnBegin(() =>
        {
            waitingTimer = 0f;
            state = "waiting";
            ai.isStopped = true;
            //ai.stoppingDistance = 0;
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
            ai.stoppingDistance = 0f;
            ai.isStopped = false;
            try
            {
                foreach (var r in recruits) r.PushRecruiterIsConquering();
            } catch(System.Exception e)
            {

            }
            
        });
        attackEnemyBaseState.SetOnEnd(() =>
        {
            recruits.Clear();
        });

        var dieToApproachToAllyEdge = new FSM_Edge(dieState, approachToAllyState, new List<Func<bool>>() { CheckHasRespawned});
        var approachToRecruitEdge = new FSM_Edge(approachToAllyState, recruitAllyState, new List<Func<bool>>() { CheckAllyInRecruitRange });
        var recruitToApproachEdge = new FSM_Edge(recruitAllyState, approachToAllyState, new List<Func<bool>>() { ()=>!CheckEnoughAllies() });
        var recruitToGoBaseEdge = new FSM_Edge(recruitAllyState, goToEnemyBaseState, new List<Func<bool>>() { CheckEnoughAllies, ()=>CheckClosestBase() });
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
        BaseBehaviour lastBase = closestBase;
        CheckClosestBase(true);
        if (lastBase != closestBase)
        {
            ai.SetDestination(closestBase.transform.position);
        }

        float magnitude = ai.pathPending ? (closestBase.transform.position - transform.position).magnitude : ai.remainingDistance;
        bool isStopped = magnitude < waitingDistance + safeDistance;
        ai.isStopped = isStopped;
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

    private bool CheckClosestBase(bool considerSelfCloser = false)
    {
        var self = closestBase;
        closestBase = null;
        float minDist = 999f;
        float dist;
        foreach (var b in BaseBehaviour.bases)
        {
            dist = (transform.position - b.transform.position).magnitude;
            if (b == self && considerSelfCloser)
            {
                dist *= 0.8f;
            }
            if (dist < minDist && (b.team != team))
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
        return hasRespawned;
    } 
    private bool CheckNoMoreLifes()
    {
        return life<=0;
    }

}
