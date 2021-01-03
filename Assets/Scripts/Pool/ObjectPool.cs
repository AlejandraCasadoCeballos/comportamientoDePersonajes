using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int initSize = 10;
    [SerializeField] bool persistent = false;

    Stack<PoolInstance> objs = new Stack<PoolInstance>();


    private void Awake()
    {
        if(prefab != null)
        {
            InitPool();
        } else
        {
            gameObject.name = "[POOL] NOT INITIALIZED";
        }
    }

    public void SetPrefabAndInit(GameObject prefab)
    {
        if(prefab != null)
        {
            this.prefab = prefab;
            InitPool();
        }
    }

    public void InitPool()
    {
        gameObject.name = "[POOL] " + prefab.name;
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
        if (persistent)
        {
            DontDestroyOnLoad(gameObject);
        }
        CreateInstances(initSize);
    }

    public static ObjectPool Create(GameObject prefab, int initSize, bool persistent = false)
    {
        GameObject obj = new GameObject("[POOL] NOT INITIALIZED");
        var pool = obj.AddComponent<ObjectPool>();
        pool.initSize = initSize;
        pool.persistent = persistent;
        pool.SetPrefabAndInit(prefab);
        return pool;
    }

    public void CreateInstances(int count = 1)
    {
        GameObject obj;
        int totalCount = objs.Count;
        PoolInstance poolInstance;
        for(int i = 0; i < count; i++)
        {
            obj = Instantiate(prefab, transform);
            obj.name = prefab.name + " (" + (totalCount + i) + ")";
            poolInstance = obj.GetComponent<PoolInstance>();
            if(poolInstance == null)
            {
                poolInstance = obj.AddComponent<PoolInstance>();
            }
            AddInstance(poolInstance);
        }
    }

    public void AddInstance(PoolInstance instance)
    {
        instance.pool = this;
        instance.gameObject.SetActive(false);
        objs.Push(instance);
        instance.transform.SetParent(transform);
    }

    public PoolInstance GetInstance(bool active = true, Transform parent = null)
    {
        if(objs.Count <= 0) CreateInstances(1);
        PoolInstance instance = objs.Pop();
        instance.gameObject.SetActive(active);
        instance.transform.SetParent(parent);
        return instance;
    }
}
