using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using UnityEngine;

public class UtilitySystem
{
    private HashSet<UtilityFunctions> utilityFunctions;
    public UtilitySystem(HashSet<UtilityFunctions> utilityFunctions)
    {
        this.utilityFunctions = utilityFunctions;
    }

}
