using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunOtherAnim : MonoBehaviour
{
    public Animator animator;
    public string trigger;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void runAnim()
    {
        //animator.keepAnimatorControllerStateOnDisable = false;
        ////animator.enabled = false;
        animator.enabled = true;
        animator.speed = 1f;
        //animator.Rebind();
        //animator.StopPlayback();

        //foreach (var v in animator.parameters)
        //{
        //    if (v.type == AnimatorControllerParameterType.Trigger)
        //    {
        //        animator.ResetTrigger(v.name);
        //    }
        //}
        //animator.ResetTrigger("AddBasePlate");
        animator.SetTrigger(trigger);

        //animator.enabled = false;
        Debug.Log("Anim triggered" + trigger);
    }
    
}
