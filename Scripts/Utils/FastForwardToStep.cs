using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastForwardToStep : MonoBehaviour
{
    public int step = 0,count=0;
    public bool startStop, fast;
    public Steps stepsScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!fast)
        {
            if (startStop)
            {
                fast = true;
                StartCoroutine(GotoNext());
               
            }
        }
        if (!startStop)
        {
            fast = false;
        }
    }

    IEnumerator GotoNext()
    {
        for(int i = stepsScript.currentStep; i < step; i++)
        {
            if (!fast)
                break;
            StepsManager.Instance.OnNext();
            yield return new WaitForSeconds(0.2f);
            StepsManager.Instance.OnNext();
            yield return new WaitForSeconds(0.2f);

        }
        startStop = false;
        
    }

}
