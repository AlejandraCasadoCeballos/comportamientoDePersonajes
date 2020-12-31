using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerBehaviour : MonoBehaviour
{
    Evaluator evaluator;
    List<Transform> spawnPoints;
    BaseBehaviour baseBehaviour;
    int spawnPointsCount;

    void Start()
    {
        baseBehaviour = GetComponentInParent<BaseBehaviour>();
        evaluator = GetComponent<Evaluator>();
        spawnPoints = new List<Transform>();
        spawnPointsCount = transform.childCount;
        for (int i = 0; i < spawnPointsCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i));
            Spawn(i); //Spawn an agent for every spawn point at start
        }


        //TO DO: implementar máquina de estados de la especificación del proyecto para el spawner


        //evaluator.SetBehaviour(/*FSM AQUÍ*/);
    }

    private void Spawn(int index = -1)
    {
        int team = baseBehaviour.team;
        if (team < 0) return;
        int i = index < 0 ? Random.Range(0, spawnPointsCount) : index;
        Transform spawnPoint = spawnPoints[i];
        Queue<DronBehaviour> queue = TeamManager.enemySpawnQueues[team];
        if(queue.Count > 0)
        {
            DronBehaviour dron = queue.Dequeue();
            dron.transform.position = spawnPoint.position;
            dron.transform.rotation = spawnPoint.rotation;
            dron.gameObject.SetActive(true);
        }
    }
}
