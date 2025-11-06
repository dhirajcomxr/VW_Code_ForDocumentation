using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TimerEvents : MonoBehaviour
{
    [SerializeField] UnityEvent timerEvents;
    [SerializeField] float timer;
    [SerializeField] bool disableAfterEvent=false;
   [SerializeField] bool debugOnEnable = false;
    float callTime;

    bool eventsCalled = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (debugOnEnable)
            Debug.Log(gameObject.name + "is enabled",gameObject);
        callTime = Time.time + timer;
        eventsCalled = false;
    }
    void PerformEvents()
    {
        eventsCalled = true;
       
        timerEvents.Invoke();
        callTime = 0;
        if (disableAfterEvent)
            gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if(!eventsCalled)
        if (Time.time >= callTime)
        {
            PerformEvents();
        }
    }
}
