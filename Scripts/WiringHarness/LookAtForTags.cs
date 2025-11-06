using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtForTags : MonoBehaviour
{
    public Vector3 temp, target;
    public float damping = 15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        //target = new Vector3(this.transform.position.x, -this.transform.position.y, Camera.main.transform.position.z);
        //this.transform.LookAt(Camera.main.transform.position, Vector3.up);
        //this.transform.localEulerAngles = this.transform.localEulerAngles + temp;

      //**    this.transform.LookAt(Camera.main.transform);
          this.transform.LookAt(Camera.main.transform,Camera.main.transform.up);//++
      //   SmoothLookFn();

        //*****************************************************************
      transform.rotation *= Quaternion.Euler(new Vector3(-90, 90, 90));


        /*Vector3 difference = Camera.main.transform.position - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        float rotationX = Mathf.Atan2(difference.y, difference.z) * Mathf.Rad2Deg;
        float rotationY = Mathf.Atan2(difference.z, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);*/

    }
    void SmoothLookFn()
    {
        var lookPos = Camera.main.transform.position - transform.position;
       var rotation = Quaternion.LookRotation(lookPos,Camera.main.transform.up);          
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
    }
}
