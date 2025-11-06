using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class Step
{
    public string locateObjectText;
    public string stepInstructions;
    public string specialToolName;
    public Animator animatedObject;
    public string animTriggerName;
    public int debugStepNum;
    public Transform lookAtPoint;
    public Transform overrideCameraPosition;
    public bool isLocked = false;
    public GameObject[] objsToHighlight;
    public GameObject[] objsToDisable;
    public GameObject[] objsToEnable;
    public GameObject[] curvedLineObjs;
    public string cautionNotes;
    public AudioClip voiceOver;
    public string torque;
    public Sprite[] toolSprite;

    // public StepsEximProcessor.StepData data;
}

public class Steps : MonoBehaviour
{
    #region Variables
    public string assemblyStepsCSV = "";
    public string dismantlingStepsCSV = "";

    public enum Process { Dismantling, Assembly };
    [Space(20)]
    public Process currentProcess;
    public int currentStep = -1;
    [SerializeField] PartHighlighter highlighter;
    [Space(20)]
    public bool auto = false;
    [Range(1, 5)]
    public int animSpeed = 3;
    [Range(0.2f, 3f)]
    public float textDuration = 1f;

    [Header("---------- DISMANTLING PROCESS ----------")]
    public List<Step> steps;
    [Header("---------- ASSEMBLY PROCESS -------------")]
    public List<Step> assemblySteps;
    Text stepDesc;
    Text toolName;
    Text torqueValueTxt;
    Image[] toolImage;
    public delegate void StepUpdate(int s);
    public event StepUpdate stepUpdated;
    StepsManager stepsMgr;
    [SerializeField] StepManagerAssignments stepManagerAssignments;
    //  [SerializeField]
    bool partLocated = false;
    bool triggeredNext = false;
    Animator currentAnim;
    GameObject replayButton;
    #endregion

    [SerializeField] bool keepHighlightForAnimation = false;
    bool enableDebug = true;
    [SerializeField] Step _currentStepData;
    //   [SerializeField]
    [SerializeField] int totalSteps = 0;
    bool useNewCamera = true;
    public bool useStepPlayer = true;
    bool initialised = false;
    int lastStep = 0;
    CanvasGroup cautionCanvas;

    List<string> instructionListDis, cautionNoteListDis;
    List<string> instructionListAssembly, cautionNoteListAssembly;

    // Start is called before the first frame update

    void Start()
    {
        instructionListDis = new List<string>();
        cautionNoteListDis = new List<string>();
        instructionListAssembly = new List<string>();
        cautionNoteListAssembly = new List<string>();

        foreach (Step steptemp in steps)
        {
            instructionListDis.Add(steptemp.stepInstructions);
            cautionNoteListDis.Add(steptemp.stepInstructions);
            //Debug.LogError(steptemp.locateObjectText);
        }

        foreach (Step steptemp in assemblySteps)
        {
            Debug.Log($"ASM Dhiraj steps : {steptemp.stepInstructions}");
            instructionListAssembly.Add(steptemp.stepInstructions);
            cautionNoteListAssembly.Add(steptemp.stepInstructions);
            /*assemblyStepsPrefab.Add(steptemp);
            Debug.LogError(steptemp.locateObjectText);*/
        }
        
        stepsMgr = StepsManager.Instance;
        stepManagerAssignments = FindObjectOfType<StepManagerAssignments>(true);
        stepDesc = stepsMgr.stepDesc;
        toolName = stepsMgr.toolName_txt; 
        //   toolImage = stepsMgr.toolImage;

        torqueValueTxt = stepsMgr.torqueValueTxt;
        if (highlighter == null)
            highlighter = FindObjectOfType<PartHighlighter>();
        //  if(currentProcess==Process.Assembly)
        if (useStepPlayer)
        {
            SetProcess(currentProcess);
            Previous();
        }
        else
            useNewCamera = false;
    }

    private void Update()
    {
        if (currentAnim != null)
        {
            AnimatorStateInfo info = currentAnim.GetCurrentAnimatorStateInfo(0);
            float time = info.normalizedTime % 1;
            if (time > 0.995f && !triggeredNext)
            {
                StartCoroutine(HighlightNextBtn());
                triggeredNext = true;
                currentAnim = null;
                StartCoroutine(SkipToNext(0.5f));
            }
        }
    }

    public Animator currentAnimator()
    {
        return currentAnim;
    }

    private void OnEnable()
    {
        SetRefereshRate(steps);
        SetRefereshRate(assemblySteps);
        stepsMgr = StepsManager.Instance;
        if (useStepPlayer)
        {
            StepsManager.NextStep += Next;
            StepsManager.PreviousStep += Previous;
        }
        else
        {
            StepsManager.NextStep += NextAnim;
            StepsManager.PreviousStep += PreviousAnim;
        }

    }
    private void OnDisable()
    {

        if (useStepPlayer)
        {
            StepsManager.NextStep -= Next;
            StepsManager.PreviousStep -= Previous;
        }
        else
        {
            StepsManager.NextStep -= NextAnim;
            StepsManager.PreviousStep -= PreviousAnim;
        }

    }
    void SetRefereshRate(List<Step> stepType)
    {
        foreach (Step s in stepType)
        {
            if (s.curvedLineObjs != null)
            {
                for (int i = 0; i < s.curvedLineObjs.Length; i++)
                {
                    if (s.curvedLineObjs[i] == null)
                        return;
                    else
                    {
                        if (s.curvedLineObjs[i].transform.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>() != null)
                        {
                            var valueToRefersh = s.curvedLineObjs[i].transform.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>();
                            Debug.Log("[Curved_1]" + valueToRefersh.refresh_rate + "     " + s.debugStepNum);
                            s.curvedLineObjs[i].gameObject.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>().refresh_rate = 1f;
                            Debug.Log("[Curved_2]" + s.curvedLineObjs[i].gameObject.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>().refresh_rate);
                        }
                    }
                }
            }
        }

    }
    #region Updated
    public void Assembly() => CheckAndSetProcess(Process.Assembly);
    public void Assembly(int stepNum)
    {
        CheckAndSetProcess(Process.Assembly);
        FastForwardToStep(stepNum);
        LocatePartv2();
        partLocated = true;
    }
    public void Dismantling() => CheckAndSetProcess(Process.Dismantling);
    public void Dismantling(int stepNum)
    {
        CheckAndSetProcess(Process.Dismantling);
        FastForwardToStep(stepNum);
        LocatePartv2();
        partLocated = true;
    }

