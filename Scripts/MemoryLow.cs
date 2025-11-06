using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryLow : MonoBehaviour
{
    [SerializeField] GameObject memoryPanel;
    private void Start()
    {
        //Debug.LogError("MemoryLow");
        if (Application.isMobilePlatform)
        {
            Application.lowMemory += OnLowMemory;
        }
    }
   
    private void OnLowMemory()
    {
        Debug.LogError("MemoryLow");
        memoryPanel.SetActive(true);
        Resources.UnloadUnusedAssets();
    }
}
