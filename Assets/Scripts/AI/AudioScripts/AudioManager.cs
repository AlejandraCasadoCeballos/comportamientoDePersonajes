using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip baseBeingCaptured;
    [SerializeField]
    private AudioClip baseCaptured;

    private AudioSource baseSound;
    [HideInInspector]
    private BaseBehaviour baseComponent;

    private int numBases;
    private void Start()
    {
        numBases  = BaseBehaviour.bases.Count();
        baseComponent = gameObject.GetComponentInParent<BaseBehaviour>();
        baseComponent.captureEvent += baseCapturedEvent;
        baseSound = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (baseComponent.isBeingConquered && !baseSound.isPlaying)
        {
            if (baseSound.clip != baseBeingCaptured)
            {
                baseSound.clip = baseBeingCaptured;
            }
            baseSound.Play();
        }
        else if (!baseComponent.isBeingConquered && baseSound.isPlaying)
        { //Aqui debería hacer que si paran de conquistar pues pare el sonido
            if (baseSound.clip = baseBeingCaptured)
            {
                baseSound.Stop();
            }
        }
    }

    private void baseCapturedEvent(string str)
    {
        baseSound.clip = baseCaptured;
        baseSound.Play();
    }
}
