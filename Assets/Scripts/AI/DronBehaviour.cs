using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronBehaviour : MonoBehaviour
{
    [SerializeField] public int team = 0;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float maxLife;
    private float life = 0f;
}
