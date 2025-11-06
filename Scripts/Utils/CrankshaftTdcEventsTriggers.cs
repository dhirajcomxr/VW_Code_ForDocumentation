using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrankshaftTdcEventsTriggers : MonoBehaviour
{
    public GameObject actuator1;
    public GameObject actuator2;
    public GameObject sprocket;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void CrankshaftOnTdc()
    {

        if (GetComponent<RotateCrankshaft>().objectsToRotate[0]._toRotate && GetComponent<RotateCrankshaft>().objectsToRotate[1]._toRotate)
        {
            actuator1.transform.localRotation = Quaternion.Euler(-90, 0, -132.665f);
            actuator2.transform.localRotation = Quaternion.Euler(-90, 0, 89.207f);
        }
        sprocket.transform.localRotation = Quaternion.Euler(0, 0, 0);

    }
    public void CrankshaftBeforeTdc()
    {
        //if (GetComponent<RotateCrankshaft>().objectsToRotate[0]._toRotate && GetComponent<RotateCrankshaft>().objectsToRotate[1]._toRotate)
        //{
        //    actuator1.transform.localRotation = Quaternion.Euler(-90, 0, -132.665f - 80);
        //    actuator2.transform.localRotation = Quaternion.Euler(-90, 0, 89.207f - 80);
        //}
        sprocket.transform.localRotation = Quaternion.Euler(0,-120f, 0);

    }

}
