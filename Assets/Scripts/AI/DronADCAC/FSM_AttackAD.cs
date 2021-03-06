﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FSM_AttackAD : FSM_Attack
{
    [SerializeField] Transform projectileSpawnPoint;
    //[SerializeField] float attackRange;
    DronADCACBehaviour dronBehaviour;

    [SerializeField] float rateOfFire = 1f;
    [SerializeField] float aimSpeed = 3f;
    [SerializeField] float shootAngle = 10f;
    [SerializeField] float idleDisplacement = 3f;
    float timer = 0f;

    bool hasShot = false;

    [SerializeField] string state = "";

    private void Awake()
    {
        evaluator = GetComponent<Evaluator>();
        dronBehaviour = GetComponentInParent<DronADCACBehaviour>();
        CreateFSM();
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

        var idleState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        var shootState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);
        var aimState = new FSM_Node(0.3f, ActionNode.Reevaluation.atFixedRate);

        idleState.SetOnBegin(() =>
        {
            state = "idle";
            dronBehaviour.hasRespawned = false;
            if (dronBehaviour.gameObject.activeSelf)
            {
                dronBehaviour.ai.isStopped = false;
                hasShot = false;
                //dronBehaviour.ai.Warp(new Vector3(8.96f, 1.3f, -20.86f));
                Vector3 randomDir = new Vector3(UnityEngine.Random.value, 0f, UnityEngine.Random.value).normalized;
                Vector3 dst = dronBehaviour.transform.forward * idleDisplacement + randomDir * idleDisplacement * 0.5f;

                //dronBehaviour.ai.destination = dronBehaviour.transform.position + dronBehaviour.transform.forward * 5f;
                dronBehaviour.ai.SetDestination(dronBehaviour.transform.position + dst);
            }
            
        });

        idleState.SetOnUpdate(() =>
        {
            Vector3 dst = dronBehaviour.ai.destination;
            Debug.DrawLine(dronBehaviour.transform.position, dst, dronBehaviour.ai.isStopped ? Color.red : Color.blue);
        });
        var idleToAim = new FSM_Edge(idleState, aimState, new List<Func<bool>>() { () => dronBehaviour.closestEnemy != null });
        idleState.AddEdge(idleToAim);
        var dieToIdle = new FSM_Edge(dieState, idleState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.hasRespawned,
        });
        dieState.AddEdge(dieToIdle);
        
        shootState.SetOnBegin(() =>
        {
            timer = 0f;
            state = "shoot";
            dronBehaviour.ai.isStopped = true;
            //TODO: SHOOT PROJECTILE
            if(dronBehaviour.closestEnemy != null)
            {
                var projectile = TeamManager.projectilePool.GetInstance();
                projectile.GetComponent<Projectile>().Init(projectileSpawnPoint, dronBehaviour.closestEnemy.transform, dronBehaviour.team, dronBehaviour.attackDamage);
            }
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
            ()=>dronBehaviour.closestEnemy != null && hasShot
        });
        shootState.AddEdge(shootToAim);
        var shootToIdle = new FSM_Edge(shootState, idleState, new List<Func<bool>>()
        {
            ()=>dronBehaviour.closestEnemy == null && hasShot
        });
        shootState.AddEdge(shootToIdle);

        aimState.SetOnBegin(() =>
        {
            state = "aim";
            hasShot = false;
            dronBehaviour.ai.isStopped = true;
        });
        aimState.SetOnUpdate(() =>
        {
            if (dronBehaviour.closestEnemy == null) return;
            Vector3 target = dronBehaviour.closestEnemy.transform.position;
            Debug.DrawLine(dronBehaviour.transform.position, target);
            float angleY = Vector3.SignedAngle(dronBehaviour.transform.forward, Vector3.ProjectOnPlane(target - dronBehaviour.transform.position, Vector3.up), Vector3.up);
            float rotationY = angleY * aimSpeed * Time.deltaTime;
            if (Mathf.Abs(rotationY) > Mathf.Abs(angleY)) rotationY = angleY;
            dronBehaviour.transform.RotateAround(transform.position, Vector3.up, rotationY);
        });
        var aimToShoot = new FSM_Edge(aimState, shootState, new List<Func<bool>>()
        {
            () => {
                if(dronBehaviour.closestEnemy == null) return false;
                Vector3 target = dronBehaviour.closestEnemy.transform.position;
                return Vector3.Angle(transform.forward, Vector3.ProjectOnPlane((target-transform.position), Vector3.up)) < shootAngle*0.5f;
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
        evaluator.SetBehaviour(fsm, false);
    }
}
