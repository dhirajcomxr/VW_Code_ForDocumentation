using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]

public class StepsEximProcessor : MonoBehaviour
{
    [Tooltip("The Object with the steps script component.")]
    [SerializeField] string testString;
   [SerializeField] bool now = false;
    [SerializeField] public Steps main;
    [SerializeField] bool detectParentRootMismatch = false;
   [Tooltip("Use this to differentiate 1L vs 1.5L etc in the save file Name")]
    [SerializeField] string saveFilePrefix="SAVED";
 //  [Tooltip("Load using fileName")] [SerializeField] string loadFileName;
   [Tooltip("Drag the csv file here")] [SerializeField] TextAsset loadFile;
   public string eMsg;
    [SerializeField] bool saveNow = false,loadNow=false;
     List<string> st;
    private string assemblyStepsString = "ASSEMBLY", dismantlingStepsString = "DIS";
    public bool debugMode = false;
    public static string SAVE_FOLDER_NAME="Dump",TYPE_ASSEMBLY="Assembly Steps",TYPE_DISMANTLING="Dismantling Steps";
//    [HideInInspector]
    public bool editor = true;
 // [HideInInspector]// un/-comment this line to view the current imported step list
    public List<Step> alist;
    public Sprite[] allToolSprites;
   [SerializeField] private TextAsset asm, dism;
    public bool HasStepReference() => main ??false;
    [System.Serializable]
   class HeaderNames
    {
        // All Header Name References
     public string serial_number = "SR",
            locate_part_instruction = "LOCATE",
            step_instruction = "STEP",
            is_locked="LOCK",
            animated_object="LINK",
            anim_trigger="TRIGGER",
            look_at_point= "LOOK",
            object_to_highlight= "HIGHLIGHT",
            object_to_disable="DISABLE",
            object_to_enable="ENABLE",
            caution_notes = "CAUTION",
            specs="SPEC",
            override_camera_pos = "CAMERA",
            tool_name="TOOL USED",
            tool_image = "IMAGE",
            audio = "AUDIO"  
            ;
       
    }

   // [HideInInspector]
    HeaderNames refHeaders;// Reference Headers to compare with headers on the file
     [SerializeField] 
    HeaderNames headers;// these headers change to match the file contents
    ObjectReferenceMapper mapper;
  [System.Serializable]
   public class StepData
    {
        //public Animator animator;
        //public GameObject[] objectsToHighlight, objectsToEnable, objectsToDisable;

        //public Transform cameraOverride, lookAtPoint;
        //public Sprite[] toolImgs;
        public int animatorId, cameraPosId, lookAtPosId;
        public int[] objToHighlightIds,objectsToEnableIds,objectsToDisableIds;
        public static StepData GetStepDataFor(Step st)
        {
            
            StepData stepData = new StepData();
            if(st.animatedObject!=null)
            stepData.animatorId = ObjectReferenceMapper.GenerateId(st.animatedObject.gameObject);
           
            if (st.objsToEnable.Length > 0)
                Debug.Log("ob to enable on " + st.locateObjectText,st.objsToEnable[0]);
            stepData.objectsToEnableIds = ObjectReferenceMapper.GenerateId(st.objsToEnable);
            stepData.objectsToDisableIds = ObjectReferenceMapper.GenerateId(st.objsToDisable);
            stepData.objToHighlightIds = ObjectReferenceMapper.GenerateId(st.objsToHighlight);
            // stepData.toolImgs = st.toolSprite;
            if (st.overrideCameraPosition != null)
                stepData.cameraPosId = ObjectReferenceMapper.GenerateId(st.overrideCameraPosition.gameObject);
            else
                Debug.Log("Camera Pos is Null for" + st.locateObjectText);
           if(st.lookAtPoint!=null)
            stepData.lookAtPosId = ObjectReferenceMapper.GenerateId(st.lookAtPoint.gameObject);
            return stepData;
        }
        //public static  StepData GetStepDataFor(Step st)
        //{
        //    StepData stepData = new StepData();
        //    stepData.animator = st.animatedObject;
        //    stepData.objectsToEnable = st.objsToEnable;
        //    stepData.objectsToDisable = st.objsToDisable;
        //    stepData.objectsToHighlight = st.objsToHighlight;
        //    stepData.toolImgs = st.toolSprite;
        //    stepData.cameraOverride = st.overrideCameraPosition;
        //    stepData.lookAtPoint = st.lookAtPoint;
        //    return stepData;
        //}
        public static Step GetStepFrom(Step st, StepData sd)
        {
            StepData stepData = new StepData();
            Step result = st;
            ObjectReferenceMapper mapper = ObjectReferenceMapper.Instance;
            GameObject animG = mapper.GetGameObjectFromId(sd.animatorId);
            if (animG)
                st.animatedObject = animG.GetComponent<Animator>();
            else
                Debug.Log("No Animator");
            st.objsToEnable = mapper.GetGameObjectsFromIds(stepData.objectsToEnableIds);
            st.objsToDisable = mapper.GetGameObjectsFromIds(stepData.objectsToDisableIds);
            st.objsToHighlight = mapper.GetGameObjectsFromIds(stepData.objToHighlightIds);
            //  st.toolSprite = stepData.toolImgs;
            GameObject co = mapper.GetGameObjectFromId(stepData.cameraPosId);
            if (co)
                st.overrideCameraPosition = co.transform;
            else
                Debug.Log("No COP");
            GameObject lAt = mapper.GetGameObjectFromId(stepData.lookAtPosId);
            if (lAt)
                st.lookAtPoint = mapper.GetGameObjectFromId(stepData.lookAtPosId).transform;
            else
                Debug.Log("No Lookat Pt");
            return result;
        }
        //public static Step GetStepFrom(Step st,StepData sd)
        //{
        //    StepData stepData = new StepData();
        //    Step result = st;
          
        //    st.animatedObject = stepData.animator;
        //    st.objsToEnable = stepData.objectsToEnable;
        //    st.objsToDisable = stepData.objectsToDisable;
        //    st.objsToHighlight = stepData.objectsToHighlight;
        //    st.toolSprite = stepData.toolImgs;
        //    st.overrideCameraPosition = stepData.cameraOverride;
        //    st.lookAtPoint = stepData.lookAtPoint;
        //    return result;
        //}

    }
    
    string GetStepDataStringFor(Step st)
    {

        StepData stepData = StepData.GetStepDataFor(st);
        string ip = JsonUtility.ToJson(stepData);
      return  TextUtils.Base64Encode(ip);
    }
    StepData GetStepData(string ip)
    {
        string bIp = TextUtils.Base64Decode(ip);
        StepData sd = JsonUtility.FromJson<StepData>(bIp);
        //if(sd.objectsToEnableIds!=null && sd.objToHighlightIds!=null)
        //Debug.Log("oe:" + sd.objectsToEnableIds.Length + " | oh:" + sd.objToHighlightIds.Length);
        return sd;
    }
    Step ModifyStepFromString(Step ipStep,string voiceData)
    {
        Step result = ipStep;
        if (voiceData.Length > 0)
        {
            string bIp = TextUtils.Base64Decode(voiceData);
            StepData sd = JsonUtility.FromJson<StepData>(bIp);
           result= StepData.GetStepFrom(ipStep, sd);
        }
        return result;
    }
    private enum LoadDataType
    {
        Assembly,Dismantling,Error,
    }

   // public List<Dictionary<string, object>> data { get; private set; }

    private int totalSteps;
    public bool HasFileToLoad() => loadFile!=null;
    void Reset()
    {
        main = FindObjectOfType<Steps>();
    }
    // Start is called before the first frame update
    void Awake()
    {
       
        if (main == null)
            main = FindObjectOfType<Steps>();
    }

