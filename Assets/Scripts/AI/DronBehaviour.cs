using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronBehaviour : MonoBehaviour
{
    [SerializeField] public int team = 0;

    private void Update()
    {
        transform.position -= Vector3.forward*0.1f;
    }
}
