using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class DronBehaviour : MonoBehaviour
{
    [SerializeField] public float movementSpeed;
    [SerializeField] public float maxLife;
    [HideInInspector] public int team = 0;
    [HideInInspector] public bool hasRespawned = false;
    [HideInInspector] public NavMeshAgent ai;
    [HideInInspector] public BaseBehaviour currentBase;
    [HideInInspector] public float life;


    private void Awake()
    {
        life = maxLife;
        ai=GetComponent<NavMeshAgent>();
        ai.speed = movementSpeed;
    }

    public void ReceiveDamage(float damage)
    {
        life -= damage;
    }
}
