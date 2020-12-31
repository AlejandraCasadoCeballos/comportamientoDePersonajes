using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{

    [SerializeField] int dronADCountPerTeam = 10;
    [SerializeField] GameObject dronADPrefab;
    [SerializeField] int dronCACCountPerTeam = 10;
    [SerializeField] GameObject dronCACPrefab;
    [SerializeField] int recruiterCountPerTeam = 2;
    [SerializeField] GameObject recruiterPrefab;
    //[SerializeField] GameObject dronADPrefab;

    [SerializeField] Color[] _teams;
    public static Color[] teamColors;
    public static int numTeams { get => teamColors.Length; }

    public static Queue<DronBehaviour>[] enemySpawnQueues;

    private void Awake()
    {
        teamColors = _teams;
        enemySpawnQueues = new Queue<DronBehaviour>[numTeams];
        DronBehaviour dronBehaviour;
        GameObject obj;
        for(int i = 0; i < numTeams; i++)
        {
            enemySpawnQueues[i] = new Queue<DronBehaviour>();
            for (int j = 0; j < recruiterCountPerTeam; j++)
            {
                obj = Instantiate(recruiterPrefab);
                obj.SetActive(false);
                dronBehaviour = obj.GetComponent<DronBehaviour>();
                dronBehaviour.team = i;
                enemySpawnQueues[i].Enqueue(dronBehaviour);
            }
            for (int j = 0; j < dronADCountPerTeam + dronCACCountPerTeam; j++)
            {
                if(j%2 == 0)obj = Instantiate(dronADPrefab);
                else obj = Instantiate(dronCACPrefab);

                obj.SetActive(false);
                dronBehaviour = obj.GetComponent<DronBehaviour>();
                dronBehaviour.team = i;
                enemySpawnQueues[i].Enqueue(dronBehaviour);
            }
        }
    }
}
