using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;

public class DronADCACBehaviour : DronBehaviour
{
    [SerializeField] float enemyDetectionRange = 5f;
    [SerializeField] float protectionRange = 10f;
    [SerializeField] public float attackDamage = 3f;
    
    [SerializeField] float recruiterInfluence = 0.5f;

    UtilitySystem utilitySystem;
    Evaluator evaluator;
    SphereCollider attackRangeTrigger;
    

    public RecruiterBehaviour recruiter = null;
    public bool recruiterIsWaiting = false;
    public bool recruiterIsConquering = false;

    private HashSet<DronBehaviour> enemiesInRange = new HashSet<DronBehaviour>();
    public static HashSet<DronADCACBehaviour> dronADCACSet=new HashSet<DronADCACBehaviour>();

    FSM_GoToWaitingPoint fsm_GoToWaitingPoint;
    FSM_Attack fsm_Attack;
    FSM_ConquerOrDefend fsm_ConquerOrDefend;
    
    public DronBehaviour closestEnemy;
    public BaseBehaviour targetBase;
    public Vector3 recruiterWaitingPoint;
    BaseBehaviour closestAllyBase;
    public BaseBehaviour baseToConquer;

    [Header("UTILITY DISPLAY")]
    [SerializeField] float attackUtility = 0f;
    [SerializeField] float goToWaitingPointUtility = 0f;
    [SerializeField] float conquerUtility = 0f;
    [SerializeField] float defendUtility = 0f;


    private void Start()
    {
        fsm_GoToWaitingPoint = GetComponentInChildren<FSM_GoToWaitingPoint>();
        fsm_Attack = GetComponentInChildren<FSM_Attack>();
        fsm_ConquerOrDefend = GetComponentInChildren<FSM_ConquerOrDefend>();

        attackRangeTrigger = GetComponent<SphereCollider>();
        attackRangeTrigger.radius = enemyDetectionRange;

        CreateUtilitySystem();
    }

    private void OnEnable()
    {
        dronADCACSet.Add(this);
    }

    private void OnDisable()
    {
        recruiter?.recruits.Remove(this);
        dronADCACSet.Remove(this);
    }

    public void PushRecruiterIsWaiting(Vector3 waitingPos)
    {
        recruiterWaitingPoint = waitingPos;
        recruiterIsWaiting = true;
        recruiterIsConquering = false;
        evaluator.Evaluate();
    }

    public void PushRecruiterIsConquering()
    {
        baseToConquer = recruiter.closestBase;
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

    private void Update()
    {
        enemiesInRange.RemoveWhere((a) => (!a.gameObject.activeSelf || a.life <= 0));
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
            () => ConquerEnemyBase()).SetOnEnd(() => recruiter.recruits.Remove(this));

        ActionNode defendAllyBase = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => DefendAllyBase());

        UtilityFunction attackEnemyUtility = new UtilityFunction(attackEnemy, enemyDistance).SetOnEvaluateUtility(
            (float per1) => {
                float u = (1f - per1) * 0.9f + 0.1f;
                attackUtility = u;
                return u;
                });

        UtilityFunction goWaitingPointUtility = new UtilityFunction(goWaitingPoint, waitingRecruiter).SetOnEvaluateUtility(
            (float per2) => {
                float u = per2;
                goToWaitingPointUtility = u;
                return u;
                });

        UtilityFunction conquerEnemyBaseUtility = new UtilityFunction(conquerEnemyBase, conquestSignal).SetOnEvaluateUtility(
            (float per3) => {
                float u = per3;
                conquerUtility = u;
                return u;
                });

        UtilityFunction defendAllyBaseUtility = new UtilityFunction(defendAllyBase, allyBaseIsBeingConquered).SetOnEvaluateUtility(
            (float per4) => {
                float u = 1f - per4;
                defendUtility = u;
                return u;
                });

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
        Vector3 dir;
        RaycastHit hit;
        foreach(var d in enemiesInRange)
        {
            dir = (transform.position - d.transform.position);
            dist = dir.magnitude;
            
            if(dist <= minDist)
            {
                if (Physics.Raycast(transform.position, -dir, out hit, protectionRange))
                {
                    if (hit.transform.gameObject == d.gameObject)
                    {
                        minDist = dist;
                        closestEnemy = d;
                    }
                }
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
        closestAllyBase = null;

        foreach(var b in BaseBehaviour.bases)
        {
            if(b.team == team || !b.belongsToATeam)
            {
                if(b.isBeingNeutralized || b.isBeingConquered)
                {
                    dist = (b.transform.position - transform.position).magnitude;
                    if(dist <= minDist)
                    {
                        closestAllyBase = b;
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
        fsm_ConquerOrDefend.evaluator.paused = true;

        fsm_Attack.fsm.Restart();
    }
    private void GoWaitingPoint()
    {
        //FSM de ir al punto de espera del reclutador
        fsm_Attack.evaluator.paused = true;
        fsm_GoToWaitingPoint.evaluator.paused = false;
        fsm_ConquerOrDefend.evaluator.paused = true;

        fsm_GoToWaitingPoint.fsm.Restart();
    }
    private void ConquerEnemyBase()
    {
        targetBase = baseToConquer;
        fsm_ConquerOrDefend.evaluator.paused = false;
        fsm_Attack.evaluator.paused = true;
        fsm_GoToWaitingPoint.evaluator.paused = true;

        fsm_ConquerOrDefend.fsm.Restart();
        //FSM conquistar base
    }
    private void DefendAllyBase()
    {
        targetBase = closestAllyBase;
        fsm_ConquerOrDefend.evaluator.paused = false;
        fsm_Attack.evaluator.paused = true;
        fsm_GoToWaitingPoint.evaluator.paused = true;

        fsm_ConquerOrDefend.fsm.Restart();
        //FSM defender base 
    }
}
