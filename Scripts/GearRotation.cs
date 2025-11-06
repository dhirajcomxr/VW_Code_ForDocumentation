using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearRotation : MonoBehaviour
{
    // Start is called before the first frame update
   

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, 0.0f, -20.0f*Time.deltaTime, Space.Self);

    }
}
