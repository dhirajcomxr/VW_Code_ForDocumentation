using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Animations
{
    public Animator animator;
    public string trigger;
}
public class AnimTriggerHelper : MonoBehaviour
{
    public GameObject[] objectsToDisable;
    public GameObject[] objectsToEnable;
    public Animations[] animations;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetAnimationsToDefault()
    {
        foreach(var v in animations)
        {
            //v.animator.keepAnimatorControllerStateOnDisable = false;
            //animator.enabled = false;
            v.animator.enabled = true;
            v.animator.Rebind();
            v.animator.speed = 1f;
    
            //v.animator.StopPlayback();

            //foreach (var p in v.animator.parameters)
            //{
            //    if (p.type == AnimatorControllerParameterType.Trigger)
            //    {
            //        v.animator.ResetTrigger(p.name);
            //    }
            //}
            //v.animator.ResetTrigger("AddBasePlate");
            //v.animator.SetTrigger(v.trigger);

            //Debug.Log("Anim triggered" + v.trigger);
            //v.animator.enabled = false;
        }
      
    }

    public void DisableObjects()
    {
        foreach(GameObject o in objectsToDisable)
        {
            o.SetActive(false);
        }
    }
    public void EnableObjects()
    {
        foreach (GameObject o in objectsToEnable)
        {
            o.SetActive(true);
        }
    }
}