    void CheckAndSetProcess(Process proc)
    {
        if (currentStep > 0)
        {
            currentStep = stepManagerAssignments.preloadStep;

                FastForwardToStep(currentStep);
                //FastForwardToStep(currentStep);
        }
            
            //FastForwardToStep(0);
        if (proc != currentProcess)
        {
            Debug.Log("current step []" + proc);
            SetProcess(proc);
            Previous();
        }
    }
    void SetProcess(Process proc)
    {
        //DebugLog("SET PROCESS:" + proc.ToString());
        partLocated = false;
        currentProcess = Process.Dismantling;
        if (proc == Process.Assembly)
        {
            DebugLog("Starting ASSEMBLY Process");
            // Fast-Forward all Dismantling steps
            currentStep = 0;
            int targetSt = steps.Count - 1;
            Debug.Log(targetSt + "[CURENT STEP OF ASSEMBLY]");
            FastForwardToStep(targetSt);

            Debug.Log( targetSt +  "[CURENT STEP OF ASSEMBLY USing Fast foward] ");
        }
        else
        {
            DebugLog("Starting Dismantling Process");
            // Fast-Rewind All Dismantling Steps
            currentStep = steps.Count - 1;
            FastForwardToStep(0);
        }
        currentProcess = proc;
        totalSteps = GetTotalSteps();
        currentStep = 0;
        initialised = true;
        //   Previous();
        //  partLocated = true;
    }

    IEnumerator HighlightNextBtn(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        iTween.PunchScale(stepsMgr.nextBtn, iTween.Hash("amount", Vector3.one * 0.2f, "time", 0.5f, "looptype", "loop"));
        yield return new WaitForSeconds(5f);
        if (stepsMgr.nextBtn.GetComponent<iTween>() != null)
        {
            DestroyImmediate(stepsMgr.nextBtn.GetComponent<iTween>());
            stepsMgr.nextBtn.transform.localScale = Vector3.one;
        }
    }

    public Step GetStepAt(int s)
    {
        if (s < 0)
            s = 0;
        if (s >= GetTotalSteps())
            s = GetTotalSteps() - 1;
        return currentProcess == Process.Dismantling ? steps[s] : assemblySteps[s];
    }
    public void UpdateCurrentStepData()
    {
        _currentStepData = GetStepAt(currentStep);
    }
    public void Previous()
    {
        //DebugLog("PREVIOUS:" + currentStep);
        if (currentStep >= totalSteps)
            currentStep = totalSteps - 1;
        //REVERSE CURRENT STEP
        partLocated = false;
        if (currentStep >= 0)
        {
            if (!GetStepAt(currentStep).isLocked)
            {
                AnimRewind(GetCurrentStep());
                FastTraversalV2(GetCurrentStep(), 0);
            }
        }
        // REVERSE HALF OF PREV STEP
        currentStep--;
        if (currentStep < 0)
            currentStep = 0;

        Step c = GetCurrentStep();
        if (c.isLocked)
        {
            DebugLog(currentStep + " : is Locked, skipping step");
            if (currentStep > 0)
            {
                while (GetCurrentStep().isLocked)
                {
                    currentStep--;
                }
            }
            if (currentStep <= 0)
            {
                if (GetCurrentStep().isLocked)
                {
                    while (GetCurrentStep().isLocked)
                    {
                        currentStep++;
                    }
                }
            }
        }
        Step d = GetCurrentStep();

        FastTraversalV2(d, 0);
        // RunToggles(GetCurrentStep(), false);
        //    FastTraversalV2(GetCurrentStep(), 0);
        highlighter.RemoveHighLight();
        LocatePartv2();
        partLocated = true;
        _updateStep();
    }
    public void Replay()
    {
        if (currentStep >= 0 && currentStep < totalSteps)
        {
            if (partLocated)
                LocatePartv2();
            else
            {
                Step curStep = GetCurrentStep();
                //Reset Camera Pos
                SetCameraPositionV2(curStep);
                // Play VO
                if (curStep.voiceOver != null && GetComponent<AudioSource>() != null)
                {
                    GetComponent<AudioSource>().clip = curStep.voiceOver;
                    GetComponent<AudioSource>().Play();
                }
                // start animation
                if (curStep.animatedObject != null)
                {
                    curStep.animatedObject.speed = auto ? animSpeed : 1f;
                    curStep.animatedObject.SetTrigger(curStep.animTriggerName);
                    currentAnim = curStep.animatedObject;

                }
            }
        }
    }
    public void Next()
    {
       // DebugLog("NEXT" + currentStep);
       // Debug.Log("Total Step...."+totalSteps);
        totalSteps = GetTotalSteps();
        if (currentStep < 0)
            currentStep = 0;
        if (currentStep >= totalSteps)
            currentStep = totalSteps - 1;
        while (GetCurrentStep().isLocked && currentStep < totalSteps - 1)
            currentStep++;
        Step curStep = GetCurrentStep();
        if (curStep.isLocked)
        {
            Debug.LogError("CURRENT STEP IS LOCKED?");
        }
        else
        {
            // check and stop highlighting the button
            if (stepsMgr.nextBtn.GetComponent<iTween>() != null)
            {
                DestroyImmediate(stepsMgr.nextBtn.GetComponent<iTween>());
                stepsMgr.nextBtn.transform.localScale = Vector3.one;
            }
            if (!partLocated)
            {
                //  if (currentAnim != null)
                //  if (curStep.animatedObject != null)
                //  CompleteThisStep(curStep);// complete Current Step              
                FastTraversalV2(curStep, 1);
                // FastTraversal(currentStep);              
                currentStep++;

                while (GetCurrentStep().isLocked && currentStep < totalSteps - 1)
                    currentStep++;
                if (!GetCurrentStep().isLocked)
                {
                    LocatePartv2();
                    partLocated = true;
                }

            }
            else
            {
                partLocated = false;
                CompleteV2();
            }
        }
        _updateStep();
    }

    void ResolveSpecialTool()
    {

    }

