using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTiltScript : MonoBehaviour
{
    public GameObject enginePivot;
    public Animator currAnimator;
    public Vector3 OriginalRotation;
    public Vector3 TiltRotatation;
    public float tiltTime;
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
    public void RotateTensionerEnginePivot()
    {
        StartCoroutine(RotateTransformToAngle(enginePivot.transform,TiltRotatation, tiltTime));
    }
    public void RotateTensionerEnginePivot_Rev()
    {
        StartCoroutine(RotateTransformToAngle(enginePivot.transform, OriginalRotation, tiltTime));
    }
    public void ResetTensionerEnginePivot()
    {
        Animator currAnim = GetComponent<Animator>();
        //Debug.Log("Current state: " + currAnim.GetCurrentAnimatorStateInfo(0).ToString() + " is same " + currAnim.GetCurrentAnimatorStateInfo(0).IsName("AddTimingBelt") + " speed: " + currAnim.speed);
        if (/*currAnim.GetCurrentAnimatorStateInfo(0).IsName("AddTimingBelt"*/currAnim == currAnimator && currAnim.speed < 5f)
        {
            Debug.Log("ResetTensionerEnginePivot");
            //StartCoroutine(RotateTransformToAngle(enginePivot.transform, new Vector3(0f, 0f, -67.1f), 1f));
            enginePivot.transform.localRotation = Quaternion.Euler(OriginalRotation);
        }
    }
    public void TiltedTensionerEnginePivot()
    {
        Animator currAnim = GetComponent<Animator>();
        //Debug.Log("Current state: " + currAnim.GetCurrentAnimatorStateInfo(0).ToString() + " is same " + currAnim.GetCurrentAnimatorStateInfo(0).IsName("AddTimingBelt") + " speed: " + currAnim.speed);
        if (/*currAnim.GetCurrentAnimatorStateInfo(0).IsName("AddTimingBelt"*/currAnim == currAnimator && currAnim.speed < 5f)
        {
            Debug.Log("TiltedTensionerEnginePivot");
            //StartCoroutine(RotateTransformToAngle(enginePivot.transform, new Vector3(0f, 0f, -67.1f), 1f));
            enginePivot.transform.localRotation = Quaternion.Euler(TiltRotatation);
        }
    }
    IEnumerator RotateTransformByAngle(Transform _transform, Vector3 byAngles, float inTime)
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
