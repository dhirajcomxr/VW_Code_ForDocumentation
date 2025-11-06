using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UpcomingSteps : MonoBehaviour
{
   [SerializeField] Steps steps;
    [SerializeField] StepsManager stepsManager;
    [SerializeField] Text stepCounterText;
    [SerializeField] Text[] nextStepsElement;
    public List<string> nextSteps;
    List<Step> nextStepsBlock;
    public int numberOfPreviousSteps, numberOfNextSteps=3;
    int cur;
    // Start is called before the first frame update
    private void Reset()
    {
        steps = FindObjectOfType<Steps>();
        stepsManager = FindObjectOfType<StepsManager>();
    }
    private void OnEnable()
    {
        StepsManager.NextStep += UpdateBlock;
        StepsManager.PreviousStep += UpdateBlock;
        UpdateBlock();
    }
    private void OnDisable()
    {
        StepsManager.NextStep -= UpdateBlock;
        StepsManager.PreviousStep -= UpdateBlock;
    }
    public void UpdateBlock()
    {
        init();
        DisplayValues();
    }
    private void init()
    {
        foreach (var item in nextStepsElement)
        {
            item.text = "";
        }

        nextSteps = new List<string>();
         cur = steps.currentStep;
       nextStepsBlock = new List<Step>();
        int startVal = cur;
        if (cur < 0)
            startVal = 0;
        int totalVals = numberOfNextSteps +1 ;
        int totalSteps = steps.GetTotalSteps();

        if ((startVal + totalVals) >= totalSteps)
            totalVals = totalSteps - startVal;

     //   Debug.Log("START VAL:" + startVal + ", TOTALVAL:" + totalVals + ", TotalStep:" + totalSteps);
        nextStepsBlock = steps.GetCurrentStepList().GetRange(startVal, totalVals);

    }
   
    void DisplayValues()
    {
        if (stepCounterText != null)
            stepCounterText.text =""+ (cur);
        for (int i = 0; i < numberOfNextSteps; i++)
        {
            Step nsb = null;
            if (i < nextStepsBlock.Count)
                nsb = nextStepsBlock[i];
            if (nsb != null)
            {
              
                nextSteps.Add(nsb.locateObjectText);
                nextSteps.Add(nsb.stepInstructions);
            }
            else
            {
                nextSteps.AddRange(new string[] { null, null });
            }
        }
        int startIndex = 3;
        if (steps.PartLocated())
            startIndex = 2;

        for (int i = 0; i < nextStepsElement.Length; i++)
        {
            GameObject par = nextStepsElement[i].transform.parent.gameObject;
            if (nextSteps[i + startIndex] != null)
            {
                par.SetActive(true);
                nextStepsElement[i].text = nextSteps[i + startIndex];//+ System.Environment.NewLine;
            }
            else
                par.SetActive(false);
        }
    }
    // Update is called once per frame
   
}