    /*public void FastForwardTo(int ffstep)
    {
        DebugLog("FF to :" + ffstep);
        if (ffstep >= GetTotalSteps())
        {
            Debug.Log("Fast Fwd to Assembly! = BLOCKED ");
            Assembly(ffstep - totalSteps);
            // FastForwardToStep(0);
        }
        else
        {
            FastForwardToStep(ffstep);
            LocatePartv2();
            partLocated = true;
        }
    }*/

    public void FastForwardTo(int ffstep)
    {
        DebugLog("FF to :" + ffstep);
        if (ffstep >= GetTotalSteps())
        {
            
            int stepCount = ffstep - totalSteps;
            Debug.Log("Fast Fwd to Assembly!" + stepCount);
            CheckAndSetProcess(Process.Assembly);
          
            Assembly(stepCount);
            //Assembly(ffstep - totalSteps);
        }
        else
            FastForwardToStep(ffstep);
        LocatePartv2();
        partLocated = true;
    }
    void FastForwardToStep(int ffStep)
    {
        //  ffStep = ffStep >= GetTotalSteps() ? GetTotalSteps() - 1 : ffStep;

        if (ffStep > currentStep)
        {
            while (ffStep > currentStep)
            {
                FastTraversalV2(GetCurrentStep(), 1);
                currentStep++;
            }
        }
        else
        {
            AnimRewind(GetCurrentStep());
            while (ffStep < currentStep)
            {
                FastTraversalV2(GetCurrentStep(), 0);
                currentStep--;
            }
        }
        highlighter.RemoveAllHighlights();
        _updateStep();
    }

    void FastTraversal(int stepId)
    {
        Step s = currentProcess == Process.Dismantling ? steps[stepId] : assemblySteps[stepId];
        ToggleObjects(s.objsToEnable, true);
        //  ToggleObjects(s.objsToEnable, fwd);
        if (s.animatedObject != null)
        {
            DebugLog("FT");
            Animator curAO = s.animatedObject;

            bool curState = s.animatedObject.gameObject.activeInHierarchy;
            s.animatedObject.gameObject.SetActive(true);
            s.animatedObject.SetTrigger(s.animTriggerName);
            s.animatedObject.speed = 1000f;
            s.animatedObject.Update(10f);
            //un-comment following 
            //AnimatorStateInfo info = s.animatedObject.GetCurrentAnimatorStateInfo(0);
            //var has = s.animatedObject.runtimeAnimatorController.animationClips[0].GetHashCode();
            //Debug.Log(stepId + "INFO HASH:" + s.animatedObject.runtimeAnimatorController.animationClips[0].name);
            // s.animatedObject.Play(has, 0, 1f);
            s.animatedObject.Update(10f);
            s.animatedObject.gameObject.SetActive(curState);
            currentAnim = s.animatedObject;
            //  curAO.speed = 0;

        }
        ToggleObjects(s.objsToDisable, false);
        //  ToggleObjects(s.objsToDisable, !fwd);
    }

    //On click next button
    void FastTraversalV2(Step s, int fwd)
    {
        bool state = fwd >= 1 ? true : false;

        if (fwd == 1)   //Fast Forward to Next
            ToggleObjects(s.objsToEnable, true);
        else
            ToggleObjects(s.objsToDisable, true);
        //DebugLog("Fast Forwarded");

        //stepManagerAssignments.ResetPlayPauseButton();
        //if (s.stepInstructions.Contains("Remove the intake manifold"))
        //    Debug.Log("INTAKE BEFORE:"+s.objsToDisable[0].activeSelf);
        if (!s.isLocked)
            if (s.animatedObject != null && s.animatedObject.gameObject.activeInHierarchy && s.animTriggerName.ToString() != "")
            {

                Animator curAO = s.animatedObject;

                bool curState = s.animatedObject.gameObject.activeInHierarchy;
                if (!curState)
                    CheckReversal(s.animatedObject.gameObject);
                s.animatedObject.gameObject.SetActive(true);
                s.animatedObject.SetTrigger(s.animTriggerName);
              //  Debug.Log("Trigger name: " + s.animTriggerName + "Step " + s.debugStepNum);
                s.animatedObject.speed = fwd >= 1 ? 1000f : 0;
                s.animatedObject.Update(10f);

                //can be added twice to compensate for error 
                s.animatedObject.Update(10f);
                s.animatedObject.gameObject.SetActive(curState);
                s.animatedObject.enabled = false;
                currentAnim = s.animatedObject;

            }

        if (fwd >= 1)
            ToggleObjects(s.objsToDisable, false);
        else
            ToggleObjects(s.objsToEnable, false);
        //if (s.stepInstructions.Contains("Remove the intake manifold"))
        //    Debug.Log("INTAKE AFTER:" + s.objsToDisable[0].activeSelf);
    }

    public Step GetCurrentStep()
    {
        Step result;
        //**********************************************************************
        //if (currentStep < 0)
        //    currentStep = 0;
        //*********************************************************************
        if (currentProcess == Process.Dismantling)
        {
            if (currentStep >= steps.Count)
                currentStep = steps.Count - 1;
            result = steps[currentStep];
        }
        else
        {
            if (currentStep >= assemblySteps.Count)
                currentStep = assemblySteps.Count - 1;
            result = assemblySteps[currentStep];
        }

        //if (currentStep > (steps.Count - 2))
        //    Debug.LogError("Reaching!!!");
        return result;
    }

