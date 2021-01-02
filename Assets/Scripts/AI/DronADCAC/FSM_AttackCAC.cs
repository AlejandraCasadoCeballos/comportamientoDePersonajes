using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_AttackCAC : FSM_Attack
{
    [SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float idleDisplacement = 3f;
    [SerializeField] float aimSpeed = 3f;

    float timer;
    bool hasAttacked = false;

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

        var approachEnemyState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        approachEnemyState.SetOnBegin(() =>
        {
            state = "approach";
            dronBehaviour.hasRespawned = false;
            dronBehaviour.ai.isStopped = false;

            Vector3 randomDir = new Vector3(UnityEngine.Random.value, 0f, UnityEngine.Random.value).normalized;
            Vector3 dst = dronBehaviour.transform.forward * idleDisplacement + randomDir * idleDisplacement * 0.5f;

            //dronBehaviour.ai.destination = dronBehaviour.transform.position + dronBehaviour.transform.forward * 5f;
            dronBehaviour.ai.SetDestination(dronBehaviour.transform.position + dst);
        });
        approachEnemyState.SetOnUpdate(() =>
        {
            if (dronBehaviour.closestEnemy != null)
            {
                dronBehaviour.ai.stoppingDistance = attackRange;
                dronBehaviour.ai.SetDestination(dronBehaviour.closestEnemy.transform.position);

            }
            else dronBehaviour.ai.stoppingDistance = 0f;
        });

        var dieToApproachEnemy = new FSM_Edge(dieState, approachEnemyState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned,
        });

        var attackState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        attackState.SetOnBegin(() =>
        {
            state = "attack";
            dronBehaviour.ai.isStopped = true;

            hasAttacked = false;
            timer = 0f;
            anim.Play("DronCACAttack");
        });
        attackState.SetOnUpdate(() =>
        {
            timer += Time.deltaTime;
            if(timer >= attackSpeed)
            {
                hasAttacked = true;
            }

            Vector3 target = dronBehaviour.closestEnemy.transform.position;
            Debug.DrawLine(dronBehaviour.transform.position, target);
            float angleY = Vector3.SignedAngle(dronBehaviour.transform.forward, Vector3.ProjectOnPlane(target - dronBehaviour.transform.position, Vector3.up), Vector3.up);
            float rotationY = angleY * aimSpeed * Time.deltaTime;
            if (Mathf.Abs(rotationY) > Mathf.Abs(angleY)) rotationY = angleY;
            dronBehaviour.transform.RotateAround(transform.position, Vector3.up, rotationY);
        });

        var approachToAttack = new FSM_Edge(approachEnemyState, attackState, new List<Func<bool>>()
        {
            ()=>{
                Debug.Log("checking");
                if(dronBehaviour.closestEnemy ==  null) return false;
                Debug.Log("checking: " + ((dronBehaviour.transform.position-dronBehaviour.closestEnemy.transform.position).magnitude <= attackRange));
                return (dronBehaviour.transform.position-dronBehaviour.closestEnemy.transform.position).magnitude <= attackRange;
                }
        });
        var attackToApproach1 = new FSM_Edge(attackState, approachEnemyState, new List<Func<bool>>()
        {
            ()=>hasAttacked,
        });
        var attackToApproach2 = new FSM_Edge(attackState, approachEnemyState, new List<Func<bool>>()
        {
            ()=>(dronBehaviour.transform.position-dronBehaviour.closestEnemy.transform.position).magnitude > attackRange
        });

        dieState.AddEdge(dieToApproachEnemy);
        approachEnemyState.AddEdge(approachToAttack);
        attackState.AddEdge(attackToApproach1);
        attackState.AddEdge(attackToApproach2);
        fsm.SetNodes(new FSM_Node[] { dieState, approachEnemyState, attackState });
        fsm.SetRoot(approachEnemyState);
        evaluator.SetBehaviour(fsm);
    }
}
