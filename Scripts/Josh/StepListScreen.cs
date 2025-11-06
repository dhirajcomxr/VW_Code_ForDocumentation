using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StepListScreen : MonoBehaviour
{
    public ImageLabelList labelList;
    public ImageLabelList toolLabelList;
    public Text totalToolsText;
    public Steps steps;
     List<string> stepListLabels;
    public List<string> toolsRequired;
    public List<Sprite> toolImgs;
    public UILiveLabel stepLabel;
    public UILiveLabel toolScrollLabel;
    public ScreenLinker screenLinker;
    public Vector2 val;
    bool downloadTools = true;
    bool isLoaded = false;
  // [SerializeField]
    List<string> stepTitles;
    int total = -1;
    int totalTools = -1;
    public void IpVal(Vector2 v)
    {
  //      Debug.Log("V:" + v.ToString());
        val = v;
        if(stepLabel)
        stepLabel.UpdateLabel(""+(int)((1-v.y) * total));
    }
    public void ToolListScrollVal(Vector2 v)
    {
        //      Debug.Log("V:" + v.ToString());
        
        if (toolScrollLabel)
            toolScrollLabel.UpdateLabel("" + (int)((1 - v.y) * totalTools));
    }
    private void Reset()
    {

        Debug.Log("STEPLIST");
        steps = FindObjectOfType<Steps>();
        labelList = GetComponentInChildren<ImageLabelList>();
        if (!stepLabel)
            stepLabel = GetComponentInChildren<UILiveLabel>();
    }


    // Start is called before the first frame update
    private void OnEnable()
    {
        if(steps!=null)
        LoadData();
    }
   
    // Update is called once per frame
    void Update()
    {
        

    }

    //Load step list and tools
   public void LoadData()
    {
        Debug.Log("<color=red>[CALL]</color> Called StepList Load : loaded: "+isLoaded.ToString());
        if (!isLoaded)
        {
            if (labelList != null && steps != null)
            {
                BuildCombinedList();
                if (stepListLabels.Count > 0)
                {
                    //      labelList.Load(stepListLabels.ToArray());
                    labelList.LoadLabels(stepTitles.ToArray(), stepListLabels.ToArray());
                    toolLabelList.Load(toolImgs.ToArray(), toolsRequired.ToArray());
                    totalTools = toolsRequired.Count;
                   
                    total = stepListLabels.Count;
                }
                isLoaded = true;
            }
            else
                Debug.LogError("Steps Overview !");
        }
    }
    void SetTotalToolsText()
    {
        if (totalToolsText)
            totalToolsText.text = "Total Tools: " + totalTools;
    }

    //Load tools
    public void LoadToolDialogTools()
    {
        if (!(toolImgs.Count > 0 && toolsRequired.Count > 0))
        {
            Debug.LogError("Tool Imgs: " + toolImgs.Count + ", tools Reqd: " + toolsRequired.Count);
            toolsRequired = new List<string>();
            for (int i = 0; i < toolImgs.Count; i++)
            {
                if(toolImgs[i]!=null)
                toolsRequired.Add(toolImgs[i].name);
            }
         //   toolLabelList.Load(toolImgs.ToArray(), toolsRequired.ToArray());
        }
        Debug.Log("Tool Imgs: " + toolImgs.Count + ", tools Reqd: " + toolsRequired.Count);

        if ((toolImgs.Count > 0 && toolsRequired.Count > 0))
        {
            toolLabelList.Load(toolImgs.ToArray(), toolsRequired.ToArray());
            totalTools = toolImgs.Count;
            SetTotalToolsText();
            toolLabelList.gameObject.SetActive(true);
            Mixpanel.Track("Tool_List_Loaded","toolcount",toolImgs.Count);
        }
    }
    public void ClearData()
    {
        stepListLabels = new List<string>();
        stepTitles = new List<string>();
        toolsRequired = new List<string>();
        isLoaded = false;
    }
    void BuildCombinedList()
    {
        ClearData();
        VWSectionModule module = screenLinker.GetModuleManager().GetSelectedModule();
       
        if (downloadTools)
        {        
            if (module)
            {
                toolImgs = StepsEximProcessor.GetToolSpritesFrom(module.stepMain);
                if(toolImgs.Count>0)
                    for (int i = 0; i < toolImgs.Count; i++)
                    {
                        toolsRequired.Add(toolImgs[i].name);
                    }
                else
                {
                    toolsRequired = StepsEximProcessor.GetToolNamesFrom(module.assemblyFile, module.dismantlingFile);
                    Debug.Log("<color=red>[IMG]</color> Total Tools: " + toolsRequired.Count);
                    if (toolsRequired.Count > 0)
                    {
                        Debug.Log("<color=red>[IMG]</color>Downloading Tools: " + toolsRequired.Count);

                        screenLinker.GetScreenManager().SetLoadingState(true);
                        screenLinker.GetAssetLoader().DownloadImages(toolsRequired, "", ".png", OnToolImageDownloadComplete);
                    }
                    else
                    {
                        toolsRequired = StepsEximProcessor.GetToolNamesFromSpecialTools(module.stepMain);
                        if(toolsRequired.Count>0)
                        {
                            Debug.Log("<color=red>Got from special Tools: !</color>"+toolsRequired.Count);
                            screenLinker.GetScreenManager().SetLoadingState(true);
                            screenLinker.GetAssetLoader().DownloadImages(toolsRequired, "", ".png", OnToolImageDownloadComplete);

                        }
                    }
                }
            }
            else
                Debug.LogError("No Module Found!!");

        }
        AddStepsToStepList(steps.steps.ToArray());
        AddStepsToStepList(steps.assemblySteps.ToArray());

    }
    void OnToolImageDownloadComplete(List<Sprite> tools)
    {
        screenLinker.GetScreenManager().SetLoadingState(false);
        toolImgs = tools;
    }
    void AddStepsToStepList(Step[] stepArray)
    {
        
        for (int i = 0; i < stepArray.Length; i++)
        {
            Step curStep = stepArray[i];
            if (curStep.stepInstructions.Length < 2)
                stepListLabels.Add(curStep.locateObjectText);
            else
            stepListLabels.Add(curStep.stepInstructions);
            stepTitles.Add("" +( stepTitles.Count+1));
            if (!downloadTools)
            {
                string toolName = curStep.specialToolName;
                if (toolName.Length > 1)
                {
                    if (curStep.toolSprite != null)
                    {

                        for (int t = 0; t < curStep.toolSprite.Length; t++)
                        {
                            if (curStep.toolSprite[t] != null)
                                if (!toolImgs.Contains(curStep.toolSprite[t]))
                                {
                                    toolImgs.Add(curStep.toolSprite[t]);
                                    toolsRequired.Add(curStep.toolSprite[t].name);
                                }
                        }
                    }
                    else
                    if (!toolsRequired.Contains(toolName))
                    {
                        toolsRequired.Add(toolName);
                        toolImgs.Add(null);
                    }
                }
            }
        }
    }

    public void SetStepsReference(Steps gSteps)
    {
        //Debug.Log("<color=red>[CALL]</color> Called StepList Load via Ref");
        ClearData();
        steps = gSteps;
     
     //   LoadData();
    }
}