    public int GetTotalSteps() => GetCurrentStepList().Count;
    public List<Step> GetCurrentStepList() => currentProcess == Process.Dismantling ? steps : assemblySteps;
    public PartHighlighter GetPartHighlighter() => highlighter;
    void DebugLog(string msg)
    {
        if (enableDebug)
            Debug.Log(msg);
    }
    /// <summary>
    /// ////
    /// </summary>
    /// <param name="s"></param>
    /// <param name="forward"></param>
    /// <param name="toggleFirst"></param>
    void RunToggles(Step s, bool forward)
    {

        ToggleObjects(s.objsToDisable, !forward);
        // disable the objects which were enabled for this step
        ToggleObjects(s.objsToEnable, forward);
        //   ToggleHighlight(s.objsToHighlight, forward);

    }
    void CheckReversal(GameObject g)
    {

        if (g.activeSelf)
        {
            if (!g.activeInHierarchy)
            {
                GameObject par = g.transform.parent.gameObject;
                while (!par.activeSelf)
                {
                    Debug.Log("This", par);
                    par.SetActive(true);
                    par = par.transform.parent.gameObject;
                }
            }
        }
    }
    void LocatePartv2()
    {
        //DebugLog("Locate:" + currentStep);
        if (currentStep > -1)
        {
            Step lpv2 = GetCurrentStep();

            Step lpv2Temp = GetCurrentStep();

            //stepsMgr.toolList.transform.localPosition = new Vector3(-(Screen.width / 2f) - 280f,
            //    stepsMgr.toolList.transform.localPosition.y, stepsMgr.toolList.transform.localPosition.z);

            if (currentStep < totalSteps)
            {
                //      lpv2 = GetCurrentStep();           
                ToggleObjects(lpv2.objsToEnable, true);
                //*****************************************************************************
                //   if (lpv2.animatedObject)

                //*****************************************************************************
                if (lpv2 != null)
                {
                    if (lpv2.animatedObject != null)
                    {
                        lpv2.animatedObject.enabled = true;
                        CheckReversal(lpv2.animatedObject.gameObject);
                    }
                    else
                        Debug.Log("<color=red>No Animator found at step:</color>" + currentStep);
                }
                else
                    Debug.LogError("Cannot Find:" + lpv2.animatedObject.name, lpv2.animatedObject.gameObject);
                //   ToggleObjects(lpv2.objsToDisable, true);
                // start at first frame
                if (lpv2.animatedObject != null)
                {
                    AnimRewind(lpv2);
                }
                //  FastTraversalV2(lpv2, 0);

                // enable highlight
                ToggleHighlight(lpv2.objsToHighlight, true);
                if (currentProcess == Process.Dismantling)
                {
                    ToggleCurvedLineDelay(steps[currentStep].curvedLineObjs, false);
                    if (currentStep != 0)
                        ToggleCurvedLineDelay(steps[lastStep].curvedLineObjs, false);
                }
                else
                {
                    ToggleCurvedLineDelay(assemblySteps[currentStep].curvedLineObjs, false);
                    if (currentStep != 0)
                        ToggleCurvedLineDelay(assemblySteps[lastStep].curvedLineObjs, false);
                }
                // set the cam position
                SetCameraPositionV2(lpv2);
                // show description
                // set the description
                //    stepsMgr.stepDesc.text = s.locateObjectText;
                ShowStaticContent(lpv2);
                ShowOneTimeUseAssembly(currentStep);
            }
        }
    }
    void CompleteV2()
    {

        Step cmpStep = GetCurrentStep();
        // disable highlight for current step
        if (!keepHighlightForAnimation)
            highlighter.RemoveHighLight();
        SetCameraPositionV2(cmpStep);
        // set the description
        stepsMgr.stepDesc.text = cmpStep.stepInstructions;
        if (currentProcess == Process.Dismantling)
        {
            ToggleCurvedLineDelay(steps[currentStep].curvedLineObjs, true);
            lastStep = currentStep;
        }
        else
        {
            ToggleCurvedLineDelay(assemblySteps[currentStep].curvedLineObjs, true);
            lastStep = currentStep;
        }

        // Play VO
        if (cmpStep.voiceOver != null && GetComponent<AudioSource>() != null)
        {
            GetComponent<AudioSource>().clip = cmpStep.voiceOver;
            GetComponent<AudioSource>().Play();
        }

        // start animation
        if (cmpStep.animatedObject != null)
        {
            cmpStep.animatedObject.enabled = true;
            cmpStep.animatedObject.speed = auto ? animSpeed : 1f;
            currentAnim = cmpStep.animatedObject;
        }
        else
        {
            StartCoroutine(HighlightNextBtn(1f));
            StartCoroutine(SkipToNext(textDuration));
        }

    }
    void AnimRewind(Step thisStep) => SkipAnim(thisStep, 0);
    void AnimForward(Step thisStep) => SkipAnim(thisStep, 1);

    void SkipAnim(Step thisStep, int skipTo)
    {
        //DebugLog("SKIPANIM:" + skipTo);
        if (thisStep.animatedObject != null)
        {
            Animator curAO = thisStep.animatedObject;

            //   bool curAState = curAO.gameObject.activeInHierarchy;
            AnimatorStateInfo info = curAO.GetCurrentAnimatorStateInfo(0);
            if (skipTo == 0)
                thisStep.animatedObject.SetTrigger(thisStep.animTriggerName);
            //  curAO.enabled = true;
            //   curAO.Play(info.fullPathHash, 0, skipTo);
            curAO.Play(info.fullPathHash, -1, skipTo);

            currentAnim = thisStep.animatedObject;
            //   curAO.gameObject.SetActive (curAState);
            //  if (skipTo == 0)
            // curAO.speed = 0;
            curAO.speed = skipTo == 0 ? 0 : 10000f;


        }
    }

    //Go to next step
    void SetCameraPositionV2(Step s)
    {
        if (stepsMgr == null)
            stepsMgr = StepsManager.Instance;
        if (useNewCamera)
        {
            if (s.lookAtPoint != null && s.overrideCameraPosition != null)
            {
                //Debug.Log("<color=blue>[NEW METHOD!]</color>");
                stepsMgr.mainCamEx.NewPosTarget(s.overrideCameraPosition.position, s.lookAtPoint);
            }
            else
            {
                if (s.lookAtPoint != null)
                {
                    stepsMgr.freeCamPivot.transform.position = s.lookAtPoint.transform.position;
                    stepsMgr.mainCamEx.target = s.lookAtPoint;
                }
                if (s.overrideCameraPosition != null)
                {
                    DebugLog("USE NEW cam");
                    if (s.lookAtPoint != null)
                    {
                        stepsMgr.mainCamEx.distance = Vector3.Distance(s.lookAtPoint.transform.position, s.overrideCameraPosition.transform.position);
                    }
                    stepsMgr.mainCamEx.NewPosRot(s.overrideCameraPosition.position, s.overrideCameraPosition.localRotation);
                }
            }
          
        }
        else
            SetCameraPosition(s);
    }
    void ToggleObjects(GameObject[] objs, bool isOn)
    {
        if(objs!=null)
        foreach (GameObject g in objs)
        {
            if (g != null)
                g.SetActive(isOn);
            else
                Debug.Log("Null Element found in Step:" + currentStep + " of " + currentProcess.ToString());
        }
        else
            Debug.LogError("Null Element found in Step:" + currentStep + " of " + currentProcess.ToString()+" Inst: "+GetCurrentStep().stepInstructions);
    }

