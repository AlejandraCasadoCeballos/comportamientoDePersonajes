using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurretBehaviour : MonoBehaviour
{
    [Header("Object references")]
    [SerializeField] Transform turretHead;

    [Header("Parameters")]
    [SerializeField] float aimSpeed = 1;
    [SerializeField] float rateOfFire = 1;
    [SerializeField] float attackRange = 6;
    [SerializeField] float attackAngle = 200;
    [SerializeField] float shootAngle = 20;
    [SerializeField] float fireDamage = 3;

    private SphereCollider rangeTrigger;

    Vector3 originalForward;

    HashSet<DronBehaviour> allAgents = new HashSet<DronBehaviour>();
    DronBehaviour closestEnemy;
    BaseBehaviour baseBehaviour;
    Evaluator evaluator;

    bool hasShot = false;

    private void Start()
    {
        baseBehaviour = GetComponentInParent<BaseBehaviour>();
        SphereCollider collision = GetComponent<SphereCollider>();
        rangeTrigger = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        rangeTrigger.center = collision.center;
        rangeTrigger.radius = attackRange;
        rangeTrigger.isTrigger = true;
        originalForward = transform.right;
        evaluator = GetComponent<Evaluator>();
        CreateFSM();
    }

    private void CreateFSM()
    {
        var fsm = new FSM();

        var idleState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        idleState.SetOnBegin(() =>
        {
            hasShot = false;
        });
        idleState.SetOnUpdate(() =>
        {
            RotateToTarget(turretHead.position + originalForward);
        });
        var aimState = new FSM_Node(0.1f, ActionNode.Reevaluation.atFixedRate);
        aimState.SetOnUpdate(() =>
        {
            if(closestEnemy != null)
                RotateToTarget(closestEnemy.transform.position);
        });
        var shootState = new FSM_Node(1f, ActionNode.Reevaluation.onlyOnEnd);
        shootState.SetOnBegin(() =>
        {
            Debug.Log("SHOOT");
            hasShot = true;
            shootState.End();
        });

        var idleToAimEdge = new FSM_Edge(idleState, aimState, new List<Func<bool>>() { GetClosestEnemy });
        var aimToIdleEdge = new FSM_Edge(aimState, idleState, new List<Func<bool>>() { () => !GetClosestEnemy() });
        var aimToShootEdge = new FSM_Edge(aimState, shootState, new List<Func<bool>>() { EnemyInShootAngle });
        var shootToIdleEdge = new FSM_Edge(shootState, idleState, new List<Func<bool>>() { ()=>{
            return hasShot;
        }});

        idleState.AddEdge(idleToAimEdge);
        aimState.AddEdge(aimToIdleEdge);
        aimState.AddEdge(aimToShootEdge);
        shootState.AddEdge(shootToIdleEdge);

        
        fsm.SetNodes(new FSM_Node[] { idleState, aimState, shootState });
        fsm.SetRoot(idleState);

        evaluator.SetBehaviour(fsm);
    }

    private void RotateToTarget(Vector3 target)
    {
        Debug.DrawLine(turretHead.position, target);
        float angleY = Vector3.SignedAngle(turretHead.right, Vector3.ProjectOnPlane(target-turretHead.position, Vector3.up), Vector3.up);
        float rotationY = angleY * aimSpeed * Time.deltaTime;
        if (Mathf.Abs(rotationY) > Mathf.Abs(angleY)) rotationY = angleY;
        turretHead.RotateAround(turretHead.position, Vector3.up, rotationY);
    }

    private bool GetClosestEnemy()
    {
        float minDist = attackRange+1f;
        float dist;
        DronBehaviour closest = null;
        foreach(var d in allAgents)
        {
            if (d.team == baseBehaviour.team) continue;
            if (!AgentInAttackAngle(d)) continue;
            dist = (d.transform.position - transform.position).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = d;
            }
        }
        closestEnemy = closest;
        return closest != null;
    }

    private bool EnemyInShootAngle()
    {
        return AgentInShootAngle(closestEnemy);
    }

    private bool AgentInAttackAngle(DronBehaviour behaviour)
    {
        if (behaviour.team == baseBehaviour.team) return false;
        Vector3 dirToAgent = behaviour.transform.position - transform.position;
        float angle = Vector3.Angle(originalForward, dirToAgent);
        return angle < attackAngle * 0.5f && allAgents.Contains(behaviour);
    }

    private bool AgentInShootAngle(DronBehaviour behaviour)
    {
        if (behaviour.team == baseBehaviour.team) return false;
        Vector3 dirToAgent = behaviour.transform.position - transform.position;
        float angle = Vector3.Angle(turretHead.right, dirToAgent);
        return angle < shootAngle * 0.5f && allAgents.Contains(behaviour);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if (behaviour != null && behaviour.team < TeamManager.numTeams)
        {
            allAgents.Add(behaviour);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if (behaviour != null && behaviour.team < TeamManager.numTeams)
        {
            allAgents.Remove(behaviour);
        }
    }
}
