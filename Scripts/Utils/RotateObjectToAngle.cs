using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectToAngle : MonoBehaviour
{
    public GameObject objectToRotate;
    public float angleToRotate;
    public enum axis { x, y, z };
    public axis _axis;
    
    public bool rotate=false;
    private bool rotated = false;

    private Quaternion originalRotation;

    
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {

        if (rotate)
        {
            if (!rotated)
            {
                originalRotation = objectToRotate.transform.rotation;
                switch (_axis)
                {
                    case axis.x:
                        objectToRotate.transform.localRotation = Quaternion.Euler(angleToRotate, objectToRotate.transform.localEulerAngles.y, objectToRotate.transform.localEulerAngles.z);
                        break;
                    case axis.y:
                        objectToRotate.transform.localRotation = Quaternion.Euler(objectToRotate.transform.localEulerAngles.x, angleToRotate, objectToRotate.transform.localEulerAngles.z);
                        break;
                    case axis.z:
                        objectToRotate.transform.localRotation = Quaternion.Euler(objectToRotate.transform.localEulerAngles.x, objectToRotate.transform.localEulerAngles.y, angleToRotate);
                        break;
                }
                Debug.Log("rotated" + objectToRotate.transform.localEulerAngles);
                rotated = true;

               
            }
        }
        else
        {
            if (rotated)
            {
                Debug.Log("reversed" + objectToRotate.transform.localEulerAngles);
                objectToRotate.transform.rotation = originalRotation;
                rotated = false;
            }
        }
        
    }

    public void rotateXTo180()
    {
        objectToRotate.transform.localRotation = Quaternion.Euler(-180f, 0f, 0f);
    }
    public void rotateXTo0()
    {
     
            objectToRotate.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        
    }
  

    public void rotateYTo90()
    {
        objectToRotate.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }


  
}