    void ToggleHighlight(GameObject[] obj, bool add)
    {
        if (add)
        {
            if (keepHighlightForAnimation)
                highlighter.RemoveHighLight();
            if (obj != null)
                highlighter.Highlight(obj);
            else
                Debug.LogError("null highlight list found in " + currentStep + " of " + currentProcess + " inst " + GetCurrentStep().locateObjectText);
        }
        else
        {
            if (!keepHighlightForAnimation)
                highlighter.RemoveHighLight();
        }
    }
    void ToggleCurvedLineDelay(GameObject[] obj, bool isUsing)
    {
        if (isUsing)
        {
            foreach (GameObject go in obj)
            {
                go.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>().refresh_rate = 0.015f;
            }
        }
        else
        {
            foreach (GameObject go in obj)
            {
                go.GetComponent<IndieMarc.CurvedLine.CurvedLine3D>().refresh_rate = 1f;
            }
        }
    }

    public void ShowRestartBtn(Step s)
    {
        //stepsMgr.restartBtnContainer.SetActive(partLocated && s.animatedObject != null);
    }
    public bool PartLocated() => partLocated;
    void _updateStep()
        => stepUpdated?.Invoke(currentStep);
    void ShowStaticContent(Step s)
    {
        // set the description
        stepsMgr.stepDesc.text = s.locateObjectText;

        //stepsMgr.toolPopup.SetActive(s.specialToolName != "" && s.specialToolName != "-");
        //if (stepsMgr.toolPopup != null)
        //    if(stepsMgr.toolName_txt!=null)
        //    stepsMgr.toolName_txt.text = s.specialToolName;

        bool hasToolSprites = false;
        if (s.toolSprite != null)
            hasToolSprites = s.toolSprite.Length > 0;
        // NEW UI ANIMATION
        //iTween.MoveTo(stepsMgr.toolList.gameObject, iTween.Hash("x", hasToolSprites ? 20f : -280f, "time", 0.5f, "delay", 1f));

        stepsMgr.toolList.gameObject.SetActive(hasToolSprites);
        if (hasToolSprites)
        {

            stepsMgr.toolList.Load(s.toolSprite, true);

        }
        //  toolName.text = s.specialToolName;
        //  toolImage.sprite = s.toolSprite;
        //if(stepsMgr.slideShow!=null)
        //stepsMgr.slideShow.Load(s.toolSprite);

        stepsMgr.torquePopup.SetActive(s.torque != "");

        //iTween.MoveTo(stepsMgr.torquePopup.transform.GetChild(0).gameObject, iTween.Hash("x", s.torque != "" ? Screen.width : Screen.width + 700f, "time", 0.5f, "delay", 0.5f));
        stepsMgr.torqueValueTxt.text = s.torque;
        //   torqueValueTxt.text = s.torque;

        // set the caution text
        bool showCaution = false;
        s.cautionNotes = s.cautionNotes.Trim();
        if (s.cautionNotes != null)
        {
            showCaution = (s.cautionNotes.Length > 0);
            if(cautionCanvas==null)
             cautionCanvas = stepsMgr.caution_popup.GetComponent<CanvasGroup>();

            if (cautionCanvas)
            {
                cautionCanvas.alpha = 1;
                //    stepsMgr.caution_popup.SetActive(showCaution);
                stepsMgr.mainCamEx.PanLeft();
            }

        }
   //     iTween.MoveTo(stepsMgr.caution_popup.transform.GetChild(0).gameObject, iTween.Hash("x", showCaution ? Screen.width : Screen.width + 700f, "time", 0.5f, "delay", 0.2f));
        iTween.MoveTo(stepsMgr.caution_popup.transform.GetChild(0).gameObject, iTween.Hash("x", showCaution ? Screen.width : Screen.width + 700f, "time", 0.5f, "delay", 0.2f));
        // stepsMgr.caution_txt.text = s.cautionNotes.Replace(".", ".\n");
        if (showCaution)
        {
            string[] cautionLines = s.cautionNotes.ToString().Split(new string[] { ":::" }, System.StringSplitOptions.None);
            stepsMgr.caution_txt.Load(cautionLines);
        }
        
        /*if (stepsMgr.oneTimeUse != null)
            stepsMgr.oneTimeUse.SetActive(s.stepInstructions.ToUpper().Contains("RENEW") 
                || s.stepInstructions.ToUpper().Contains("REPLACE") 
                || s.stepInstructions.Contains("રીન્યુ") 
                || s.stepInstructions.Contains("नवीनीकृत") 
                || s.stepInstructions.Contains("புதுப்பிக்க") 
                || s.cautionNotes.ToUpper().Contains("RENEW")
                || s.cautionNotes.ToUpper().Contains("REPLACE")
                || s.cautionNotes.Contains("રીન્યુ") 
                || s.cautionNotes.Contains("नवीनीकृत") 
                || s.cautionNotes.Contains("புதுப்பிக்க"));*/
        if (stepsMgr.infoPanel != null)
            stepsMgr.infoPanel.SetActive(s.cautionNotes != "" || s.torque != "");
    }

    void ShowOneTimeUse(Step s)
    {
        //Debug.LogWarning("nslfjalkjfla -----> " + s.stepInstructions);
        if (stepsMgr.oneTimeUse != null)
            stepsMgr.oneTimeUse.SetActive(s.stepInstructions.ToUpper().Contains("RENEW")
                || s.stepInstructions.ToUpper().Contains("REPLACE")
                || s.cautionNotes.ToUpper().Contains("RENEW")
                || s.cautionNotes.ToUpper().Contains("REPLACE"));
    }

