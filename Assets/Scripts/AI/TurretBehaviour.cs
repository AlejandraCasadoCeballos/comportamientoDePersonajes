using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    BaseBehaviour baseBehaviour;

    private void Start()
    {
        baseBehaviour = GetComponentInParent<BaseBehaviour>();
        SphereCollider collision = GetComponent<SphereCollider>();
        rangeTrigger = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        rangeTrigger.center = collision.center;
        rangeTrigger.radius = attackRange;
        rangeTrigger.isTrigger = true;
        originalForward = transform.right;
    }

    private void CreateFSM()
    {
        //var idleState = new FSM_Node()
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
        float angle = Vector3.Angle(transform.right, dirToAgent);
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
