using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mixpanel;
public class StepManagerAssignments : MonoBehaviour
{
    public Steps stepMain;
    public VWModuleManager modulemanager;
    public UIScreenManager ScreenManager;
   // public VWSectionModule vwSelectionModule;

    public Text stepDesc;
    public GameObject caution_popup;
    public ImageLabelList caution_txt;
    public StepsTracker stepsTracker;
    public GameObject toolPopup;
    public Text toolName_txt;
    //public Sprite defaultToolSprite;
    public Image toolImage;
    public ImageLabelList toolList;
    public ImageLabelCollection slideShow;
    public GameObject torquePopup;
    public Text torqueValueTxt;
    public Text stepCounterText;
    public GameObject oneTimePane,infoPanel;
    public Button nextButton, previousButton,replayButton,pauseButton,PlayButton;
    public GameObject explodedViewResetUi;
    public GameObject explodedViewLabel;
    public ScreenLinker linker;
    public int preloadStep = -1;
    [SerializeField] bool doNotReload = false;
    [SerializeField] Text currentProcessText;
    StepsManager manager;
    private bool isAnimPause;

    //    [SerializeField] VWSectionModule vwSelectionModule;

    private void Reset()
    {
        //vwSelectionModule = FindObjectOfType<VWSectionModule>();
        stepMain = FindObjectOfType<Steps>();
   
    }
    public void ApplyAssignments() => SetAssignments();
    public void DoNotReload() => doNotReload = true;
    private void OnEnable()
    {
        manager = StepsManager.Instance;
        if (stepMain)
            stepMain.enabled = true;
        SetAssignments();
       
        if (stepMain)
        {
            if (!doNotReload)
            {
                if(stepMain.currentStep != 0)
                    stepMain.currentStep++;
                preloadStep = stepMain.currentStep;
                if (preloadStep < 0)
                    stepMain.Dismantling();
                manager.OnPrevious();
            }
            if (doNotReload)
                doNotReload = false;
        }
        if(linker.GetReferenceManager().GetSelectedName().Length>0)
        AppLogger.LogEventProcDesc(AppLogger.EventType.RNR, AppLogger.EventType.process_dism, "Started " + linker.GetReferenceManager().GetSelectedName());
        Mixpanel.Track("Switched_Process","process_type",AppLogger.EventType.process_dism);
        UpdateCurrentProcessText();
    }

    public void PreLoadStep(int s)
    {
        //Debug.Log("will preload with step " + s);
        preloadStep = s + 1;
    }

    public void PauseTicketBTN()
    {
        linker.GetScreenManager().SelectScreen(ScreenLinker.ScreenID.PROCESS_SELECTION);
        linker.PauseTicket();

    }
    public void PlayAssembly()
    {
        string mod = modulemanager.curModule.GetModuleName();
        if (mod.Contains("Brakes Bleeding") || mod.Contains("Bleeding"))
        {
            Debug.LogError("Brake Bleeding not have Assembly");
            ScreenManager.Toast(mod.ToString()+" doesn't have Assembly module step");
            return;
        }
        else
        {
            Debug.LogError(mod.ToString()+" have Assembly");
        }
       /* if (vwSelectionModule.GetModuleName().Contains("Brakes Bleeding "))
        {
            Debug.LogError("Brake Bleeding not have Assembly");
        }*/
        linker.GetScreenManager().SetLoadingState(true);
      
        UnPause();
        if (stepMain)
            stepMain.Assembly();

        if (linker.GetReferenceManager().GetSelectedName().Length > 0)
        {
            AppLogger.LogEventProcDesc(AppLogger.EventType.RNR, AppLogger.EventType.process_asm, "Started " + linker.GetReferenceManager().GetSelectedName());
            Mixpanel.Track("Switched_Process","process_type",AppLogger.EventType.process_asm);
        }
        Invoke("StopLoading", 1);
    }
    void StopLoading()
    {
        linker.GetScreenManager().SetLoadingState(false);
        UpdateCurrentProcessText();
        if (stepsTracker)
            stepsTracker.UpdateToCurrentStep();
    }
    public void PlayDismantling()
    {
        linker.GetScreenManager().SetLoadingState(true);

        UnPause();
        if (stepMain)
            stepMain.Dismantling();
        //  UpdateCurrentProcessText();
        //if (stepsTracker)
        //    stepsTracker.UpdateToCurrentStep();
        if (linker.GetReferenceManager().GetSelectedName().Length > 0)
        {
            AppLogger.LogEventProcDesc(AppLogger.EventType.RNR, AppLogger.EventType.process_dism, "Started " + linker.GetReferenceManager().GetSelectedName());
            Mixpanel.Track("Switched_Process","process_type",AppLogger.EventType.process_dism);
        }
        Invoke("StopLoading", 1);
    }
    private void UpdateCurrentProcessText()
    {
        if(stepMain)
        if (currentProcessText)
            currentProcessText.text = stepMain.currentProcess.ToString();

    }
    void SetAssignments()
    {
        if(!manager)
            manager = StepsManager.Instance;
        manager.stepDesc = stepDesc;
        manager.caution_txt = caution_txt;
        manager.caution_popup = caution_popup;
        manager.toolList = toolList;
        manager.toolPopup = toolPopup;
        manager.toolName_txt = toolName_txt;
        manager.toolImage = toolImage;
        manager.slideShow = slideShow;
        manager.torqueValueTxt = torqueValueTxt;
        manager.torquePopup = torquePopup;
        manager.nextBtn = nextButton.gameObject;
        manager.prevBtn = previousButton.gameObject;
        manager.oneTimeUse = oneTimePane;
        manager.infoPanel = infoPanel;
        nextButton.onClick.AddListener(manager.OnNext);
        previousButton.onClick.AddListener(manager.OnPrevious);
        nextButton.onClick.AddListener(UpdateStepCounter);
        previousButton.onClick.AddListener(UpdateStepCounter);
        pauseButton.onClick.AddListener(() => TogglePause(0));
        PlayButton.onClick.AddListener(() => TogglePause(1));
        Debug.Log("Toggle pause added");
        if (stepMain)
        replayButton.onClick.AddListener(stepMain.Replay);
    }
    public void Replay()
    {
        UnPause();
        stepMain.Replay();
    }
    public void Next()
    {
        manager.OnNext();
    }
    public void Previous()
    {
        UnPause();
        manager.OnPrevious();
    }
    public void UnPause()
    {
        Time.timeScale = 1;
    }

    public void ResetPlayPauseButton()
    {
        if (pauseButton != null && PlayButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            PlayButton.gameObject.SetActive(false);
        }
    }
    void TogglePause(float value)
    {
        stepMain.currentAnimator().speed = value;
    }
    private void UpdateStepCounter()
    {
        if (stepCounterText != null)
            if(stepMain)
            stepCounterText.text = "" + stepMain.currentStep;
    }
    private void OnDisable()
    {
        preloadStep = 0;
        nextButton.onClick.RemoveListener(manager.OnNext);
        previousButton.onClick.RemoveListener(manager.OnPrevious);
        replayButton.onClick.RemoveListener(stepMain.Replay);

        nextButton.onClick.RemoveListener(UpdateStepCounter);
        previousButton.onClick.RemoveListener(UpdateStepCounter);
        if (linker.GetReferenceManager().GetSelectedName().Length > 0)
            AppLogger.LogEventDesc(AppLogger.EventType.RNR, "Exited " + linker.GetReferenceManager().GetSelectedName());

    }
}