    void ShowOneTimeUseAssembly(int s)
    {
        if (currentProcess == Process.Dismantling)
        {
            //Debug.LogWarning("nslfjalkjfla -----> " + instructionListDis[s]);
            if (stepsMgr.oneTimeUse != null)
                stepsMgr.oneTimeUse.SetActive(instructionListDis[s].ToUpper().Contains("RENEW")
                    || instructionListDis[s].ToUpper().Contains("REPLACE")
                    || cautionNoteListDis[s].ToUpper().Contains("RENEW")
                    || cautionNoteListDis[s].ToUpper().Contains("REPLACE"));
        }
        else
        {
            //Debug.LogWarning("nslfjalkjfla -----> " + instructionListAssembly[s]);
            if (stepsMgr.oneTimeUse != null)
                stepsMgr.oneTimeUse.SetActive(instructionListAssembly[s].ToUpper().Contains("RENEW")
                    || instructionListAssembly[s].ToUpper().Contains("REPLACE")
                    || cautionNoteListAssembly[s].ToUpper().Contains("RENEW")
                    || cautionNoteListAssembly[s].ToUpper().Contains("REPLACE"));
        }
    }
    #endregion
    #region FallBack
    //--------------------------------------------------------------------------------------
    void PreviousAnim()
    {
        StartCoroutine(TriggerPrevAnim());

    }
    IEnumerator TriggerPrevAnim()
    {
        if (currentStep < 1)
        {
            yield break;
        }
        if (currentProcess == Process.Dismantling)
        {
            for (int i = 0; i < 2; i++)
            {
                // reset the objects to their original state
                // enable the objects which are supposed to be disabled
                ToggleObjects(steps[currentStep].objsToDisable, true);
                // disable the objects which were enabled for this step
                ToggleObjects(steps[currentStep].objsToEnable, false);

                // disable highlight for current step
                ToggleHighlight(steps[currentStep].objsToHighlight, false);

                // reset at first frame
                if (steps[currentStep].animatedObject != null)
                {
                    steps[currentStep].animatedObject.SetTrigger("idle");
                    yield return new WaitForSeconds(1f);
                }
                currentStep--;
            }
            // set the cam position
            //SetCameraPosition(steps[currentStep + 1]);

            // stop if audio is playing
            if (GetComponent<AudioSource>() != null)
            {
                if (GetComponent<AudioSource>().isPlaying)
                {
                    GetComponent<AudioSource>().Stop();
                }
            }
            partLocated = false;
            NextAnim();
        }
        else if (currentProcess == Process.Assembly)
        {
            for (int i = 0; i < 2; i++)
            {
                // reset the objects to their original state
                // enable the objects which are supposed to be disabled
                ToggleObjects(assemblySteps[currentStep].objsToDisable, true);
                // disable the objects which were enabled for this step
                ToggleObjects(assemblySteps[currentStep].objsToEnable, false);

                // disable highlight for current step
                ToggleHighlight(assemblySteps[currentStep].objsToHighlight, false);

                // reset at first frame
                if (assemblySteps[currentStep].animatedObject != null)
                {

                    assemblySteps[currentStep].animatedObject.SetTrigger(assemblySteps[currentStep].animTriggerName);
                    AnimatorStateInfo info = assemblySteps[currentStep].animatedObject.GetCurrentAnimatorStateInfo(0);

                    assemblySteps[currentStep].animatedObject.Play(info.nameHash, 0, 0f);
                    //yield return new WaitForEndOfFrame();
                    assemblySteps[currentStep].animatedObject.speed = 0f;
                    assemblySteps[currentStep].animatedObject.enabled = false;
                    yield return new WaitForSeconds(0.1f);
                }
                currentStep--;
            }

            // set the cam position
            //SetCameraPosition(assemblySteps[currentStep + 1]);

            // stop if audio is playing
            if (GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Stop();
            }
            partLocated = false;
            NextAnim();
        }
    }
    void LocatePart()
    {
        stepsMgr.prevBtn.GetComponent<Button>().interactable = false;
        if (currentProcess == Process.Dismantling)
        {
            if (currentStep > -1)
            {
                // disable the previous object
                ToggleObjects(steps[currentStep].objsToDisable, false);
                ToggleObjects(steps[currentStep].objsToEnable, true);
            }

            if (currentStep >= steps.Count - 1)
            {
                // if reached end then start assembly process
                Debug.Log("Reached End");
                currentProcess = Process.Assembly;
                currentStep = -1;
                partLocated = false;
                return;
            }

            // set the next step as the current step
            currentStep++;

            ShowRestartBtn(steps[currentStep]);

            // check if current and the following steps are locked
            while (steps[currentStep].isLocked && currentStep < steps.Count - 1)
            {
                // disable objects
                ToggleObjects(steps[currentStep].objsToDisable, false);

                // go to next step
                currentStep++;
                Debug.Log("SKIPPED: " + steps[currentStep].locateObjectText);
            }

            if (currentStep < steps.Count)
            {
                // set the cam position
                SetCameraPosition(steps[currentStep]);

                // show description
                ShowStaticContent(steps[currentStep]);
                ShowOneTimeUseAssembly(currentStep);

                // enable highlight
                ToggleHighlight(steps[currentStep].objsToHighlight, true);

                ToggleCurvedLineDelay(steps[currentStep - 1].curvedLineObjs, false);
                // start at first frame
                if (steps[currentStep].animatedObject != null)
                {
                    steps[currentStep].animatedObject.SetTrigger(steps[currentStep].animTriggerName);
                    steps[currentStep].animatedObject.speed = 0;
                    steps[currentStep].animatedObject.enabled = false;
                }
            }
        }
        else if (currentProcess == Process.Assembly)
        {
            // fast forward to completion if the user clicks next before the animation is complete
            if (currentStep > -1)
            {
                if (assemblySteps[currentStep].animatedObject != null)
                {
                    assemblySteps[currentStep].animatedObject.speed = 100f;
                    // assemblySteps[currentStep].animatedObject.playbackTime = 100f;
                    //      currentAnim.playbackTime = 0.998f;
                    assemblySteps[currentStep].animatedObject.StopPlayback();
                    StartCoroutine(ResetAnimSpeed(assemblySteps[currentStep].animatedObject));
                }
            }
            currentStep++;

            // check if current and the following steps are locked
            while (assemblySteps[currentStep].isLocked)
            {
                foreach (GameObject g in assemblySteps[currentStep].objsToEnable)
                {
                    g.SetActive(true);
                }
                currentStep++;
                Debug.Log("SKIPPED: " + assemblySteps[currentStep].locateObjectText);
            }

            // set the cam position
            SetCameraPosition(assemblySteps[currentStep]);

            // show description, caution, torque values
            ShowStaticContent(assemblySteps[currentStep]);
            //ShowOneTimeUse(assemblyStepsPrefab[currentStep]);
            ShowOneTimeUseAssembly(currentStep);

            // enable highlight
            ToggleHighlight(assemblySteps[currentStep].objsToHighlight, true);

            ToggleCurvedLineDelay(steps[currentStep - 1].curvedLineObjs, false);

            // enable the objects required for the current step
            ToggleObjects(assemblySteps[currentStep].objsToEnable, true);
            ToggleObjects(assemblySteps[currentStep].objsToDisable, false);


            // start at first frame
            if (assemblySteps[currentStep].animatedObject != null)
            {
                Debug.Log("Triggered: " + assemblySteps[currentStep].animTriggerName);
                assemblySteps[currentStep].animatedObject.SetTrigger(assemblySteps[currentStep].animTriggerName);
                //  assemblySteps[currentStep].animatedObject.playbackTime = 0.1f;
                assemblySteps[currentStep].animatedObject.enabled = false;
                assemblySteps[currentStep].animatedObject.speed = 0f;
            }
        }
    }
    void CompleteTheStep()
    {
        stepsMgr.prevBtn.GetComponent<Button>().interactable = true;
        if (currentProcess == Process.Dismantling)
        {
            // disable highlight for current step
            ToggleHighlight(steps[currentStep].objsToHighlight, false);

            ToggleCurvedLineDelay(steps[currentStep].curvedLineObjs, true);

            // set the description
            stepDesc.text = steps[currentStep].stepInstructions;

            // Play VO
            if (steps[currentStep].voiceOver != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().clip = steps[currentStep].voiceOver;
                GetComponent<AudioSource>().Play();
            }

            // start animation
            if (steps[currentStep].animatedObject != null)
            {
                steps[currentStep].animatedObject.enabled = true;
                steps[currentStep].animatedObject.speed = auto ? animSpeed : 1f;
                currentAnim = steps[currentStep].animatedObject;
            }
            else
            {
                StartCoroutine(HighlightNextBtn(1f));
                StartCoroutine(SkipToNext(textDuration));
            }
        }
        else if (currentProcess == Process.Assembly)
        {
            // disable highlight for current step
            ToggleHighlight(assemblySteps[currentStep].objsToHighlight, false);
            // set the description
            stepDesc.text = assemblySteps[currentStep].stepInstructions;

            ToggleCurvedLineDelay(steps[currentStep].curvedLineObjs, true);
            // Play VO
            if (assemblySteps[currentStep].voiceOver != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().clip = assemblySteps[currentStep].voiceOver;
                GetComponent<AudioSource>().Play();
            }
            // start animation
            if (assemblySteps[currentStep].animatedObject != null)
            {
                //assemblySteps[currentStep].animatedObject.SetTrigger(assemblySteps[currentStep].animTriggerName);
                assemblySteps[currentStep].animatedObject.enabled = true;
                assemblySteps[currentStep].animatedObject.speed = auto ? animSpeed : 1f;
                currentAnim = assemblySteps[currentStep].animatedObject;
            }
            else
            {
                StartCoroutine(HighlightNextBtn(1f));
                StartCoroutine(SkipToNext(textDuration));
            }
        }
    }

