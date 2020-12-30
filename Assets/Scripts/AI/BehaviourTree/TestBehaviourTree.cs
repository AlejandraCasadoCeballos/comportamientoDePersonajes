﻿using System.Collections;
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

        PerceptionNode p_belongsToATeam = new PerceptionNode(()=> belongsToATeam ? 1f : 0f);
        PerceptionNode p_majorityOfEnemiesInside = new PerceptionNode(() => majorityOfEnemiesInside ? 1f : 0f);
        PerceptionNode p_majorityOfAgentsInside = new PerceptionNode(() => majorityOfAgentsInside ? 1f : 0f);

        ActionNode a_continueNeutralization = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnUpdate(
            ()=>Debug.Log("continueNeutralization")
        );

        ActionNode a_cancelNeutralization = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate);
        a_cancelNeutralization.SetOnBegin(
            () => {
                Debug.Log("cancelNeutralization");
                a_cancelNeutralization.End();
            });

        ActionNode a_continueConquest = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnUpdate(
            () => Debug.Log("continueConquest")
        );

        ActionNode a_cancelConquest = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate);
        a_cancelConquest.SetOnBegin(
            () => {
                Debug.Log("cancelConquest");
                a_cancelConquest.End();
            });

        SequenceNode sq_continueNeutralization = new SequenceNode(new Node[] {
            p_majorityOfEnemiesInside,
            a_continueNeutralization});

        SequenceNode sq_continueConquest = new SequenceNode(new Node[]
        {
            p_majorityOfAgentsInside,
            a_continueConquest
        });

        SelectorNode st_neutralization = new SelectorNode(new Node[]
        {
            sq_continueNeutralization,
            a_cancelNeutralization
        });

        SelectorNode st_conquest = new SelectorNode(new Node[]
        {
            sq_continueConquest,
            a_cancelConquest
        });

        SuccederNode sd_neutraliztion = new SuccederNode(st_neutralization);

        SequenceNode sq_belongsToATeam = new SequenceNode(new Node[]
        {
            p_belongsToATeam,
            sd_neutraliztion
        });

        SelectorNode st_root = new SelectorNode(new Node[]
        {
            sq_belongsToATeam,
            st_conquest
        });

        tree.SetParentNode(st_root);
    }
}
