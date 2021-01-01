using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Attack : MonoBehaviour
{
    [HideInInspector] public Evaluator evaluator;
    [HideInInspector] public FSM fsm;

    private void Awake()
    {
        evaluator = GetComponent<Evaluator>();
    }
}
