using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ObjectsToRotate {
    public GameObject _object;
    public bool _toRotate=false;
    public Vector3 _axis;
}

public class RotateCrankshaft : MonoBehaviour
{

    public ObjectsToRotate[] objectsToRotate;
    public float speed;
    public bool isToRotate;
    public GameObject Belt;
    public float beltSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isToRotate)
        {
            foreach (ObjectsToRotate o in objectsToRotate)
            {
                //Vector3 eulers = o._object.transform.localEulerAngles;
                //eulers += o._axis * speed;
                //o._object.transform.localEulerAngles = eulers;
                if(o._toRotate)
                o._object.transform.Rotate(o._axis, speed,Space.Self);
                
            }
            Belt.GetComponent<MeshRenderer>().material.mainTextureOffset += new Vector2(0, Time.deltaTime * beltSpeed);
        }
    }
}
