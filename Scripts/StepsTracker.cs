using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StepsTracker : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    Steps stepMain;
    [SerializeField]
    StepsManager manager;
    [SerializeField]
    private int currentStep = -1,totalSteps = -1;
    [SerializeField]
    private Text stepCounterText;
    [SerializeField]
    private GameObject processCompletePanel;
    [SerializeField]
    GameObject torxImg, allenKeyImg;
    [SerializeField]
    private Image fillImage;
    [Range(0,1f)]
    [SerializeField] float completionPercent = 0.0f;
   [SerializeField] bool enableNext = false, isRnrClosed;

    public GameObject NextButton, PreviousButton;
    private void OnEnable()
    {
        isRnrClosed = true;
        enableNext = false;
        manager = StepsManager.Instance;
        VWReferencesManager references = FindObjectOfType<VWReferencesManager>();
       stepMain= references.GetSteps();
        UpdateTracker();
  
        UpdateToCurrentStep();
        UpdateTracker();

        //if (manager != null)
        //{
        //    StepsManager.NextStep += UpdateToCurrentStep;
        //    StepsManager.PreviousStep += UpdateToCurrentStep;
        //}
        if (stepMain != null)
            stepMain.stepUpdated += UpdateToStep;
       
    }
    private void OnDisable()
    {

        if (stepMain != null)
            stepMain.stepUpdated -= UpdateToStep;

        if (isRnrClosed)
        {
            isRnrClosed = false;
            Mixpanel.Track("R&R_Closed");
        }

    }
   public int prevStep = -1;
    private void UpdateToStep(int stepNum)
    {
        totalSteps = stepMain.GetTotalSteps();
        currentStep = stepNum;
      
        completionPercent = stepNum * 1f / totalSteps;
        UpdateStepCounter();
        UpdateToolHeadImages();
        prevStep = currentStep;

        if(prevStep >= totalSteps-1 && processCompletePanel.activeInHierarchy)
        {
            Mixpanel.Track(stepMain.currentProcess.ToString() + " Completed");
        }
    }
    public void UpdateToCurrentStep()
    {
        if (enableNext)
        {
            enableNext = false;
        }
        currentStep = 0;
        UpdateTracker();
        UpdateStepCounter();
        UpdateToolHeadImages();
      
    }
    void UpdateStepCounter()
    {
        if (stepCounterText != null)
            if (stepMain)
            stepCounterText.text = "" + currentStep;
        if (fillImage)
            fillImage.fillAmount = completionPercent;

        if (currentStep == 0)
        {
            PreviousButton.SetActive(false);
        }
        else
        {
            PreviousButton.SetActive(true);
        }

        if (processCompletePanel)
        {
            
            processCompletePanel.SetActive(enableNext);
           
            //    if (processCompletePanel.activeSelf)
            //    {
            if (enableNext)
            {
                NextButton.SetActive(false);
                enableNext = false;
                prevStep = -1;
            }
        //    }
        }
        if (currentStep >= (totalSteps - 1))
        {
            enableNext = true;
            Debug.Log("<color=blue>Will show Panel Next</color> cur : " + currentStep + " total: " + totalSteps);
           // if(AppLogger.EventType.process_asm = )
        }
        else if (currentStep != (totalSteps - 1))
        {

            NextButton.SetActive(true);
            processCompletePanel.SetActive(enableNext);
            enableNext = false;
        }
 
    }

    void UpdateToolHeadImages()
    {
        if (stepMain != null) {
            Step curStep;
            curStep = stepMain.GetCurrentStep();
            string locateTxt = curStep.locateObjectText.ToLower();
            string instTxt = curStep.stepInstructions.ToLower();
            torxImg.SetActive(locateTxt.Contains("torx") || instTxt.Contains("torx"));
            allenKeyImg.SetActive(locateTxt.Contains("allen") || instTxt.Contains("allen"));
           }
    }

    private void UpdateTracker()
    {
        if (stepMain != null)
        {
            currentStep = stepMain.currentStep;
            totalSteps = stepMain.GetTotalSteps();
            completionPercent = currentStep * 1f / totalSteps;
        }
    }
  
}
