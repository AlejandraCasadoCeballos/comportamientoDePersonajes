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

    public static Queue<DronBehaviour>[] dronSpawnQueues;

    [SerializeField] ObjectPool _projectilePool;
    public static ObjectPool projectilePool;
    [SerializeField] Material projectileMat;
    [HideInInspector] public static Material[] projectileMats;

    private void Awake()
    {
        projectilePool = _projectilePool;
        teamColors = _teams;
        dronSpawnQueues = new Queue<DronBehaviour>[numTeams];
        DronBehaviour dronBehaviour;
        GameObject obj;
        Material dronADMat;
        Material dronCACMat;
        Material recruiterMat;
        MeshRenderer renderer;

        List<MeshRenderer> agentRenderers;

        projectileMats = new Material[numTeams];

        for(int i = 0; i < numTeams; i++)
        {
            projectileMats[i] = Instantiate(projectileMat);
            projectileMats[i].SetColor("EmiTint", teamColors[i]*0.95f);
            projectileMats[i].SetColor("AlbedoTint", teamColors[i]);

            dronSpawnQueues[i] = new Queue<DronBehaviour>();

            renderer = dronADPrefab.GetComponentInChildren<MeshRenderer>();
            dronADMat = Instantiate(renderer.sharedMaterial);
            dronADMat.SetColor("AlbedoTint", teamColors[i]);

            renderer = dronCACPrefab.GetComponentInChildren<MeshRenderer>();
            dronCACMat = Instantiate(renderer.sharedMaterial);
            dronCACMat.SetColor("AlbedoTint", teamColors[i]);

            renderer = recruiterPrefab.GetComponentInChildren<MeshRenderer>();
            recruiterMat = Instantiate(renderer.sharedMaterial);
            recruiterMat.SetColor("AlbedoTint", teamColors[i]);

            for (int j = 0; j < recruiterCountPerTeam; j++)
            {
                obj = Instantiate(recruiterPrefab);
                obj.SetActive(false);
                dronBehaviour = obj.GetComponent<DronBehaviour>();
                dronBehaviour.team = i;
                dronSpawnQueues[i].Enqueue(dronBehaviour);
                agentRenderers = UsefulFuncs.GetComponentsInChildrenDepthOne<MeshRenderer>(obj.transform);
                foreach (var r in agentRenderers) if (r != null) r.sharedMaterial = recruiterMat;
            }
            List<DronBehaviour> aux = new List<DronBehaviour>();
            for(int j = 0; j < dronADCountPerTeam; j++)
            {
                obj = Instantiate(dronADPrefab);
                obj.SetActive(false);
                dronBehaviour = obj.GetComponent<DronBehaviour>();
                dronBehaviour.team = i;
                aux.Add(dronBehaviour);
                //dronSpawnQueues[i].Enqueue(dronBehaviour);
                agentRenderers = UsefulFuncs.GetComponentsInChildrenDepthOne<MeshRenderer>(obj.transform);
                foreach (var r in agentRenderers)
                {
                    if (r != null)
                    {
                        r.sharedMaterial = dronADMat;
                    }
                }
            }
            for (int j = 0; j < dronCACCountPerTeam; j++)
            {
                obj = Instantiate(dronCACPrefab);
                obj.SetActive(false);
                dronBehaviour = obj.GetComponent<DronBehaviour>();
                dronBehaviour.team = i;
                aux.Add(dronBehaviour);
                //dronSpawnQueues[i].Enqueue(dronBehaviour);
                agentRenderers = UsefulFuncs.GetComponentsInChildrenDepthOne<MeshRenderer>(obj.transform);
                foreach (var r in agentRenderers)
                {
                    if (r != null)
                    {
                        r.sharedMaterial = dronCACMat;
                    }
                }
            }
            int totalCount = aux.Count;
            int index;
            for(int j = 0; j < totalCount; j++)
            {
                index = Random.Range(0, totalCount - j);
                dronSpawnQueues[i].Enqueue(aux[index]);
                aux.RemoveAt(index);
            }
        }
    }

    public static void AddDronToQueue(DronBehaviour dron)
    {
        dron.gameObject.SetActive(false);
        dronSpawnQueues[dron.team].Enqueue(dron);
    }
}