    IEnumerator SkipToNext(float t)
    {
        yield return new WaitForSeconds(t);
        if (auto)
        {
            NextAnim();
        }
        triggeredNext = false;
    }

    void NextAnim()
    {
        // check and stop highlighting the button
        if (stepsMgr.nextBtn.GetComponent<iTween>() != null)
        {
            DestroyImmediate(stepsMgr.nextBtn.GetComponent<iTween>());
            stepsMgr.nextBtn.transform.localScale = Vector3.one;
        }
        if (!partLocated)
        {
            partLocated = true;
            LocatePart();
            StartCoroutine(SkipToNext(textDuration));
        }
        else
        {
            partLocated = false;
            CompleteTheStep();
        }
    }

    IEnumerator ResetAnimSpeed(Animator anim)
    {
        Debug.Log("Reset was called");
        yield return new WaitForSeconds(2f);
        anim.speed = 1f;
    }

    // ---------------------------------------------------------------------------------

    void SetCameraPosition(Step s)
    {
        if (s.overrideCameraPosition != null)
        {
            // PIP camera
            if (stepsMgr.fixedCamera != null)
            {
                stepsMgr.fixedCamera.transform.position = s.overrideCameraPosition.position;
                stepsMgr.fixedCamera.transform.localEulerAngles = s.overrideCameraPosition.localEulerAngles;
            }
            // main camera
            stepsMgr.mainCamera.transform.position = s.overrideCameraPosition.position;
            stepsMgr.mainCamera.transform.localEulerAngles = s.overrideCameraPosition.localEulerAngles;
        }
        if (s.lookAtPoint != null)
        {
            stepsMgr.freeCamPivot.transform.position = s.lookAtPoint.transform.position;
        }
    }
    // ---------------------------------------------------------------------------------

    [ContextMenu("Export Steps to CSV")]
    void ExportToCSV()
    {
        List<string> dSteps = new List<string>();
        dSteps.Add("Sr.No.,Locate part instruction,Step Instruction,isLocked,Tool Name");
        for (int i = 0; i < steps.Count; i++)
        {
            dSteps.Add(
                (i + 1) + ","
                + steps[i].locateObjectText.Replace(",", "") + ","
                + steps[i].stepInstructions.Replace(",", "") + ","
                + steps[i].isLocked.ToString() + ","
                + steps[i].specialToolName.Replace(",", "")
                );
        }
        string dStepPath = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + "DismantlingSteps.csv";
        File.WriteAllLines(dStepPath, dSteps.ToArray());

        List<string> aSteps = new List<string>();
        aSteps.Add("Sr.No.,Locate part instruction,Step Instruction,isLocked,Tool Name");
        for (int i = 0; i < assemblySteps.Count; i++)
        {
            aSteps.Add(
                (i + 1) + ","
                + assemblySteps[i].locateObjectText.Replace(",", "") + ","
                + assemblySteps[i].stepInstructions.Replace(",", "") + ","
                + assemblySteps[i].isLocked.ToString() + ","
                + assemblySteps[i].specialToolName.Replace(",", "")
                );
        }
        string aStepPath = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + "Assembly.csv";
        File.WriteAllLines(aStepPath, aSteps.ToArray());
    }

