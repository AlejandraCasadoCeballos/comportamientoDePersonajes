using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnerBehaviour : MonoBehaviour
{
    Evaluator evaluator;
    List<Transform> spawnPoints;
    BaseBehaviour baseBehaviour;
    int spawnPointsCount;
    FSM baseSpawner;
    FSM_Node waitingState, generateState;
    FSM_Edge generate_Wait_Edge, wait_Generate_Edge;
    [SerializeField] float respawnTime = 5.0f;
    float time;
    bool spawned;

    void Start()
    {
        baseBehaviour = GetComponentInParent<BaseBehaviour>();
        evaluator = GetComponent<Evaluator>();
        spawnPoints = new List<Transform>();
        spawnPointsCount = transform.childCount;
        time = 0.0f;

        #region Variables FSM
        waitingState = new FSM_Node();
        generateState = new FSM_Node();
        generate_Wait_Edge = new FSM_Edge(generateState, waitingState,new List<Func<bool>>() {HasSpawned}); // le meto la función que determina la condición o eso creia
        wait_Generate_Edge = new FSM_Edge(waitingState, generateState, new List<Func<bool>>() {CheckTimeSpawn}) ;
        waitingState.AddEdge(wait_Generate_Edge);
        generateState.AddEdge(generate_Wait_Edge);
        baseSpawner = new FSM();
        baseSpawner.AddNode(waitingState);
        baseSpawner.AddNode(generateState);
        baseSpawner.SetRoot(waitingState);
        #endregion

        for (int i = 0; i < spawnPointsCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i));
            Spawn(i); //Spawn an agent for every spawn point at start
        }


        //TO DO: implementar máquina de estados de la especificación del proyecto para el spawner

        
      
        evaluator.SetBehaviour(baseSpawner);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (CheckTimeSpawn())
        {
            baseSpawner.Evaluate();
            Spawn();
            time = 0.0f;
        }
        if (HasSpawned())
        {
            baseSpawner.Evaluate();
            spawned = false;
        }
    }

    private bool CheckTimeSpawn()
    {
        return time >= respawnTime;
    }

    private bool HasSpawned()
    {
        return spawned;
    }
    private void Spawn(int index = -1)
    {
        int team = baseBehaviour.team;
        if (team < 0) return;
        int i = index < 0 ? UnityEngine.Random.Range(0, spawnPointsCount) : index;
        Transform spawnPoint = spawnPoints[i];
        Queue<DronBehaviour> queue = TeamManager.enemySpawnQueues[team];
        if(queue.Count > 0)
        {
            DronBehaviour dron = queue.Dequeue();
            dron.transform.position = spawnPoint.position;
            dron.transform.rotation = spawnPoint.rotation;
            dron.gameObject.SetActive(true);
        }
        spawned = true;
    }
}
