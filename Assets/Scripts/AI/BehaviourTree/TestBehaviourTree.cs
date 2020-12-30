using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehaviourTree : MonoBehaviour
{
    BehaviourTree tree;

    [SerializeField] bool majorityOfEnemiesInside = true;
    [SerializeField] bool belongsToATeam = true;
    [SerializeField] bool majorityOfAgentsInside = true;

    private void Start()
    {
        tree = GetComponent<BehaviourTree>();

        PerceptionNode belongsToATeamPerception = new PerceptionNode(()=> belongsToATeam ? 1f : 0f);
        PerceptionNode majorityOfEnemiesInsidePerception = new PerceptionNode(() => majorityOfEnemiesInside ? 1f : 0f);
        PerceptionNode majorityOfAgentsInsidePerception = new PerceptionNode(() => majorityOfAgentsInside ? 1f : 0f);

        ActionNode continueNeutralization = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnUpdate(
            ()=>Debug.Log("continueNeutralization")
        );

        ActionNode cancelNeutralization = new ActionNode(1f, ActionNode.Reevaluation.onlyOnEnd);
        cancelNeutralization.SetOnUpdate(
            () => {
                Debug.Log("cancelNeutralization");
                cancelNeutralization.End();
            });
    }
}
