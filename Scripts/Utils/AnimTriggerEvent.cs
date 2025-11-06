using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTriggerEvent : MonoBehaviour
{
    public RotateCrankshaft rotateCrankshaft;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTimingBeltFitAnimCompleted()
    {
        foreach(ObjectsToRotate obj in rotateCrankshaft.objectsToRotate)
        {
            obj._toRotate = true;
        }
    }

}
