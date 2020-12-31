using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsefulFuncs
{
    public static List<T> GetComponentsInChildrenDepthOne<T>(Transform t)
    {
        List<T> components = new List<T>();
        for(int i = 0; i < t.childCount; i++)
        {
            T comp = t.GetChild(i).GetComponent<T>();
            components.Add(comp);
        }
        return components;
    }
}
