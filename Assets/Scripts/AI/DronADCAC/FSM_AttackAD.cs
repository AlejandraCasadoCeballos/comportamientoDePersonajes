using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_AttackAD : FSM_Attack
{
    [SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float rateOfFire = 1f;
    [SerializeField] float aimSpeed = 3f;
    [SerializeField] float shootAngle = 10f;
    float timer = 0f;

    bool hasShot = false;

    void Start()
    {
        dronBehaviour = GetComponentInParent<DronADCACBehaviour>();
        CreateFSM();
    }

    private void CreateFSM()
    {
        fsm = new FSM(0.5f);
        var dieState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        var anyToDie = new FSM_Edge(fsm.anyState, dieState, new List<Func<bool>>() { () => dronBehaviour.life <= 0f });
        fsm.anyState.AddEdge(anyToDie);

        var idleState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        var shootState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        var aimState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);

        idleState.SetOnBegin(() =>
        {
            dronBehaviour.hasRespawned = false;

            dronBehaviour.ai.isStopped = true;
            hasShot = false;
        });

        var dieToIdle = new FSM_Edge(dieState, idleState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned,
        });
        dieState.AddEdge(dieToIdle);
        
        shootState.SetOnBegin(() =>
        {
            timer = 0f;

            //TODO: SHOOT PROJECTILE
        });
        shootState.SetOnUpdate(() =>
        {
            timer += Time.deltaTime;
            if(timer >= rateOfFire)
            {
                hasShot = true;
            }
        });
        var shootToAim = new FSM_Edge(shootState, aimState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.closestEnemy != null
        });
        shootState.AddEdge(shootToAim);
        var shootToIdle = new FSM_Edge(shootState, idleState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.closestEnemy == null && hasShot
        });
        shootState.AddEdge(shootToIdle);



        aimState.SetOnBegin(() =>
        {
            hasShot = false;
            
        });
        aimState.SetOnUpdate(() =>
        {
            Vector3 target = dronBehaviour.closestEnemy.transform.position;
            Debug.DrawLine(transform.position, target);
            float angleY = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(target - transform.position, Vector3.up), Vector3.up);
            float rotationY = angleY * aimSpeed * Time.deltaTime;
            if (Mathf.Abs(rotationY) > Mathf.Abs(angleY)) rotationY = angleY;
            transform.RotateAround(transform.position, Vector3.up, rotationY);
        });
        var aimToShoot = new FSM_Edge(aimState, shootState, new List<Func<bool>>()
        {
            () => {
                Vector3 target = dronBehaviour.closestEnemy.transform.position;
                return Vector3.Angle(transform.forward, (target-transform.position)) < shootAngle*0.5f;
                }
        });
        aimState.AddEdge(aimToShoot);
        var aimToIdle = new FSM_Edge(aimState, shootState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.closestEnemy == null
        });
        aimState.AddEdge(aimToIdle);

        fsm.SetNodes(new FSM_Node[] { dieState, idleState, shootState, aimState });
        fsm.SetRoot(idleState);
        evaluator.SetBehaviour(fsm);
    }
}
