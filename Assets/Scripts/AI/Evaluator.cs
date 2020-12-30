using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evaluator : MonoBehaviour
{
    private Behaviour behaviour;
    [SerializeField] private float standByReevaluationRate = 1f;

    public void SetBehaviour(Behaviour behaviour)
    {
        this.behaviour = behaviour;
        StopAllCoroutines();
        StartCoroutine(EvaluateAtFixedRate());
        this.behaviour.Evaluate();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(EvaluateAtFixedRate());
    }

    private void Update()
    {
        behaviour?.currentNode?.Update();
    }

    IEnumerator EvaluateAtFixedRate()
    {
        while (true)
        {
            if (behaviour != null && behaviour.currentNode != null && behaviour.currentNode.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(behaviour.currentNode.reevaluationRate);
                behaviour.Evaluate();
            }
            else yield return new WaitForSeconds(standByReevaluationRate);
        }
    }
}
