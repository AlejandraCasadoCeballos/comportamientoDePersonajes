using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtilitySystem : MonoBehaviour
{
    UtilitySystem utilitySystem;
    [SerializeField] private float numP1;
    [SerializeField] private float numP2;
    [SerializeField] private float numP3;

    private void Awake()
    {
        utilitySystem = GetComponent<UtilitySystem>();

        PerceptionNode p1 = new PerceptionNode(()=> numP1);
        PerceptionNode p2 = new PerceptionNode(() => numP2);
        PerceptionNode p3 = new PerceptionNode(() => numP3);

        ActionNode node1 = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            ()=> Debug.Log("Empieza nodo 1"));

        ActionNode node2 = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => Debug.Log("Empieza nodo 2"));

        ActionNode node3 = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => Debug.Log("Empieza nodo 3"));

        UtilityFunction u1=new UtilityFunction(node1,p1).SetOnEvaluateUtility(
            (float per1) => per1);
        
        UtilityFunction u2 = new UtilityFunction(node2, p2).SetOnEvaluateUtility(
            (float per2) => per2);

        UtilityFunction u3 = new UtilityFunction(node3, p3).SetOnEvaluateUtility(
            (float per3) => per3);

        utilitySystem.SetUtilityFunctions(new HashSet<UtilityFunction>(){u1,u2,u3});
    }
}
