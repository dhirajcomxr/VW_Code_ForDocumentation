using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class RAMMonitor : MonoBehaviour
{
    void Start()
    {
        Application.lowMemory += LowMemory;
    }
    public void LowMemory()
    {
        Debug.Log("Low memory");
        Resources.UnloadUnusedAssets();
    }
    void Update()
    {
        //// Get the total system memory in bytes
        //long totalMemoryBytes = SystemInfo.systemMemorySize * 1024 * 1024;
        //// Get the allocated memory in bytes
        //long allocatedMemoryBytes = (long)Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        //long ReservedMemory = (long)Profiler.GetTotalReservedMemoryLong() / (1024 * 1024);
        //long UnusedReservedMemory = (long)Profiler.GetTotalUnusedReservedMemoryLong() / (1024 * 1024);

        //// Display the RAM usage in the Unity console
        //Debug.Log("Allocated: " + allocatedMemoryBytes.ToString("F2") + " MB");
        //Debug.Log("Reserved: " + ReservedMemory.ToString("F2") + " MB");
        //Debug.Log("Unused Reserved: " + UnusedReservedMemory.ToString("F2") + " MB");
    }
}
