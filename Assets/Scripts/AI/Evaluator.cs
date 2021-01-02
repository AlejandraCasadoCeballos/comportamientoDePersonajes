using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evaluator : MonoBehaviour
{
    private Behaviour behaviour;
    [SerializeField] private float standByReevaluationRate = 1f;
    public bool paused = false;

    public void SetBehaviour(Behaviour behaviour)
    {
        this.behaviour = behaviour;
        StopAllCoroutines();
        StartCoroutine(EvaluateAtFixedRate());
        this.behaviour.currentNode?.Begin();
        this.behaviour.Evaluate();
    }

    

    public void Evaluate()
    {
        behaviour?.Evaluate();
    }

    private void OnEnable()
    {
        this.behaviour?.currentNode?.Begin();
        this.behaviour?.Evaluate();
        StopAllCoroutines();
        StartCoroutine(EvaluateAtFixedRate());
    }

    private void Update()
    {
        if(!paused)
            behaviour?.currentNode?.Update();
    }

    IEnumerator EvaluateAtFixedRate()
    {
        while (true)
        {
            if (!paused && behaviour != null && behaviour.currentNode != null && behaviour.currentNode.reevaluationMode == ActionNode.Reevaluation.atFixedRate)
            {
                yield return new WaitForSeconds(behaviour.currentNode.reevaluationRate);
                behaviour.Evaluate();
            }
            else
            {
                yield return new WaitForSeconds(standByReevaluationRate);
            }
        }
    }
}