    private void Start()
    {
        if (main != null)
        {
            foreach (Step steps in main.steps)
            {
                steps.toolSprite = null;
                steps.toolSprite = new Sprite[0];
            }
            foreach (Step steps in main.assemblySteps)
            {
                steps.toolSprite = null;
                steps.toolSprite = new Sprite[0];

            }
        }
        LoadDismToolSprites();
        LoadAsmToolSprites();
    }

    void ExportStepsToCSV()
    {
        ExportDismantlingSteps();
        ExportAssemblySteps();

    }

   public void ExportDismantlingSteps()
    {
        if (HasDismantlingSteps())
            ExportStepsToCSV(main.steps, saveFilePrefix + "-" + TYPE_DISMANTLING + "-");
    }
    [ContextMenu("Populate ToolSprites To Asm Steps from toolName")]
    public void LoadAsmToolSprites()
    {
        ScanToolFolder();
        if (main)
        {
            main.assemblySteps = ProcessToolNames(main.assemblySteps,new List<Sprite>( allToolSprites));
        }
    }
    [ContextMenu("Populate ToolSprites To Dism Steps from toolName")]
    public void LoadDismToolSprites()
    {
        ScanToolFolder();
        if (main)
        {
            main.steps = ProcessToolNames(main.steps,new List<Sprite>( allToolSprites));
        }
    }

    //SVS
    [ContextMenu("Replace ToolSprites To ASSEMBLY Steps from toolName jpg to png")]
    public void ReplaceAssmToolSprites()
    {
        Sprite[] pngSprites = ScanToolFolderForPng();
        if (main)
        {
            //Debug.Log(allToolSprites[0].name);
            main.assemblySteps = ReplaceToolNames(main.assemblySteps, new List<Sprite>(pngSprites));
        }
    }
    //SVS
    [ContextMenu("Replace ToolSprites To DISMENTAL Steps from toolName jpg to png")]
    public void ReplaceDismToolSprites()
    {
       Sprite[] pngSprites= ScanToolFolderForPng();
        if (main)
        {
            //Debug.Log(allToolSprites[0].name);
            main.steps = ReplaceToolNames(main.steps, new List<Sprite>(pngSprites));
        }
    }


    List<Step> ProcessToolNames(List<Step> stl, List<Sprite> allTools)
    {
        int totalFound = 0,notFound=0;
        List<Step> result = new List<Step>();
        for (int s = 0; s < stl.Count; s++)
        {
            Step curStep = stl[s];

            List<Sprite> curStepToolSpriteList = new List<Sprite>();
            if (curStep.toolSprite.Length < 1)
                if (curStep.specialToolName.Length > 1)
                {
                    string spName = curStep.specialToolName.ToLower();
                    spName = spName.Replace(" and ", ",")
                  .Replace(" with ", ",").Replace("/", "_");
                    spName = spName.Trim();
                    for (int i = 0; i < allToolSprites.Length; i++)
                        if (allToolSprites[i] != null)
                        {
                            if (spName.Contains("spanner") && !spName.Contains("+") && !spName.Contains("and") && !spName.Contains(",") && !spName.Contains("_") && !spName.Contains("&") && !spName.Contains(";") && !spName.Contains("/"))
                            {
                                if(spName == allToolSprites[i].name.ToLower())
                                {
                                    curStepToolSpriteList.Add(allToolSprites[i]);
                                    totalFound++;
                                }
                            }
                            else if (spName.Contains(allToolSprites[i].name.ToLower()))
                            {
                                curStepToolSpriteList.Add(allToolSprites[i]);
                                totalFound++;
                            }
                        }
                    if (curStepToolSpriteList.Count > 0)
                        curStep.toolSprite = curStepToolSpriteList.ToArray();
                    else
                    {
                        notFound++;
                        Debug.Log("step: " + s + ":Tool Name <color=yellow>" + spName + "</color> not found, with instruction:" + curStep.stepInstructions);
                    }
                    }

            result.Add(curStep);
        }
        //Debug.Log("Found " + totalFound + " tools, "+notFound+" not found.");
        return result;
    }
    //SVS -Function to replce jpg with png tools
    List<Step> ReplaceToolNames(List<Step> stl, List<Sprite> allTools)
    {
        int totalFound = 0, notFound = 0;
        List<Step> result = new List<Step>();
        for (int s = 0; s < stl.Count; s++)
        {
            Step curStep = stl[s];
            //List<Sprite> curStepToolSpriteList = new List<Sprite>();
            if (curStep.toolSprite.Length > 0)
            {
                for (int i = 0; i < curStep.toolSprite.Length; i++)
                {
                    //Debug.Log("Sprite length " + allTools.Count);
                    if (curStep.toolSprite[i] != null)
                    {
                        Sprite _ssprite = allTools.Find(x => x.name.ToLower() == curStep.toolSprite[i].name.ToLower());
                        if (_ssprite != null)
                        {
                            curStep.toolSprite[i] = _ssprite;
                            Debug.Log("Step: " + curStep.debugStepNum + " sprite replace : " + _ssprite.name);
                            totalFound++;
                        }
                        else
                        {
                            Debug.Log("<color=yellow>Step: " + curStep.debugStepNum + " Not Found: " + curStep.toolSprite[i].name + "</color>");
                            notFound++;
                        }
                    }
                    else
                    {
                        Debug.Log("<color=red>Step: " + curStep.debugStepNum + " Null </color>");
                    }
                }
            }
            result.Add(curStep);
        }
        Debug.Log("Found " + totalFound + " tools, " + notFound + " not found.");
        return result;
    }

    //SVS
    public Sprite[] ScanToolFolderForPng()
    {
        //allToolSprites = LoadIcons("Images" + System.IO.Path.DirectorySeparatorChar + "TOOL" + System.IO.Path.DirectorySeparatorChar +"PNG");
        Sprite[] LoadIconsPng(string resPath)
        {
            Sprite[] Icons; // icons array
            object[] loadedIcons = Resources.LoadAll(resPath, typeof(Sprite));
            Icons = new Sprite[loadedIcons.Length];
            //this
            for (int x = 0; x < loadedIcons.Length; x++)
            {
                Icons[x] = (Sprite)loadedIcons[x];
            }
            //or this
            //loadedIcons.CopyTo (Icons,0);
            return Icons;
        }
        return LoadIconsPng("Images" + System.IO.Path.DirectorySeparatorChar + "TOOL" + System.IO.Path.DirectorySeparatorChar + "PNG");
        //Sprite[] SetImages(string imageName)
        //{
        //    List<Sprite> result = new List<Sprite>();
        //    string modImgName = imageName.Replace(":;:", ",");
        //    modImgName = modImgName.Replace(" & ", ",").Replace(" and ", ",")
        //          .Replace(" with ", ",").Replace(" + ", ",").Replace("/", "_");
        //    string[] allImgs = modImgName.Split(',');

        //    char dirSeperator = System.IO.Path.DirectorySeparatorChar;
        //    for (int i = 0; i < allImgs.Length; i++)
        //    {

        //        totalTools++;
        //        Sprite nextSprite = Resources.Load<Sprite>("Images" + dirSeperator + "TOOL" + dirSeperator + "PNG" + dirSeperator + allImgs[i]);
        //        if (nextSprite != null)
        //        {
        //            result.Add(nextSprite);
        //            //       Debug.Log("<color=green>Tool Sprite:</color> [" + allImgs[i] + "] found");
        //            foundTools++;
        //        }
        //        else
        //        {

        //            Debug.Log("<color=yellow>Tool Sprite:</color> [" + allImgs[i] + "] not found");
        //        }
        //    }

        //    return result.ToArray();
        //    //    display.image.gameObject.SetActive(imageName.Length > 0 ? true : false);
        //}
    }




