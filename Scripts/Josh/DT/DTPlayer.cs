using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTPlayer : MonoBehaviour
{
    [SerializeField] UIScreenManager screenManager;

    [SerializeField] TextAsset curDtFile;
  [SerializeField]  DiagnosticTreeManager.TextPanel display;
    [SerializeField] DiagnosticTreeManager dtManager;
    [SerializeField] DiagnosticStep curStep;
   
    [SerializeField] DiagnosticTree tree;
    public int totalSteps = 0, counter = 0;
    public int curr = 0;
    bool hasScan = false;
    // Start is called before the first frame update
    private AssetLoader loader;
    [SerializeField] DTInputModule inputModule;
    List<string> activityNumbers;
    public bool useRemoteLoading;

    private void Reset()
    {
        dtManager = FindObjectOfType<DiagnosticTreeManager>();

        if (dtManager)
        {
            display = dtManager.display;
            Debug.Log("Copied Settings..");
        }
     
    }
    private void OnEnable()
    {
        inputModule.SetInputEvaluator(OnInput);
        DiagnosticStepOutputEvaluator.onDisplayText = DisplayResultDialog;
        inputModule.onInputError += OnInputError;
        inputModule.onInput += dtManager.LogOnAnswer;
       
        //   display.instruction
    }
    private void Start()
    {
        display.instruction.margin = new Vector4(1, 1, 0, 0);
    }
    private void OnDisable()
    {
        DiagnosticStepOutputEvaluator.onDisplayText = null;
    }
    void DisplayResultDialog(string msg)
    {
        Debug.Log("Recieved :" + msg);
        screenManager.GetDialog().Show("Result",msg);
    }
    void OnInputError(string msg)
    {
        screenManager.Toast(msg);
    }
    public void ShowCurrStep()
    {
        hasScan = false;
        ShowStep(curr);
    }
    public DiagnosticStep GetCurrentStep() => curStep;
    public void Load(TextAsset asset)
    {
       tree= DiagnosticTreeFactory.LoadFromFile(asset);
  
       //tree = DiagnosticTreeFactory.LoadFromString(asset.text);
        Debug.Log("<Color=red>Asset Load From File</Color>" + asset.text);
        totalSteps = tree.steps.Count;
        if (tree != null)
        {
            activityNumbers = new List<string>();
            for (int i = 0; i < tree.steps.Count; i++)
            {
                Debug.Log("<Color=red>Asset Load From File    .............</Color>");
                activityNumbers.Add(tree.steps[i].activity_id);
            }
            ShowStep(0);
        }
        else
            Debug.LogError("Tree did not load");
       
    }
    public void OnInput(DiagnosticStep step)
    {
        DiagnosticStepOutputEvaluator.onDisplayText = DisplayResultDialog;
        string nextStepId = DiagnosticStepOutputEvaluator.Evaluate(step);
        SetNextStep(nextStepId);
    }
    public void SetupInputForStep(DiagnosticStep step)
    {
        inputModule.SetInputFor(step);
        
    }
    public DeviceCameraControl DeviceCamera;
    public void SetNextStep(string nextId)
    {
       // DeviceCamera.DTStepNum = System.Convert.ToInt32(nextId);
        Debug.Log("Set Next Step   "+nextId);
        DiagnosticStepOutputEvaluator.onDisplayText = DisplayResultDialog;
        int nid = -1;
        if (nextId != "")
            for (int i = 0; i < activityNumbers.Count; i++)
            {
                if (activityNumbers[i] == nextId)
                {
                    curr = i;
                    nid = i;
                }
                //   ShowStep(i);
            }
        else
        {
            curr = activityNumbers.Count;
            //++curr;
        }

        if (nid < 0)
        {
            curr = activityNumbers.Count;
            // ++curr;
        }

        if (hasScan)
        {
            display.scan_dialog.SetActive(true);
            dtManager.ScanOptionSelect((int)GetCurrentStep().scanOption);
        }
        else
            ShowStep(curr);
        Debug.Log("Next: " + nextId);
    }
    // public GameObject CameraPanal;
    bool OnLastStepOpenEndScreen;
    public bool dtcompleted = false;
    //public bool alreadySubmitted = false;
    public void ShowStep(int counter)
    {
        DeviceCamera.DTStepNumber = counter;

        Debug.Log("Step Counter"+counter);
        if (counter == totalSteps - 3)
        {
            if (inputModule.YesNoMethod == true)
            {
                Debug.Log("YEs   No  ...........................");
                
            }
         
        }


        if (counter >= (totalSteps))
        {
            Debug.Log("Last Step call........................." + counter);
            screenManager.SelectScreen(ScreenLinker.ScreenID.DT_END_SCREEN);
            Debug.Log("Diagnostic exited for " + dtManager.dynamicModuleName);
            AppLogger.LogEventDesc(AppLogger.EventType.DIAG, "End " + dtManager.dynamicModuleName);
            dtcompleted = true;
            //  CameraPanal.SetActive(false);
        }
        else
        {
            dtcompleted = false;
        }

       

        if (counter < (totalSteps))
        {
            curStep = tree.steps[counter];
            float progress = (counter + 1) * 1f / totalSteps;
            Debug.Log("Cur DialogBox" + curStep.isDialog);
            if(curStep.isDialog)
            {
                Debug.Log("<color=blue>[DT]</color>IS Dialog: "+curStep.instruction+", Next is "+curStep.output.outputs[0].name+":"+curStep.output.outputs[0].outputId);
                screenManager.GetDialog().ShowDirect(curStep.dialogData.name, curStep.dialogData.val,"OK",()=>SetNextStep(curStep.output.outputs[0].outputId));
            }
            else
            { 
            if (display.stepFill)
                display.stepFill.fillAmount = progress;
            if (display.progress != null)
                display.progress.SetValueWithoutNotify(progress);
       /*     if (display.stageLabel != null)
                if (curStep.stage.Length > 0)
                    display.stageLabel.text = curStep.stage;
                else
                    display.stageLabel.gameObject.SetActive(false);*/
            display.stepId.text = "" + (counter + 1);//+ ":" + totalSteps;
            display.question.text = curStep.question;

            display.instruction.text = curStep.instruction;
            display.activity.text = curStep.activity;
            display.scan_data.text = curStep.scan;
            display.caution_notes.transform.parent.gameObject.SetActive(curStep.caution_notes != "");
            display.caution_notes.text = curStep.caution_notes;
            display.uploadInfo.transform.parent.gameObject.SetActive(curStep.scan != "");
            display.uploadInfo.text = curStep.scan.Replace(System.Environment.NewLine, "");
            display.specs.transform.parent.gameObject.SetActive(curStep.specs != "");
            display.specs.text = curStep.specs;

            hasScan = curStep.scan != "" ? true : false;
            inputModule.SetInputFor(curStep);
            for (int i = 0; i < display.images.Length; i++)
            {
                    if (curStep.imgs != null)
                    {
                        if( curStep.imgs.Length < 1)
                        {
                            foreach (var displayImg in display.images)
                            {
                                displayImg.gameObject.SetActive(false);
                            }
                        }
                        if (i < curStep.imgs.Length)
                        {
                            Debug.Log("DI:" + display.images.Length + ", CS:" + curStep.imgs.Length);
                            if (display.images[i] != null && curStep.imgs[i] != null)
                                SetImage(display.images[i], curStep.imgs[i]);
                        }
                    }
            }

            if (curStep.part_number != "")
            {
                display.part_number.text = curStep.part_number;
                string[] parts = curStep.part_number.Split(';', ':');
                //          highlighter.Highlight(parts);
            }
        }
            //else
            //    highlighter.RemoveHighLight();
        }
        else
            Debug.Log("Play Next Module");
    }

    void loadCurDTstep()
    {

    }
    void SetImage(ImageLabelCollection imgDisplay, string imgList)
    {
        Debug.Log("hjvgdvjbvjbvhv");
        imgList = imgList.Replace(":;:", ",");
        string[] st = imgList.Split(',');
        List<Sprite> sp = new List<Sprite>();
        for (int i = 0; i < st.Length; i++)
        {
            //   Debug.Log("ST:"+st[i]+"L:"+st.Length+" i:"+i,imgDisplay.gameObject);
            if (st[i].Length > 0)
                sp.Add(GetImage(st[i]));
        }
  
        imgDisplay.Load(sp.ToArray());

    }
    public List<DiagnosticStep> GetSteps() => tree.steps;
    public Sprite GetImage(string imageName)
    {
        if (useRemoteLoading)
        {
            return dtManager.GetImage(imageName);
        }
        else
        {
            char dirSeperator = System.IO.Path.DirectorySeparatorChar;
            Sprite s = null;
            if (imageName.Length > 0)
                s = Resources.Load<Sprite>("IMAGES" + dirSeperator + "DT IMAGES" + dirSeperator + imageName);
            if (s == null)
                Debug.Log("No Sprite set for " + imageName);
            return s;
        }
    }
}
