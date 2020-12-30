using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static int numTeams;

    [SerializeField] int _numTeams;

    private void Awake()
    {
        numTeams = _numTeams;
    }
}