    public void ScanToolFolder()
    {
        allToolSprites = LoadIcons("Images" + System.IO.Path.DirectorySeparatorChar + "TOOL" + System.IO.Path.DirectorySeparatorChar + "PNG");
    Sprite[] LoadIcons(string resPath)
    {
    Sprite[] Icons; // icons array
    object[] loadedIcons = Resources.LoadAll(resPath, typeof(Sprite));
        Icons = new Sprite[loadedIcons.Length];
        //this
        for (int x = 0; x < loadedIcons.Length; x++)
        {
            Icons[x] = (Sprite)loadedIcons[x];
        }
            //or this
            //loadedIcons.CopyTo (Icons,0);
            return Icons;
    }
        Sprite[] SetImages(string imageName)
        {
            List<Sprite> result = new List<Sprite>();
            string modImgName = imageName.Replace(":;:", ",");
            modImgName = modImgName.Replace(" & ", ",").Replace(" and ", ",")
                  .Replace(" with ", ",").Replace(" + ", ",").Replace("/", "_");
            string[] allImgs = modImgName.Split(',');

            char dirSeperator = System.IO.Path.DirectorySeparatorChar;
            for (int i = 0; i < allImgs.Length; i++)
            {
                totalTools++;
                Sprite nextSprite = Resources.Load<Sprite>("Images" + dirSeperator + "TOOL" + dirSeperator + "PNG" + dirSeperator + allImgs[i]);
                if (nextSprite != null)
                {
                    result.Add(nextSprite);
                    //       Debug.Log("<color=green>Tool Sprite:</color> [" + allImgs[i] + "] found");
                    foundTools++;
                }
                else
                {
                    Debug.Log("<color=yellow>Tool Sprite:</color> [" + allImgs[i] + "] not found");
                }
            }
            return result.ToArray();
            //    display.image.gameObject.SetActive(imageName.Length > 0 ? true : false);
        }
    }
      
     

    [ContextMenu("Populate Empty Fields:Assembly")]
    void PopulateEmptyFieldsAssembly() =>
        main.assemblySteps = PopulateEmptyFields(main.assemblySteps, Steps.Process.Assembly);

    [ContextMenu("Populate Empty Fields:Dismantling")]
    void PopulateEmptyFieldsDismantling() =>
      main.steps = PopulateEmptyFields(main.steps, Steps.Process.Dismantling);


    List<Step> PopulateEmptyFields(List<Step> theseSteps, Steps.Process ipProc)
    {
        Debug.Log("Populating Empty Steps for " + ipProc.ToString() + " ...");
        int totalSkips = 0;
        for (int i = 0; i < theseSteps.Count; i++)
        {
            string skipString = "";
            Step curStep = theseSteps[i];
            GameObject[] animatorObj = new GameObject[] {curStep.animatedObject==null?null: curStep.animatedObject.gameObject };
            if (animatorObj[0]==null)
                Debug.Log("<color=red>No Animator Found!</color> for Step:" + i);
            else
            {
                if (curStep.lookAtPoint == null)
                    curStep.lookAtPoint = curStep.animatedObject.transform;
                else
                    skipString += ", Lookat Point ";
                if (curStep.objsToHighlight == null)
                    curStep.objsToHighlight = animatorObj;
                else
                    skipString += ", Skipping Highlight Objects";

                if (ipProc == Steps.Process.Assembly)
                {
                    if (curStep.objsToEnable.Length < 1)
                        curStep.objsToEnable = animatorObj;
                    else
                        skipString += ", Objects to Enable";
                }
                else
                {
                    if (curStep.objsToDisable.Length < 1)
                        curStep.objsToDisable = animatorObj;
                    else
                        skipString += ", Objects to Disable";
                }
            }
            if (skipString.Length > 1)
            {
                Debug.Log(" Step:" + i + " skipping " + skipString);
                totalSkips++;
            }
        }
        if(totalSkips>0)
        Debug.Log("Skipped "+totalSkips+" out of " + theseSteps.Count + "steps.");
        Debug.Log("Process Complete");
        return theseSteps;

    }

    public void ExportAssemblySteps()
    {
        if (HasAssemblySteps())
            ExportStepsToCSV(main.assemblySteps, saveFilePrefix + "-" + TYPE_ASSEMBLY + "-");
    }
    public string GetForLocalization(List<Step> steps)
    {
        string dSteps = "Locate part instruction,Step Instruction,Caution Notes"+ System.Environment.NewLine;

        for (int i = 0; i < steps.Count; i++)
        {
            string nextStep = ProcessStringForExport(steps[i].locateObjectText) + ","
                + ProcessStringForExport(steps[i].stepInstructions) + ","
                + ProcessStringForExport(steps[i].cautionNotes);
            dSteps += System.Environment.NewLine + nextStep;
           
        }
        return dSteps;
    }
    public void Translate(List<Step> mainSteps,List<Step> localeSteps)
    {

        for (int i = 0; i < mainSteps.Count; i++)
        {
            mainSteps[i].stepInstructions = ProcessStringForImport( localeSteps[i].stepInstructions);
            mainSteps[i].locateObjectText = ProcessStringForImport(localeSteps[i].locateObjectText);
            mainSteps[i].cautionNotes = ProcessStringForImport(localeSteps[i].cautionNotes);
            if (localeSteps[i].torque != null)
            {
                mainSteps[i].torque = ProcessStringForImport(localeSteps[i].torque);
            }
        }
    }

    public void TranslateForTorque(List<Step> mainSteps, List<Step> localeSteps)
    {
        for (int i = 0; i < mainSteps.Count; i++)
        {
            if (localeSteps[i].torque != null)
            {
                mainSteps[i].torque = ProcessStringForImport(localeSteps[i].torque);
            }
        }
    }

