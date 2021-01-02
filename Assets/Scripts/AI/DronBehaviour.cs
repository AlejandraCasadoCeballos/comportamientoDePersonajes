using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DronBehaviour : MonoBehaviour
{
    [HideInInspector] public int team = 0;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float maxLife;
    [HideInInspector] public float life;
    [HideInInspector] public bool hasRespawned = false;
    [HideInInspector] public NavMeshAgent ai;

    private void Awake()
    {
        life = maxLife;
        ai=GetComponent<NavMeshAgent>();
        ai.speed = movementSpeed;
    }
}
