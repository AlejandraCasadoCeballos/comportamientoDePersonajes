using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip shoot;
    private AudioSource sound;

    [SerializeField] float speed;
    Vector3 direction;
    float damage = 1f;
    PoolInstance pool;
    
    new Renderer renderer;
    [SerializeField] int team;
    [SerializeField] Material neutralMat;

    private void Start()
    {
        direction = transform.forward;
        pool = GetComponent<PoolInstance>();
        renderer = GetComponentInChildren<Renderer>();
        sound = GetComponent<AudioSource>();
        sound.clip = shoot;
    }

    public void Init(Transform origin, Transform destiny, int team, float damage)
    {
        this.team = team;
        transform.position = origin.position;
        transform.LookAt(destiny);
        //transform.rotation = origin.rotation;
        this.damage = damage;
        direction = transform.forward;
        if(renderer == null) renderer = GetComponentInChildren<Renderer>();
        if (team < 0) renderer.sharedMaterial = neutralMat;
        else renderer.sharedMaterial = TeamManager.projectileMats[team];
        if(sound == null)
        {
            sound = GetComponent<AudioSource>();
            sound.clip = shoot;
        }
        sound.Play();
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.tag == "Agent")
            {
                DronBehaviour dronBehaviour = other.GetComponent<DronBehaviour>();
                if (dronBehaviour != null && dronBehaviour.team != team)
                {
                    dronBehaviour.ReceiveDamage(damage);
                    pool.PushBackToPool();
                }
            } else if (other.gameObject.layer == (LayerMask.NameToLayer("Floor")))
            {
                pool.PushBackToPool();
            }
            
        }
        
    }
}