    [ContextMenu("Import Assembly Steps")]
    void ImportAssemblySteps()
    {
        TextAsset data = Resources.Load(assemblyStepsCSV) as TextAsset;
        string[] lines = Regex.Split(data.text, System.Environment.NewLine);
        for (int i = 1; i < lines.Length; i++)
        {
            string srNo = Regex.Split(lines[i], ",")[0];
            string locateText = Regex.Split(lines[i], ",")[1];
            string stepInstr = Regex.Split(lines[i], ",")[2];
            string isLocked = Regex.Split(lines[i], ",")[3];
            string toolName = Regex.Split(lines[i], ",")[4];
            string renew = Regex.Split(lines[i], ",")[5];
            string torque = Regex.Split(lines[i], ",")[6];

            assemblySteps[i - 1].locateObjectText = locateText;
            assemblySteps[i - 1].stepInstructions = stepInstr;
            assemblySteps[i - 1].specialToolName = toolName;
            assemblySteps[i - 1].torque = torque;
        }
    }

    [ContextMenu("Import Dismantling Steps")]
    void ImportDismantlingSteps()
    {
        TextAsset data = Resources.Load(dismantlingStepsCSV) as TextAsset;
        string[] lines = Regex.Split(data.text, System.Environment.NewLine);
        for (int i = 1; i < lines.Length; i++)
        {
            string srNo = Regex.Split(lines[i], ",")[0];
            string locateText = Regex.Split(lines[i], ",")[1];
            string stepInstr = Regex.Split(lines[i], ",")[2];
            string isLocked = Regex.Split(lines[i], ",")[3];
            string toolName = Regex.Split(lines[i], ",")[4];
            string renew = Regex.Split(lines[i], ",")[5];
            string torque = Regex.Split(lines[i], ",")[6];

            steps[i - 1].locateObjectText = locateText;
            steps[i - 1].stepInstructions = stepInstr;
            steps[i - 1].specialToolName = toolName;
            steps[i - 1].torque = torque;
        }
    }
    #endregion
    [ContextMenu("Update Step Numbers")]
    public void AddNumbers()
    {
        int i = 0;
        foreach (Step s in assemblySteps)
        {
            s.debugStepNum = i;
            i++;
        }
        i = 0;
        foreach (Step s in steps)
        {
            s.debugStepNum = i;
            i++;
        }
    }

    [ContextMenu("Print Locked Step Count")]
    void ShowLockedCount()
    {
        int i = 0;
        foreach (Step s in assemblySteps)
        {
            if (s.isLocked)
            {
                i++;
            }
        }
        foreach (Step s in steps)
        {
            if (s.isLocked)
            {
                i++;
            }
        }
        Debug.Log("Locked Steps: " + i +" / "+ (assemblySteps.Count+steps.Count));
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Steps))]
[CanEditMultipleObjects]
public class StepsEditor : Editor
{
    int ffStepNum;
    bool showStep;
    int cur;
    SerializedProperty stepObj;
    public override void OnInspectorGUI()
    {


        Steps steps = (Steps)target;
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        if (steps.useStepPlayer)
        {
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Step Player", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Now Playing " + steps.currentProcess.ToString() + ": " + steps.currentStep, EditorStyles.largeLabel);
                if (steps.currentProcess != Steps.Process.Dismantling)
                    if (GUILayout.Button("Dismantling"))
                        steps.Dismantling();
                if (steps.currentProcess != Steps.Process.Assembly)
                    if (GUILayout.Button("Assembly"))
                        steps.Assembly();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Previous, Replay ,Next", MessageType.None);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(" << "))
                {
                    steps.Previous();
                    steps.UpdateCurrentStepData();
                }
                if (GUILayout.Button(" REP "))
                {
                    steps.Replay();
                    steps.UpdateCurrentStepData();
                }
                if (GUILayout.Button(" >> "))
                {
                    steps.Next();
                    steps.UpdateCurrentStepData();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Fast Forward Or Rewind to any step", MessageType.None);
                EditorGUILayout.BeginHorizontal();
                ffStepNum = EditorGUILayout.IntField(ffStepNum, GUILayout.MinWidth(30));
                if (GUILayout.Button(" GO "))
                {
                    steps.FastForwardTo(ffStepNum);
                    steps.UpdateCurrentStepData();
                }
                EditorGUILayout.EndHorizontal();

            }
            else
                EditorGUILayout.HelpBox("Step Controls are Only Visible during Play mode", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("enable Step Player to Toggle Player", MessageType.None);
            if (GUILayout.Button("Enable Step Player"))
                steps.useStepPlayer = true;

        }
    }
    void AddOutlineEffect()
    {
        cakeslice.OutlineEffect outlineEffect = FindObjectOfType<cakeslice.OutlineEffect>();
        if (outlineEffect == null)
        {
            ExteriorCam extCam = FindObjectOfType<ExteriorCam>();
            if (extCam != null)
            {
                Debug.Log("Added OutlineEffect,Color Pulse", extCam.gameObject);
                extCam.gameObject.AddComponent<cakeslice.OutlineEffect>();
                extCam.gameObject.AddComponent<OutlineFillColorPulse>();

            }
        }
    }
    void AddPartHighlighter()
    {
        PartHighlighter hl = FindObjectOfType<PartHighlighter>();
        if (hl == null)
        {
            StepsManager stpMgr = FindObjectOfType<StepsManager>();
            if (stpMgr)
            {
                Debug.Log("Added PartHighlighter", stpMgr.gameObject);
                stpMgr.gameObject.AddComponent<PartHighlighter>();
            }
        }
    }
    private void OnEnable()
    {
        Steps steps = (Steps)target;

        if (steps.GetPartHighlighter() == null)
        {
            Debug.Log("IS Null");
            AddOutlineEffect();
            AddPartHighlighter();
        }
    }
}
#endif