using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleIsolator : MonoBehaviour
{
   [SerializeField] VWModuleManager moduleManager;

    [SerializeField] private int moduleNum;
    private void Reset()
    {
        moduleManager = FindObjectOfType<VWModuleManager>();
    }
    public void Module() => Module(moduleNum);
    public void Module(int m)
    {
        if (moduleManager)
            moduleManager.SelectModule(moduleNum);
    }
}

