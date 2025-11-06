using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TensionerRollerBoltTrigger : MonoBehaviour
{
    public GameObject dowelPin;
    bool toRotate = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (toRotate)
        //{
        //    float angle = transform.localRotation.eulerAngles.z;
        //    if (transform.localRotation.eulerAngles.z<-37)
        //        angle=Mathf.Lerp()
        //        transform.Rotate(Vector3.forward, 38*Time.deltaTime, Space.Self);
        //}
    }
    public void RotateTensionerDowelPin()
    {
        StartCoroutine(RotateTransformToAngle(dowelPin.transform, new Vector3(0f,180f, 29.86f),1f));
    }
    IEnumerator RotateTransformByAngle(Transform _transform,Vector3 byAngles, float inTime)
    {
        var fromAngle = _transform.localRotation;
        var toAngle = Quaternion.Euler(_transform.localEulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            _transform.localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }
    IEnumerator RotateTransformToAngle(Transform _transform, Vector3 _toAngle, float inTime)
    {
        var fromAngle = _transform.localRotation;
        var toAngle = Quaternion.Euler(_toAngle);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            _transform.localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }
}
