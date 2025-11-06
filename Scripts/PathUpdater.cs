using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUpdater : MonoBehaviour
{
    GameObject pathUpdater;
    public bool isUpdating = true;
    // Start is called before the first frame update
    void Start()
    {
        pathUpdater = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUpdating)
        {
            pathUpdater.GetComponent<PathCreation.Examples.RoadMeshCreator>().OnPathUpdate();
        }   
    }

}
