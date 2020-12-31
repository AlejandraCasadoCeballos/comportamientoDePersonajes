using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBehaviour : MonoBehaviour
{
    private int majorityTeam = -1;
    public int team = -1;
    public bool belongsToATeam { get => team >= 0;  }
    
    private Evaluator evaluator;
    private DecisionTree tree;

    private int[] agentsCount;

    [SerializeField] float neutralizationTime = 3f;
    [SerializeField] float conquestTime = 3f;
    bool isBeingConquered { get => team == -1 && timer > 0f; }
    bool isBeingNeutralized { get => team >= 0 && timer > 0f; }
    float timer = 0f;

    [SerializeField] Color neutralColor;
    [SerializeField] new MeshRenderer renderer;
    private Material mat;

    private const string emiTintStr = "EmiTint";

    private void UpdateColor()
    {
        Debug.Log("team: " + team + " majority: " + majorityTeam + " timer: " + timer + " neutralizing: " + isBeingNeutralized + " conquering: " + isBeingConquered);
        float lerp = ((Mathf.Sin(timer * Mathf.PI - (Mathf.PI * 0.5f)) + 1f) * 0.5f);
        if (isBeingConquered)
        {
            mat.SetColor(emiTintStr, Color.Lerp(neutralColor, TeamManager.teamColors[majorityTeam], lerp));
        } else if (isBeingNeutralized)
        {
            mat.SetColor(emiTintStr, Color.Lerp(TeamManager.teamColors[team], neutralColor, lerp));
        } else
        {
            if(team < 0)
            {
                mat.SetColor(emiTintStr, neutralColor);
            } else
            {
                mat.SetColor(emiTintStr, TeamManager.teamColors[team]);
            }
        }
    }

    private int CheckMajority()
    {
        majorityTeam = team;
        int currentTeamCount = 0;
        if (belongsToATeam) currentTeamCount = agentsCount[team];
        for(int i = 0; i < TeamManager.numTeams; i++)
        {
            if(agentsCount[i] > currentTeamCount)
            {
                majorityTeam = i;
                currentTeamCount = agentsCount[i];
                
            }
        }
        return majorityTeam;
    }

    private bool CheckAllCountsEqual()
    {
        bool allEquals = true;
        int firstValue = agentsCount[0];
        for(int i = 1; i < TeamManager.numTeams; i++)
        {
            if (agentsCount[i] != firstValue) allEquals = false;
        }
        return allEquals;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if(behaviour != null && behaviour.team < TeamManager.numTeams)
        {
            agentsCount[behaviour.team]++;
            evaluator?.Evaluate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Agent") return;
        DronBehaviour behaviour = other.GetComponent<DronBehaviour>();
        if (behaviour != null && behaviour.team < TeamManager.numTeams)
        {
            agentsCount[behaviour.team]--;
            evaluator?.Evaluate();
        }
    }

    private void Start()
    {
        mat = Instantiate(renderer.sharedMaterial);
        renderer.sharedMaterial = mat;

        agentsCount = new int[TeamManager.numTeams];

        PerceptionNode p_belongsToATeam = new PerceptionNode(()=> belongsToATeam ? 1f : 0f);
        PerceptionNode p_majorityOfEnemiesInside = new PerceptionNode(() => CheckMajority() != team ? 1f : 0f);
        PerceptionNode p_majorityOfAgentsInside = new PerceptionNode(() => {
            CheckMajority();
            return majorityTeam != team && !CheckAllCountsEqual() ? 1f : 0f;
        });

        ActionNode a_continueNeutralization = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(() => timer = 0f);
        a_continueNeutralization.SetOnUpdate(
            ()=>
            {
                Debug.Log("continueNeutralization");
                timer += Time.deltaTime;
                if(timer >= neutralizationTime)
                {
                    timer = 0f;
                    team = -1;
                    a_continueNeutralization.End();
                }
                UpdateColor();
            }
        );

        ActionNode a_cancelNeutralization = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate);
        a_cancelNeutralization.SetOnBegin(
            () => {
                Debug.Log("cancelNeutralization");
                timer = 0f;
                UpdateColor();
                a_cancelNeutralization.End();
            });

        ActionNode a_continueConquest = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(() => timer = 0f);
        a_continueConquest.SetOnUpdate(
            () =>
            {
                Debug.Log("continueConquest ");
                timer += Time.deltaTime;
                if(timer >= conquestTime)
                {
                    timer = 0f;
                    team = majorityTeam;
                    a_continueConquest.End();
                }
                UpdateColor();
            }
        );

        ActionNode a_cancelConquest = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate);
        a_cancelConquest.SetOnBegin(
            () => {
                Debug.Log("cancelConquest");
                timer = 0f;
                UpdateColor();
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

        tree = new DecisionTree();
        tree.SetParentNode(st_root);

        evaluator = GetComponent<Evaluator>();
        evaluator.SetBehaviour(tree);
    }
}
