using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;

public class DronADCACBehaviour : DronBehaviour
{
    [SerializeField] float enemyDetectionRange = 5f;
    [SerializeField] float protectionRange = 10f;
    [SerializeField] float attackDamage = 3f;
    
    [SerializeField] float recruiterInfluence = 0.5f;

    UtilitySystem utilitySystem;
    Evaluator evaluator;
    SphereCollider attackRangeTrigger;
    [HideInInspector] public NavMeshAgent ai;

    public RecruiterBehaviour recruiter = null;
    private bool recruiterIsWaiting = false;
    private bool recruiterIsConquering = false;

    private HashSet<DronBehaviour> enemiesInRange = new HashSet<DronBehaviour>();

    FSM_GoToWaitingPoint fsm_GoToWaitingPoint;
    FSM_Attack fsm_Attack;
    
    public DronBehaviour closestEnemy;
    BaseBehaviour targetBase;

    private void Start()
    {
        fsm_GoToWaitingPoint = GetComponentInChildren<FSM_GoToWaitingPoint>();
        fsm_Attack = GetComponentInChildren<FSM_Attack>();

        attackRangeTrigger = GetComponent<SphereCollider>();
        attackRangeTrigger.radius = enemyDetectionRange;

        ai = GetComponent<NavMeshAgent>();
        ai.speed = movementSpeed;

        CreateUtilitySystem();
    }

    public void PushRecruiterIsWaiting()
    {
        recruiterIsWaiting = true;
        recruiterIsConquering = false;
        evaluator.Evaluate();
    }

    public void PushRecruiterIsConquering()
    {
        recruiterIsWaiting = false;
        recruiterIsConquering = true;
        evaluator.Evaluate();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if (behaviour != null && behaviour.team != team)
        {
            enemiesInRange.Add(behaviour);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if (behaviour != null && behaviour.team != team)
        {
            enemiesInRange.Add(behaviour);
        }
    }

    private void CreateUtilitySystem()
    {
        PerceptionNode enemyDistance = new PerceptionNode(() => DistanceToClosestEnemy());
        PerceptionNode waitingRecruiter = new PerceptionNode(() => CheckRecruiterWaiting());
        PerceptionNode conquestSignal = new PerceptionNode(() => CheckConquestSignal());
        PerceptionNode allyBaseIsBeingConquered = new PerceptionNode(() => CheckAllyBaseIsBeingConquered());

        ActionNode attackEnemy = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => AttackEnemy());

        ActionNode goWaitingPoint = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => GoWaitingPoint());

        ActionNode conquerEnemyBase = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => ConquerEnemyBase());

        ActionNode defendAllyBase = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => DefendAllyBase());

        UtilityFunction attackEnemyUtility = new UtilityFunction(attackEnemy, enemyDistance).SetOnEvaluateUtility(
            (float per1) => (1f-per1));

        UtilityFunction goWaitingPointUtility = new UtilityFunction(goWaitingPoint, waitingRecruiter).SetOnEvaluateUtility(
            (float per2) => per2);

        UtilityFunction conquerEnemyBaseUtility = new UtilityFunction(conquerEnemyBase, conquestSignal).SetOnEvaluateUtility(
            (float per3) => per3);

        UtilityFunction defendAllyBaseUtility = new UtilityFunction(defendAllyBase, allyBaseIsBeingConquered).SetOnEvaluateUtility(
            (float per4) => 1f-per4);

        utilitySystem = new UtilitySystem();
        utilitySystem.SetUtilityFunctions(new HashSet<UtilityFunction>() { attackEnemyUtility, goWaitingPointUtility, conquerEnemyBaseUtility, defendAllyBaseUtility });
        evaluator = GetComponent<Evaluator>();
        evaluator.SetBehaviour(utilitySystem);
    }

    private float DistanceToClosestEnemy()
    {
        closestEnemy = null;
        float minDist = enemyDetectionRange;
        float dist;
        foreach(var d in enemiesInRange)
        {
            dist = (transform.position - d.transform.position).magnitude;
            if(dist <= minDist)
            {
                minDist = dist;
                closestEnemy = d;
            }
        }
        return minDist / enemyDetectionRange;
    }
    private float CheckRecruiterWaiting()
    {
        return recruiter != null && recruiterIsWaiting ? 1.0f : 0.0f;
    }

    private float CheckConquestSignal()
    {
        return recruiter != null && recruiterIsConquering ? 1.0f : 0.0f;
    }
    private float CheckAllyBaseIsBeingConquered()
    {
        float minDist = protectionRange;
        float dist;
        targetBase = null;

        foreach(var b in BaseBehaviour.bases)
        {
            if(b.team == team || !b.belongsToATeam)
            {
                if(b.isBeingNeutralized || b.isBeingConquered)
                {
                    dist = (b.transform.position - transform.position).magnitude;
                    if(dist <= minDist)
                    {
                        targetBase = b;
                        minDist = dist;
                    }
                }
            }
        }

        return minDist / protectionRange;
    }

    private void AttackEnemy()
    {
        //FSM de ataque
        fsm_Attack.evaluator.paused = false;
        fsm_GoToWaitingPoint.evaluator.paused = true;

        fsm_Attack.fsm.Restart();
    }
    private void GoWaitingPoint()
    {
        //FSM de ir al punto de espera del reclutador
        fsm_Attack.evaluator.paused = true;
        fsm_GoToWaitingPoint.evaluator.paused = false;

        fsm_GoToWaitingPoint.fsm.Restart();
    }
    private void ConquerEnemyBase()
    {
        //FSM conquistar base
    }
    private void DefendAllyBase()
    {
        //FSM defender base 
    }
}
