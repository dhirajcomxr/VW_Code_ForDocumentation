using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCamshaftToOriginal : MonoBehaviour
{
    public GameObject actuator1;
    public GameObject actuator2;
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

        
            actuator1.transform.localRotation = Quaternion.Euler(0, -90f, -90f);
            actuator2.transform.localRotation = Quaternion.Euler(0, 0, -180f);
        
        //sprocket.transform.localRotation = Quaternion.Euler(0, 0, 0);

    }
}
