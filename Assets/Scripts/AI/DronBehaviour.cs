using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronBehaviour : MonoBehaviour
{
    [HideInInspector] public int team = 0;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float maxLife;
    [HideInInspector] public float life = 999f;
    [HideInInspector] public bool hasRespawned = false;
}
