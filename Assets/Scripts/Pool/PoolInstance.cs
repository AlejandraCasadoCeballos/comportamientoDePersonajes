using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolInstance : MonoBehaviour
{
    public ObjectPool pool;

    public void PushBackToPool()
    {
        pool?.AddInstance(this);
    }
}
