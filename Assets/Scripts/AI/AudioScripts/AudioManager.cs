using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource baseCapturandose;

    [SerializeField]
    private AudioSource baseCapturada;

    public BaseBehaviour baseComponent;

    private int numBases;
    private void Start()
    {
        numBases  = BaseBehaviour.bases.Count();


    }

    void Update()
    {
        if (baseComponent.isBeingConquered)
        {
            baseCapturandose.Play();
        }
        else
        {
            if (baseCapturandose.isPlaying)
            {
                baseCapturandose.Stop();
            }
        }

        foreach (var equipo in BaseBehaviour.bases)
        {
            if(equipo.team > 0)
            {

            }
        }
    }
}