    public void ExportAssemblyLocaleData()
    {
        if (HasAssemblySteps())
            ExportStepsLocalizationToCSV(main.assemblySteps,saveFilePrefix + "-" + TYPE_ASSEMBLY + "-");
    }
    public void ExportDismantlingLocaleData()
    {
        if (HasDismantlingSteps())
            ExportStepsLocalizationToCSV(main.steps,saveFilePrefix + "-" + TYPE_DISMANTLING + "-");
    }
    void ExportStepsToCSV(List<Step> steps,string fName)
    {
        string fileName = fName + " " + DateTime.Now.ToString("dd MMM yyy HH.mm.ss");
        List<string> dSteps = new List<string>();
        dSteps.Add("Sr.No.,Locate part instruction,Step Instruction,isLocked,Linked Part Animator Name," +
            "Anim Trigger,Look At Point,Object to Highlight,Object to Enable,Object to Disable,Caution Notes," +
            "Specs,Camera Override,Tool Used,Image,Audio");

        for (int i = 0; i < steps.Count; i++)
        {

            Step step = steps[i];
            string rowData = (i + 1).ToString() + ",";// SRNO

            rowData +=ProcessStringForExport( step.locateObjectText) + ",";//locate part

            rowData +=ProcessStringForExport( step.stepInstructions) + ",";//Step instr

            rowData += step.isLocked.ToString() + ",";//isLocked

           // string animObj = (step.animatedObject) ? step.animatedObject.name : "";
            string animObj = (step.animatedObject) ? GetPathForGo(step.animatedObject.gameObject) : "";
            rowData += animObj + ",";//Link Part Animator Name
              
            rowData += step.animTriggerName + ",";//Anim Trigger

            string lookAtPt = step.lookAtPoint ? GetPathForGo(step.lookAtPoint.gameObject) : "";
            rowData += lookAtPt + ",";//Look At Pt

            string highlightobjs=step.objsToHighlight == null?"":GetStringFromObjects(step.objsToHighlight);
            //if (highlightobjs == "")
            //{
            //    Debug.Log("Filled Highlight with Animator: " + i + " in " + fName);
            //    highlightobjs = (step.animatedObject) ? step.animatedObject.name : "";
            //}
            rowData += highlightobjs+",";// Highlight Objects

            string objsToEnable = step.objsToEnable == null ? "" : GetStringFromObjects(step.objsToEnable);
            //if (objsToEnable == "")
            //    if (fName.ToUpper().Replace(" ","").Contains("ASS"))
            //{
            //        Debug.Log("Filled Obj to enable with Animator: " + i + " in " + fName);
            //        objsToEnable = (step.animatedObject) ? step.animatedObject.name : "";
            //}
            rowData += objsToEnable + ",";// Enabled Objects

            string objsToDisable = step.objsToDisable == null ? "" : GetStringFromObjects(step.objsToDisable);
            //if (objsToDisable == "")                     
            //    if (fName.ToUpper().Replace(" ", "").Contains("DIS"))
            //{ 
            //        Debug.Log("Filled obj to disable with Animator: " + i + " in " + fName);
            //    objsToDisable = (step.animatedObject) ? step.animatedObject.name : "";
            //}
            rowData += objsToDisable + ",";//Disabled Objects

            rowData +=ProcessStringForExport( step.cautionNotes) + ",";//Caution Notes  

            rowData += ProcessStringForExport(step.torque )+ ",";// Specs

            string ovrCam=step.overrideCameraPosition==null?"": GetPathForGo(step.overrideCameraPosition.gameObject);
            rowData += ovrCam + ",";// Override Camera Position
            string spToolName = step.specialToolName;
            if (spToolName.Length < 2)
              spToolName=  spToolName.Replace("-", "");
            rowData += ProcessStringForExport(spToolName)+ ",";//Special Tool Name
            string allToolNames = "";
            if(step.toolSprite!=null)
            if (step.toolSprite.Length > 0)
            {
                for (int t = 0; t < step.toolSprite.Length; t++)
                {
                    if (step.toolSprite[t] != null)
                        allToolNames += step.toolSprite[t].name;
                    else
                        allToolNames += "(?)";
                    allToolNames += ":;:";
                }
            }
            rowData += allToolNames + ",";//Special Tool Image
            string voiceOvr = GetStepDataStringFor(step);
         //   string voiceOvr = step.voiceOver==null?"":step.voiceOver.name;
            rowData += voiceOvr + ",";// Voice Over

            dSteps.Add(rowData);
        }

        
string dStepPath = Application.dataPath + Path.DirectorySeparatorChar+SAVE_FOLDER_NAME+Path.DirectorySeparatorChar +fileName+ ".csv";
        Debug.Log("Saving "+fileName+" to " + dStepPath.ToString());
        File.WriteAllLines(dStepPath, dSteps.ToArray());
    }
    void ExportStepsLocalizationToCSV(List<Step> steps, string fName)
    {
        Debug.Log("Saving Localization Data for " + main.name);
        string fileName = fName + "-LOCALE-" + DateTime.Now.ToString("dd MMM yyy HH.mm.ss");
        List<string> dSteps = new List<string>();
        dSteps.Add("Locate part instruction,Step Instruction,Caution Notes");

        for (int i = 0; i < steps.Count; i++)
        {

            Step step = steps[i];
            string rowData ="";

            rowData += ProcessStringForExport(step.locateObjectText) + ",";//locate part
            rowData += ProcessStringForExport(step.stepInstructions) + ",";//Step instr
            rowData += ProcessStringForExport(step.cautionNotes);//Caution Notes  
            dSteps.Add(rowData);
        }


        string dStepPath = Application.dataPath + Path.DirectorySeparatorChar + SAVE_FOLDER_NAME + Path.DirectorySeparatorChar
            +"Localization"+Path.DirectorySeparatorChar
            + fileName + ".csv";
        if (!Directory.Exists(Path.GetDirectoryName(dStepPath)))
             Directory.CreateDirectory(Path.GetDirectoryName(dStepPath));
        Debug.Log("Saving " + fileName + " to " + dStepPath.ToString());
        File.WriteAllLines(dStepPath, dSteps.ToArray());
    }
    public static string CalculatePathFor(GameObject go) =>
        string.Join("/", go.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
    public static string CalculatePathFor2(GameObject go)
    {
        Transform par = go.transform.parent;
        Transform root = go.transform.root;
        string result = go.name;
        while (par != root) {
            result = par.name+"/"+result;
            par = par.parent;
                }
        if (par == root)
            result = par.name + "/" + result;
        return result;
    }

    string GetPathForGo(GameObject go)
    {
        string result = "";
        return result;
        if (go!=null)
        result=CalculatePathFor2(go);

        if (detectParentRootMismatch)
        {
            if (main != null)
                if (go.transform.root != main.transform)
                    Debug.LogError("Parent Mismatch for:" + go.name, go);
        }
        if(go!=null)
        if (result.Length < 1)
            Debug.Log("<color=pink> NO Result</color>");
        return result;
    }
    string GetStringFromObjects(GameObject[] objs)
    {
        string result = "";
        return result;
        if(objs!=null)
            foreach (var item in objs)
            {
             //   result += item.gameObject.name + ":;:";
                result += GetPathForGo(item) + ":;:";
            }
        if (objs != null)
            if (objs.Length > 1)
                Debug.Log("<color=blue>Multiple Export:</color>" + result);
        return result;
    }
    void LoadFromCSV(TextAsset loadFileAsset)
    {
      var  data = CSVSimple.Read(loadFileAsset);
        totalSteps = data.Count;
     
            InitialiseData(_GetLoadDataType(loadFileAsset));
       
        Debug.Log(totalSteps + " records found in "+loadFileAsset.name);
    }
    public bool HasAssemblySteps() => main.assemblySteps.Count > 0 ;
    public bool HasDismantlingSteps() => main.steps.Count > 0 ;

     LoadDataType _GetLoadDataType(TextAsset lFile)
    {
        string s = _GetLoadType(lFile);
        eMsg = "";
        if (s == dismantlingStepsString)
        {
            dism = lFile;
            return LoadDataType.Dismantling;
        }
        else
            if (s == assemblyStepsString)
        {
            asm = lFile;
            return LoadDataType.Assembly;
        }
        else
        {
            eMsg = "Please select the correct file to Load";
            return LoadDataType.Error;
        }
       

    }
    public string GetLoadDataType() => _GetLoadDataType(loadFile).ToString();
   string _GetLoadType(TextAsset lFile)
    {
        string result = "";
        if (lFile)
        {          
            string loadFName = lFile.name.ToString().ToUpper();
      //      Debug.Log(loadFName);
            if (loadFName.Contains(dismantlingStepsString))
                result = dismantlingStepsString;
            else
                if (loadFName.Contains(assemblyStepsString))
                result = assemblyStepsString;
         
        }
            return result;
    }
    /// <summary>
    /// Assembly Steps or Disassembly
    /// </summary>
    /// <param name="dataType"></param>
    void InitialiseData(LoadDataType dataType)
    {
        if(main!=null)
        Debug.Log("loading "+dataType.ToString(),main.gameObject);
        switch (dataType)
        {
            case LoadDataType.Assembly:
                if(main)
                main.assemblySteps = GetStepsFromFile(asm);
        //      alist = GetStepsFromData();
                break;
            case LoadDataType.Dismantling:
                if(main)
                main.steps = GetStepsFromFile(dism);
          //     alist = GetStepsFromData();
                break;
            default:
                break;
        }
    }
 
    /// <summary>
    /// Get Headers from the File
    /// </summary>
    /// <param name="curHeaders"></param>
    void UpdateHeaders(List<string> curHeaders)
    {
        refHeaders = new HeaderNames();
        headers = new HeaderNames();
       
        for (int i = 0; i < curHeaders.Count; i++)
        {

            string c = curHeaders[i];
          headers.is_locked = CompareHeader(c, refHeaders.is_locked, headers.is_locked);
          headers.locate_part_instruction = CompareHeader(c, refHeaders.locate_part_instruction,headers.locate_part_instruction);
          headers.animated_object = CompareHeader(c, refHeaders.animated_object,headers.animated_object);
          headers.anim_trigger = CompareHeader(c, refHeaders.anim_trigger,headers.anim_trigger);
          headers.look_at_point = CompareHeader(c, refHeaders.look_at_point,headers.look_at_point);
          headers.step_instruction = CompareHeader(c, refHeaders.step_instruction,headers.step_instruction);
            headers.object_to_highlight = CompareHeader(c, refHeaders.object_to_highlight, headers.object_to_highlight);
            headers.object_to_disable = CompareHeader(c, refHeaders.object_to_disable, headers.object_to_disable);
            headers.object_to_enable = CompareHeader(c, refHeaders.object_to_enable, headers.object_to_enable);
            headers.caution_notes = CompareHeader(c, refHeaders.caution_notes, headers.caution_notes);
            headers.specs = CompareHeader(c, refHeaders.specs, headers.specs);
            headers.override_camera_pos = CompareHeader(c, refHeaders.override_camera_pos, headers.override_camera_pos);
         
          headers.tool_name = CompareHeader(c, refHeaders.tool_name, headers.tool_name);
          headers.tool_image = CompareHeader(c, refHeaders.tool_image, headers.tool_image);
          headers.audio = CompareHeader(c, refHeaders.audio, headers.audio);         
        }
        string CompareHeader(string ip,string b,string c)
        {
            string[] orTerms = b.Split('|');
            string result = c;
            foreach (string sub in orTerms)
            {
                string[] terms = sub.Split('&');

                if (terms.Length > 0)
                {
                    int cur = 0;
                    string nh = ip.ToString().ToUpper();
                    foreach (var item in terms)
                    {
                        if (nh.Contains(item))
                            cur++;
                    }
                    if (cur == terms.Length)
                        result = ip;
                    else
                    {
                        result = c;
                    }
                }
                else
                    result = ip.ToUpper().Contains(sub) ? ip : "";
            }
            if (ip.ToString().ToUpper().Contains(b))
                result= ip;
            else
            {
                result= c;
            }
            if (result == c && result == b)
                eMsg = "Header not found";
            return result;
        }

    }
    static HeaderNames GetUpdatedHeaders(List<string> curHeaders)
    {
       HeaderNames defaultHeaders = new HeaderNames();
      HeaderNames  resultHeaders = new HeaderNames();

        for (int i = 0; i < curHeaders.Count; i++)
        {

            string c = curHeaders[i];
            resultHeaders.is_locked = CompareHeader(c, defaultHeaders.is_locked, resultHeaders.is_locked);
            resultHeaders.locate_part_instruction = CompareHeader(c, defaultHeaders.locate_part_instruction, resultHeaders.locate_part_instruction);
            resultHeaders.animated_object = CompareHeader(c, defaultHeaders.animated_object, resultHeaders.animated_object);
            resultHeaders.anim_trigger = CompareHeader(c, defaultHeaders.anim_trigger, resultHeaders.anim_trigger);
            resultHeaders.look_at_point = CompareHeader(c, defaultHeaders.look_at_point, resultHeaders.look_at_point);
            resultHeaders.step_instruction = CompareHeader(c, defaultHeaders.step_instruction, resultHeaders.step_instruction);
            resultHeaders.object_to_highlight = CompareHeader(c, defaultHeaders.object_to_highlight, resultHeaders.object_to_highlight);
            resultHeaders.object_to_disable = CompareHeader(c, defaultHeaders.object_to_disable, resultHeaders.object_to_disable);
            resultHeaders.object_to_enable = CompareHeader(c, defaultHeaders.object_to_enable, resultHeaders.object_to_enable);
            resultHeaders.caution_notes = CompareHeader(c, defaultHeaders.caution_notes, resultHeaders.caution_notes);
            resultHeaders.specs = CompareHeader(c, defaultHeaders.specs, resultHeaders.specs);
            resultHeaders.override_camera_pos = CompareHeader(c, defaultHeaders.override_camera_pos, resultHeaders.override_camera_pos);

            resultHeaders.tool_name = CompareHeader(c, defaultHeaders.tool_name, resultHeaders.tool_name);
            resultHeaders.tool_image = CompareHeader(c, defaultHeaders.tool_image, resultHeaders.tool_image);
            resultHeaders.audio = CompareHeader(c, defaultHeaders.audio, resultHeaders.audio);
        }
        string CompareHeader(string ip, string b, string c)
        {
            string[] orTerms = b.Split('|');
            string result = c;
            foreach (string sub in orTerms)
            {
                string[] terms = sub.Split('&');

                if (terms.Length > 0)
                {
                    int cur = 0;
                    string nh = ip.ToString().ToUpper();
                    foreach (var item in terms)
                    {
                        if (nh.Contains(item))
                            cur++;
                    }
                    if (cur == terms.Length)
                        result = ip;
                    else
                    {
                        result = c;
                    }
                }
                else
                    result = ip.ToUpper().Contains(sub) ? ip : "";
            }
            if (ip.ToString().ToUpper().Contains(b))
                result = ip;
            else
            {
                result = c;
            }
            if (result == c && result == b)
                Debug.Log("Header not found");
            return result;
        }
        return resultHeaders;
    }
    /// <summary>
    /// Extracts Steps From the Data File and returns a list of stems
    /// </summary>
    /// <returns>List of Steps</returns>
    /// 
    public List<Step> GetStepsFromFile(TextAsset file)
    {
        List<Step> result = new List<Step>();
        var dat = CSVSimple.Read(file);
        if (dat != null)
            if (dat.Count > 0)
                result = GetStepsFromData(dat);
        return result;
    }
    public List<Step> GetLocaleStepsFromFile(TextAsset file)
    {
        List<Step> result = new List<Step>();
        var dat = CSVSimple.Read(file);
        if (dat != null)
            if (dat.Count > 0)
                result = GetLocaleStepsFromData(dat);
        return result;
    }
    public List<string> GetToolNamesFor(TextAsset asm, TextAsset dism)
    {
       
        return GetToolNamesFrom(asm, dism);
    }
    public static List<Sprite> GetToolSpritesFrom(Steps ipSteps)
    {

        List<Sprite> combi = new List<Sprite>(),result = new List<Sprite>();
        combi.AddRange(GetToolSpriteForSteps(ipSteps.steps));
        combi.AddRange(GetToolSpriteForSteps(ipSteps.assemblySteps));

        for (int i = 0; i < combi.Count; i++)
        {
            if (combi[i] != null)
            {
                string curName = combi[i].name;
                bool found = false;
                for (int j = 0; j < result.Count; j++)
                {
                    if (result[j].name == curName)
                        found = true;
                }
                if (!found)
                    result.Add(combi[i]);
            }
            else
                Debug.LogError("NULL Tool-Sprite at " + i);
        }
        return result;
        
    }
  static  List<Sprite> GetToolSpriteForSteps(List<Step> steplist)
    {
        List<Sprite> result = new List<Sprite>();
        for (int i = 0; i < steplist.Count; i++)
        {
            result.AddRange(steplist[i].toolSprite);
        }
        return result;
    }
    public static List<string> GetToolNamesFrom(TextAsset asm,TextAsset dism)
    {
        //headers.tool_image
        List<string> asmTools, dismTools, combi, result;
        asmTools= dismTools= combi= result = new List<string>();
        if(asm)
        asmTools = GetToolNamesFromFile(asm);
        if(dism)
        dismTools = GetToolNamesFromFile(dism);
        Debug.Log("ASM TOOLS: " + asmTools.Count + " DISM TOOLS: " + dismTools.Count);
        combi = new List<string>();
        combi.Concat(asmTools);
        combi.Concat(dismTools);
        for (int i = 0; i < combi.Count; i++)
        {
            bool found = false;
            string curTool= combi[i];
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j] == curTool)
                    found = true;
            }
            if (!found)
                result.Add(curTool);
        }
        return result;

    }
    public static List<string> GetToolNamesFromFile(TextAsset file)
    {
        //headers.tool_image
       string headerName ="";
        int totalStepsWithTools = 0;
        List<Step> stepForTools = new List<Step>();
        List<string> result = new List<string>();
        var dat = CSVSimple.Read(file);
        if (dat != null)
        {
            HeaderNames toolHeaders = GetUpdatedHeaders(dat[2].Keys.ToList());
            headerName = toolHeaders.tool_image;
            Debug.Log("<color=yellow>[FOUND]</color> tool Header is " + headerName);
            if (dat.Count > 0)
                for (int i = 0; i < dat.Count; i++)
                {

                    string imgName = dat[i][headerName].ToString();
                    if (imgName.Length > 0)
                    {
                        totalStepsWithTools++;
                        string[] curStepTools = GetToolNames(dat[i][headerName].ToString());
                        if (curStepTools != null)
                            if (curStepTools.Length > 0)
                            {
                                result.AddRange(curStepTools);
                                Debug.Log("<color=blue>Step: " + i + " has " + curStepTools.Length + " tools</color>");
                            }
                    }

                }
            else
                Debug.Log("No Rows in Data");
        }
        else
            Debug.LogError("DATA NULL for "+file.name);
        Debug.Log("<color=blue>Total Step with Tools: </color>" + totalStepsWithTools);

        string[] GetToolNames(string toolNamesCombined)
        {
            string modImgName = toolNamesCombined.Replace(":;:", ",");
            modImgName = modImgName.Replace(" & ", ",").Replace(" and ", ",")
                  .Replace(" with ", ",").Replace(" + ", ",").Replace("/", "_");
            string[] allImgs = modImgName.Split(',');

            return allImgs;
        }
        return result;
    }
   
    public List<string> GetToolNamesForCurrentSteps()
    {
        List<string> results = new List<string>();
        List<Step> stepForTools = new List<Step>();
        if (!main)
            main = FindObjectOfType<Steps>(true);

        if (main)
            if (main.steps != null)
            {
                stepForTools.AddRange(main.steps);
                stepForTools.AddRange(main.assemblySteps);
            }
            else
                Debug.Log("No Steps Found !!");

        for (int i = 0; i < stepForTools.Count; i++)
        {
            string[] curStepSpTool = GetImageNames(stepForTools[i].specialToolName);
            if (curStepSpTool.Length > 0)
                results.AddRange(curStepSpTool);
        }

        return results;
    }
    public List<string> GetToolNamesFor(List<Step> stepForTools)
    {
        List<string> allToolNames = new List<string>();
        for (int i = 0; i < stepForTools.Count; i++)
        {
            string[] curStepSpTool = GetImageNames(stepForTools[i].specialToolName);
            if(curStepSpTool.Length>0)
            allToolNames.Concat(curStepSpTool.ToList());
        }
        return allToolNames;
    }
    string[] GetImageNames(string imageName)
    {
        List<string> result = new List<string>();
        string modImgName = imageName.Replace(":;:", ",");
        modImgName = modImgName.Replace(" & ", ",").Replace(" and ", ",")
        .Replace(" with ", ",").Replace(" + ", ",").Replace("/", "_");
        string[] allImgs = modImgName.Split(',');

        return allImgs;

        //char dirSeperator = System.IO.Path.DirectorySeparatorChar;
        //for (int i = 0; i < allImgs.Length; i++)
        //{
        //    totalTools++;
            
        //    string nextSprite = Resources.Load<Sprite>("Images" + dirSeperator + "TOOL" + dirSeperator + "PNG" + dirSeperator + allImgs[i]);
        //    if (nextSprite != null)
        //    {
        //        result.Add(nextSprite);
        //      //Debug.Log("<color=green>Tool Sprite:</color> [" + allImgs[i] + "] found");
        //        foundTools++;
        //    }
        //    else
        //    {
        //        Debug.Log("<color=yellow>Tool Sprite:</color> [" + allImgs[i] + "] not found");
        //    }
        //}
        //return result.ToArray();
        //    display.image.gameObject.SetActive(imageName.Length > 0 ? true : false);
    }
    List<Step> GetStepsFromData(List<Dictionary<string, object>> stepData)
    {
        List<Step> stepList= new List<Step>();
    
        int totalData = stepData.Count;
        int totalObjReferences = 0;
        if (totalData < 1)
            eMsg = "Wrong File!";
        st = stepData[0].Keys.ToList();
        UpdateHeaders(st);
        totalTools = foundTools = 0;
        for (int i = 0; i < totalData; i++)
        {
            Step step = new Step();
            step.stepInstructions = stepData[i][headers.step_instruction].ToString().Replace(":;:",",") ;
            step.locateObjectText = stepData[i][headers.locate_part_instruction].ToString();
            //     Debug.Log("Animated" + data[i][headers.animated_object].ToString());
            //************************************************************************************************************
            // GameObject g=  FindObject(stepData[i][headers.animated_object].ToString());

            //if(g!=null)
            //step.animatedObject = g.GetComponent<Animator>();
            //************************************************************************************************************

            step.animTriggerName = stepData[i][headers.anim_trigger].ToString();

            string isL = stepData[i][headers.is_locked]?.ToString().ToUpper();
            step.isLocked = isL.Contains("T");
            if (step.isLocked)
                Debug.Log("step:"+i+" is locked. "+step.stepInstructions);
            step.torque = stepData[i][headers.specs].ToString();
            step.specialToolName = ProcessStringForImport(stepData[i][headers.tool_name].ToString());
          //  step.cautionNotes = ProcessStringForImport(stepData[i][headers.caution_notes].ToString());
            step.cautionNotes = stepData[i][headers.caution_notes].ToString().Replace(":;:",",");
            //************************************************************************************************************
            //GameObject lapt = FindObject(stepData[i][headers.look_at_point].ToString());
            //if (lapt != null)
            //    step.lookAtPoint = lapt.transform;
            //if(stepData[i][headers.object_to_highlight].ToString().Length>0)
            //step.objsToHighlight = GetObjectsFromString(stepData[i][headers.object_to_highlight].ToString()).ToArray();
            //step.objsToEnable = GetObjectsFromString(stepData[i][headers.object_to_enable].ToString()).ToArray();
            //step.objsToDisable = GetObjectsFromString(stepData[i][headers.object_to_disable].ToString()).ToArray();
            //************************************************************************************************************


            //if (stepData[i][headers.override_camera_pos].ToString().Length > 0)
            //{
            //    var ocp = GetObjectsFromString(stepData[i][headers.override_camera_pos].ToString());
            //    if (ocp.Count > 0)
            //    {                 
            //        step.overrideCameraPosition = GetObjectsFromString(stepData[i][headers.override_camera_pos].ToString())[0].transform;
            //    }
            //}
            string imgName = stepData[i][headers.tool_image].ToString();
            if (imgName.Length > 0)
            {
                step.toolSprite = SetImages(stepData[i][headers.tool_image].ToString());
                if (step.toolSprite != null)
                    if (step.toolSprite.Length > 0)
                        Debug.Log("<color=blue>Step: " + i + " has " + step.toolSprite.Length + " tools</color>");
            }

            string voice = stepData[i][headers.audio].ToString();
            if (voice.Length > 0)
            {
                //    step.voiceOver = SetAudio(voice);
                if (!TextUtils.IsB64(voice))
                    Debug.LogError("is Not Base 64: " + i + " LI:" + step.locateObjectText);
                StepData st = GetStepData(voice);
               // step.data = st;
                if (mapper == null)
                    mapper = ObjectReferenceMapper.Instance;
                if (st.animatorId != 0)
                {
                    GameObject animGo = mapper.GetGameObjectFromId(st.animatorId);
                    if(animGo!=null)
                    step.animatedObject =animGo.GetComponent<Animator>();
                }
                step.objsToEnable = mapper.GetGameObjectsFromIds(st.objectsToEnableIds);
                step.objsToHighlight = mapper.GetGameObjectsFromIds(st.objToHighlightIds);
                step.objsToDisable = mapper.GetGameObjectsFromIds(st.objectsToDisableIds);
                if (st.cameraPosId != 0)
                {
                    GameObject g = mapper.GetGameObjectFromId(st.cameraPosId);
                    if (g != null)
                        step.overrideCameraPosition = mapper.GetGameObjectFromId(st.cameraPosId).transform;
                    else
                        Debug.LogError("Step:" + i + " Has some issues");
                }
                if (st.lookAtPosId != 0)
                {
                    GameObject g = mapper.GetGameObjectFromId(st.lookAtPosId);
                    if(g!=null)
                    step.lookAtPoint = mapper.GetGameObjectFromId(st.lookAtPosId).transform;
                    else
                        Debug.LogError("Step:" + i + " Has LookAt Pt issues");
                }
                //step.animatedObject = st.animator;
                //step.objsToHighlight = st.objectsToHighlight;
                //step.objsToEnable = st.objectsToEnable;
                //step.objsToDisable = st.objectsToDisable;
                //step.overrideCameraPosition = st.cameraOverride;
                //step.lookAtPoint = st.lookAtPoint;
            }
            stepList.Add(step);

        }
        if (stepList.Count > 0)
            Debug.Log("<color=blue>Tools:</color> Total: " + totalTools + " found: " + foundTools);
        if (stepList.Count < 1)
            eMsg = "Error Loading Steps, Please Make sure the file has the required headers";
        else
            eMsg = "";
        List<GameObject> GetObjectsFromString(string msg)
        {
           // Debug.Log(msg);
            List<GameObject> g = new List<GameObject>();
            //   string msgConv = msg.Replace(":;:", ",");
            //   string[] s = { msgConv};
            string[] s = msg.Split(new string[] { ":;:" }, StringSplitOptions.None);
            //if (msgConv.Contains(','))
            //    s = msgConv.Split(',');
            //else
            //    if (msg.Contains(';'))
            //    s = msg.Split(';');
              if(s.Length<1)
            {
                GameObject go = FindObject(msg);
                if (go != null)
                    g.Add(go);
                //else
                //    Debug.Log("<color=red>Did not find:</color> " + msg);
            }
              else
            foreach (var i in s)
            {
                if(i!="")
                {
                    GameObject foundGo = FindObject(i);
                    if (foundGo != null)
                        g.Add(foundGo);
                    //else
                    //    Debug.Log("<color=red>Did not find:</color> " + i);
                }
            }
           
            return g;
        }
      //  Debug.Log("SL: " + stepList.Count);
        return stepList;
    }
    List<Step> GetLocaleStepsFromData(List<Dictionary<string, object>> stepData)
    {
        List<Step> stepList = new List<Step>();

        int totalData = stepData.Count;
        //Debug.Log("Total Steps: " + totalData);
        int totalObjReferences = 0;
        if (totalData < 1)
            eMsg = "Wrong File!";
        st = stepData[0].Keys.ToList();
        //Debug.Log("Total Keys: " + st.Count);
        foreach (var item in st)
        {
            //Debug.Log("Key: " + item);
        }
        totalTools = foundTools = 0;
        for (int i = 0; i < totalData; i++)
        {
            Step step = new Step();
            if (stepData[i] != null)
            {
                if (stepData[i].ContainsKey(st[1]))
                    step.stepInstructions = stepData[i][st[1]].ToString().Replace(":;:", ",");
                if (stepData[i].ContainsKey(st[0]))
                    step.locateObjectText = stepData[i][st[0]].ToString();
                if (st.Count > 3)
                {
                    if (stepData[i].ContainsKey(st[3]))
                        step.torque = stepData[i][st[3]].ToString();
                }
                
                
                if (st.Count < 3)
                {
                    if (stepData[i].Keys.Count > 2)
                    {
                        st = stepData[i].Keys.ToList();
                    }
                }
                else
                if (stepData[i].ContainsKey(st[2]))
                    step.cautionNotes = stepData[i][st[2]].ToString();
               
                stepList.Add(step);

            }
        }
        return stepList;
    }
            int notFound = 0;
    GameObject FindObject2(string ip)
    {
        GameObject result = null;
        if (main)
        {
            string[] s = ip.Split('/');
            Transform root = main.transform.root;
            Transform par = root;
            Transform grandChild = root;
            for (int i = 0; i < s.Length; i++)
            {
              grandChild=  par.Find(s[i]);
                if (grandChild != null)
                    par = grandChild;
                else
                    break;
            }
            if (grandChild != null)
                result = grandChild.gameObject;
            else
                Debug.Log(s[s.Length - 1] + " found till ", par);

        }
        return result;
    }
    GameObject FindObject(string ipDir)
    {
        if (ipDir.Length > 0)
            return FindObject2(ipDir);
        else
            return null;
        GameObject FO1(string ip)
        {
            GameObject re = null;
            re = GameObject.Find(ip);
            if (re == null)
            {
                Transform result = main.transform.parent.Find(ip);
                if (result != null)
                    re = result.gameObject;
                else
                {
                    Debug.Log("<color=red>Not found:</color>" + main.transform.parent.name + ":" + ip);
                    notFound++;
                }
            }
            return re;
        }
    }
    Sprite SetImage(string imageName)
    {
        Sprite result;
       
        char dirSeperator = System.IO.Path.DirectorySeparatorChar;
            result = Resources.Load<Sprite>("Images" + dirSeperator + "DT IMAGES" + dirSeperator + imageName);
        return result;
    //    display.image.gameObject.SetActive(imageName.Length > 0 ? true : false);
    }
    int totalTools, foundTools;
    public static List<string> GetToolNamesFromSpecialTools(Steps stepMainInput)
    {
        List<string> asmTools = GetToolNamesFromSpecialTools(stepMainInput.assemblySteps);
        List<string> dismTools = GetToolNamesFromSpecialTools(stepMainInput.steps);
        List<string> combi = new List<string>();
        combi.AddRange(asmTools);
        combi.AddRange(dismTools);
        List<string> result = RemoveDuplicates(combi);
        return result;
    }
    public static List<string> GetToolNamesFromSpecialTools(List<Step> stepForTools)
    {
        List<string> allNames = new List<string>();
        for (int i = 0; i < stepForTools.Count; i++)
        {
            if(stepForTools[i].specialToolName.Length>0)
            {
                List<string> curStepTools = GetFormattedSpecialTools(stepForTools[i].specialToolName);
                if (curStepTools.Count > 0)
                    allNames.AddRange(curStepTools);
            }
        }
        return allNames;
    }
    public static List<string> RemoveDuplicates(List<string> input)
    {
        List<string> result = new List<string>();
        if (input.Count > 0)
        {
            for (int i = 0; i < input.Count; i++)
            {
                string curName = input[i];
                bool found = false;
                for (int j = 0; j < result.Count; j++)
                {
                    if (curName == result[j])
                        found = true;
                }
                if (!found)
                    result.Add(curName);
            }
            Debug.Log("Removed " + (input.Count - result.Count) + " duplicates in " + input.Count + " entries");
        }
        else
            Debug.Log("<color=red>No Tools Found!!</color>");
        return result;
    }
    public static List<string> GetFormattedSpecialTools(string spToolName)
    {
        if (spToolName.Length > 0)
        {
            string modImgName = spToolName.Replace(":;:", ",");
            modImgName = modImgName.Replace(" & ", ",").Replace(" and ", ",")
                  .Replace(" with ", ",").Replace(" + ", ",").Replace("/", "_");
            return new List<string>(modImgName.Split(','));
        }
        else
            return new List<string>();
    }
    Sprite[] SetImages(string imageName)
    {
        List<Sprite> result = new List<Sprite>();
        string modImgName = imageName.Replace(":;:", ",");
      modImgName=  modImgName.Replace(" & ", ",").Replace(" and ",",")
            .Replace(" with ",",").Replace(" + ",",").Replace("/","_");
        string[] allImgs = modImgName.Split(',');

        char dirSeperator = System.IO.Path.DirectorySeparatorChar;
        for (int i = 0; i < allImgs.Length; i++)
        {

            totalTools++;
            Sprite nextSprite=   Resources.Load<Sprite>("Images" + dirSeperator + "TOOL" + dirSeperator + "PNG" + dirSeperator + allImgs[i]);
            if (nextSprite != null)
            {
                result.Add(nextSprite);
                Debug.LogError("<color=green>Tool Sprite:</color> [" + allImgs[i] + "] found");
                foundTools++;
            }
            else
            {
               
               Debug.Log("<color=yellow>Tool Sprite:</color> [" + allImgs[i] + "] not found");
            }
        }
      
        return result.ToArray();
        //    display.image.gameObject.SetActive(imageName.Length > 0 ? true : false);
    }
    string ProcessStringForExport(string ip) => ip.Replace(",", ":;:").Replace(System.Environment.NewLine, ":::").Replace("\n",":::");
    string ProcessStringForImport(string ip)
    {
        string result = "";
        if (ip!=null)
            if(ip.Length>0)
        result= ip.Replace(":;:", ",").Replace(":::", System.Environment.NewLine);
        return result;
    }

    AudioClip SetAudio(string audioName)
    {
        AudioClip result;
        char dirSeperator = System.IO.Path.DirectorySeparatorChar;
        result = Resources.Load<AudioClip>("AUDIO" + dirSeperator + "STEPS" + dirSeperator + audioName);
        return result;
    }
    // Update is called once per frame
    void Update()
    {
        if(now)
        {
            now = false;
            testString = ProcessStringForExport(testString);
        }
        if (saveNow)
        {
            saveNow = false;
            if (main == null)
                main = FindObjectOfType<Steps>();
            ExportStepsToCSV();
        }
        if (loadNow)
        {
            
            loadNow = false;
            LoadFromCSV(loadFile);
        //    Test(alist[1]);

        }
    }
    public void LoadData(TextAsset loadFile) => LoadFromCSV(loadFile);
    public void LoadData(TextAsset asmFile,TextAsset dismFile)
    {
        Debug.Log("<color=blue>Loading ASM "+asmFile.name+" and DISM "+dismFile.name+"</color>");
        asm = asmFile;
        dism = dismFile;
        notFound = 0;
        if (main != null)
        {

            main.assemblySteps = GetStepsFromFile(asmFile);
            main.steps = GetStepsFromFile(dismFile);
        }
        else
        {
           
            Debug.LogError("No Step Reference Found!");
        }
        Debug.Log(notFound + " <color=red>Objects Not found</color>");
    }
    public void LoadStepsFromCSV() => LoadFromCSV(loadFile);
    public void SaveStepsToCSV() => ExportStepsToCSV();
    void Test(Step foo)
    {
        var fieldValues = foo.GetType()
                     .GetFields()
                     .Select(field => field.GetValue(foo))
                     .ToList();
        foreach (var item in fieldValues)
        {
            Debug.Log(item);
        }
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(StepsEximProcessor))]
[CanEditMultipleObjects]
public class StepsEximProcessorEditor : Editor
{ bool toolSpriteOptions = false;
    public override void OnInspectorGUI()
    {

      
        StepsEximProcessor stepsCtrl = (StepsEximProcessor)target;
        //      stepsCtrl.editor = EditorGUILayout.ToggleLeft(new GUIContent("Editor Mode"), stepsCtrl.editor);
        if (!stepsCtrl.editor)
            DrawDefaultInspector();
        else
        {
            if (stepsCtrl)
            {
                var stepList = serializedObject.FindProperty("alist");
                EditorGUILayout.BeginVertical();
                if(stepsCtrl.eMsg!=null)
                if (stepsCtrl.eMsg.Length > 0)
                    EditorGUILayout.HelpBox(stepsCtrl.eMsg, MessageType.Error);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("main"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loadFile"), true);

                //     if (GUILayout.Button("Load "))
                string fileType = stepsCtrl.GetLoadDataType();
                if (stepsCtrl.HasStepReference())
                {
                    if (fileType != "Error")
                    {
                        if (GUILayout.Button("Load Steps to " + fileType))
                            stepsCtrl.LoadStepsFromCSV();
                    }


                    EditorGUILayout.Space();
                    if (stepsCtrl.HasDismantlingSteps() || stepsCtrl.HasAssemblySteps())
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFilePrefix"),
                            new GUIContent("Save File Prefix:"+StepsEximProcessor.SAVE_FOLDER_NAME+"/"));
                    EditorGUILayout.BeginHorizontal();
                    if (stepsCtrl.HasAssemblySteps())
                    {
                        if (GUILayout.Button("Save Assembly Steps to CSV"))
                            stepsCtrl.ExportAssemblySteps();
                        if (GUILayout.Button("Save Assembly Locale CSV"))
                            stepsCtrl.ExportAssemblyLocaleData();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if (stepsCtrl.HasDismantlingSteps())
                    {
                        if (GUILayout.Button("Save Dismantling Steps to CSV"))
                            stepsCtrl.ExportDismantlingSteps();
                        if (GUILayout.Button("Save Dismantling Locale CSV"))
                            stepsCtrl.ExportDismantlingLocaleData();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if(GUILayout.Button("Auto-Assign Steps Prefab"))
                    {
                        Steps stepMain = FindObjectOfType<Steps>();
                        if (stepMain != null)
                            stepsCtrl.main = stepMain;
                        else
                            Debug.LogError("No Steps Prefab available in the scene");
                    }
                    if (stepsCtrl.HasFileToLoad()) 
                    if (GUILayout.Button("Dry Run"))
                        stepsCtrl.LoadStepsFromCSV();
                }

                stepsCtrl.debugMode = EditorGUILayout.BeginFoldoutHeaderGroup(stepsCtrl.debugMode, new GUIContent("Debug"));
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (stepsCtrl.debugMode)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("headers"));

                    //          EditorGUILayout.PropertyField(serializedObject.FindProperty("alist"), new GUIContent("Loaded Steps"));
                    //           EditorGUILayout.PropertyField(serializedObject.FindProperty("allToolSprites"), new GUIContent("All Tool Sprites"));
                    toolSpriteOptions = EditorGUILayout.Foldout(toolSpriteOptions, "Tools");
                    if (toolSpriteOptions)
                    {
                        EditorGUILayout.HelpBox("Total Tool Sprites: " + stepsCtrl.allToolSprites.Length,MessageType.Info);
                        if (stepsCtrl.allToolSprites.Length < 1)
                            stepsCtrl.ScanToolFolder();
                        EditorGUILayout.BeginHorizontal();
                        if (stepsCtrl.allToolSprites.Length > 0)
                        {
                            if (GUILayout.Button("Populate Tools:DISM"))
                                stepsCtrl.LoadDismToolSprites();
                            if (GUILayout.Button("Populate Tools:ASM"))
                                stepsCtrl.LoadAsmToolSprites();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    stepsCtrl.editor = EditorGUILayout.Toggle(new GUIContent("contextual Editor"), stepsCtrl.editor);
                   

                }
                 EditorGUILayout.EndVertical();

                //   EditorGUILayout.PropertyField(stepList,true);
                serializedObject.ApplyModifiedProperties();
            }
            else
                DrawDefaultInspector();
        }
    }
}
#endif