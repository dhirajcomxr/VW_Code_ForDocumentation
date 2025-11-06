using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLockUnlockStatusOfSteps : MonoBehaviour
{
    public Steps stepsScript;
    public int dismLockCount;
    public int assLockCount;
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("Update Locked and Unlocked StepText")]
    public void UpdateLockedState()
    {
        stepsScript.AddNumbers();
        List<Step> dism = stepsScript.steps;
        List<Step> assm = stepsScript.assemblySteps;
        List<Step> temp1 = new List<Step>();
        List<Step> temp2 = new List<Step>();
        dismLockCount = 0;
        assLockCount = 0;
        Debug.Log("---------Updating dism locked state----------");
        for (int i = 0; i < stepsScript.steps.Count; i++)
        {
            temp1.Add(dism[i]);
        }
        Debug.Log("Temp dism steps count:" + temp1.Count);
        for (int i = 0; i < temp1.Count; i++)
        {
            Debug.Log(temp1[i].debugStepNum + " " + temp1[i].locateObjectText + " islocked:" + temp1[i].isLocked);
            if (temp1[i].isLocked)
            {
                if (!temp1[i].locateObjectText.Contains("(Lock) "))
                {
                    temp1[i].locateObjectText = "(Lock) " + temp1[i].locateObjectText;
               
                }
                dismLockCount++;
            }
            else
            {
                
                if (temp1[i].locateObjectText.Contains("(Lock) "))
                {
                    temp1[i].locateObjectText = temp1[i].locateObjectText.Replace("(Lock) ", "");
                }
            }
            Debug.Log(temp1[i].debugStepNum + " Updated: " + temp1[i].locateObjectText + " islocked:" + temp1[i].isLocked);
        }
        stepsScript.steps = temp1;

        Debug.Log("---------Updating assm locked state----------");
        for (int i = 0; i < stepsScript.assemblySteps.Count; i++)
        {
            temp2.Add(assm[i]);
        }
        Debug.Log("Temp assm steps count:" + temp2.Count);
        for (int i = 0; i < temp2.Count; i++)
        {
            Debug.Log(temp2[i].debugStepNum + " " + temp2[i].locateObjectText + " islocked:" + temp2[i].isLocked);
            if (temp2[i].isLocked)
            {
                if (!temp2[i].locateObjectText.Contains("(Lock) "))
                {
                    temp2[i].locateObjectText = "(Lock) " + temp2[i].locateObjectText;
                  
                }
                assLockCount++;
            }
            else
            {
                if (temp2[i].locateObjectText.Contains("(Lock) "))
                {
                    temp2[i].locateObjectText = temp2[i].locateObjectText.Replace("(Lock) ", "");
                }
            }
            Debug.Log(temp2[i].debugStepNum + " Updated: " + temp2[i].locateObjectText + " islocked:" + temp2[i].isLocked);
        }
        stepsScript.assemblySteps = temp2;

    }

}
