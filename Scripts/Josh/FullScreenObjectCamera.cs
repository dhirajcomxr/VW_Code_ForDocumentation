using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
public class FullScreenObjectCamera : MonoBehaviour
{
    [SerializeField] float cameraDistance = 0.5f;
    [SerializeField] Outline outline;
    [SerializeField] Renderer subject;
    Camera camera;
    [SerializeField] bool rotate = false;
    public bool next = false;
    int curId = 0;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        if(subject)
        Focus(subject.bounds);
      
    }
    public void Focus(Renderer[] subjects)
    {
        Focus(GetBoundsfor(subjects));
    }
    public void Focus(Renderer r)
    {
        Focus(r.bounds);
    }
   
    Bounds GetBoundsfor(Renderer[] r)
    {
        Bounds b = new Bounds();
        foreach (var item in r)
        {
            b.Encapsulate(item.bounds);
        }
        return b;
    }
    // Update is called once per frame
    void Update()
    {
       
    }
    private void Focus(Bounds bounds)
    {
       if(rotate)
            CamDirection(bounds);
      //  float cameraDistance = 0.5f; // Constant factor
        Vector3 objectSizes = bounds.max - bounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        camera.transform.position = bounds.center - distance * camera.transform.forward;
    }
    void CamDirection(Bounds b)
    {
        float x = b.extents.x, y = b.extents.y, z = b.extents.z;
        if(x>z && y>z)// xy 
        {
            // X is greater
            Debug.Log("X-Y");
            camera.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if(y> x && z>x) //yz
        {
            // y is greater 
            Debug.Log("Y");
            camera.transform.eulerAngles = new Vector3(0, 90, 0);
     //       camera.transform.Rotate(Vector3.forward * -90);
           

        }
        else if(x>y && z>y)//xz
        {
            // Z is greater
            Debug.Log("Z");
            camera.transform.eulerAngles = new Vector3(90, 0, 0);

        }
        else
        {

        }
    }
}
