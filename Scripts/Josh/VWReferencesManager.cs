using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VWReferencesManager : MonoBehaviour
{
    //  [SerializeField] AssetLoader assetLoader;
    [SerializeField] bool loadFRef = false,loadAsmDism=false;
    [SerializeField] string selectedPartName = "",moduleName="";
    [Header("References")]
    [SerializeField] ExteriorCam cam;
    [SerializeField] GameObject camPivot;
    [SerializeField] GameObject searchParts;
    [SerializeField] PartListTable partList;
    [SerializeField] PartSequenceEximProcessor sequenceExim;
    [SerializeField] PartIdToStepFunction idToStepFn;
    [SerializeField] StepsEximProcessor stepsExim;
    [SerializeField] StepListScreen stepListScreen;
    [SerializeField] StepManagerAssignments stepMgrAssnmnt;
    [SerializeField] ExplodedView explodedView;
    [SerializeField] Steps stepsMain;
    [SerializeField] PartListSearchable searchableList;
    [SerializeField] PartHighlighter highlighter;
    [SerializeField] ScreenLinker screenLinker; 
    [SerializeField] UIScreenManager screenManager;
   [SerializeField] VWSectionModule vwsm;
    [SerializeField] VWModuleManager moduleManager;
    IEnumerator Start() {
        yield return new WaitForSeconds(1f);
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    public void SetSelectedPart(string sel)
        => selectedPartName = sel;
    public void UpdateName()
    {
        SetSelectedPart(GetSelectedName());
    }
    public string GetSelectedName()
    {
        //Debug.Log("PING!");
        string result = selectedPartName;
        if (idToStepFn)
        {
            result = idToStepFn.PartName;
        }
        if (result.Length < 1)
        {
            if (selectedPartName.Length > 0)
                result = moduleName;        
        }
        else
            selectedPartName = result;
        if (result.Length < 1)
        {
            moduleManager.GetSelectedModuleName();
            Debug.LogError("No Name!");
        }
        return result;
    }
    #region Functions For UI
    public void SetExplodedView()
    {
        Debug.Log("<color=blue>[VW-REF]</color>Setting Exploded View!");
        if (explodedView != null)
        {
            SetCameraTargetTo(explodedView.transform);
            idToStepFn.GoToExplodedView();
        }
        else
            SetStepsView();
    }
    public void FastForwardToStep(int s)
    {
        if (stepsMain)
            stepsMain.FastForwardTo(s);
        Debug.Log(s + "step value");
    }
    public void SetStepsViewIfNot()
    {
        if(!stepsMain)
        if (!stepsMain.gameObject.activeSelf)
            SetStepsView();
    }
    public void ShowSearchParts()
    {
       // if (searchParts)
       // {
            if (explodedView)
            {
                searchParts.SetActive(true);
            }
      //  }
    }
    public void SetStepsView()
    {
        ResetCamToSteps();
        idToStepFn.GoToStepsView();
    }
    public void ResetCamToSteps()
    {
        if (stepsMain)
            SetCameraTargetTo(stepsMain.transform);
    }
    public void UpdateStepsOverview()
    {
        stepListScreen.SetStepsReference(stepsMain);
    }
    public void SetModule(int moduleNum) { }
    void SetCameraTargetTo(Transform t)
    {
        if (cam)
            cam.target = t;
    }
    public ExplodedView GetExplodedView() => explodedView;
    public Steps GetSteps() => stepsMain;
    public void LoadExplodedViewForModule()
    {   
       // Debug.LogError("LoadExplodedViewForModule");
        Invoke("AnimationLoadingClose",1f);
        Invoke("ResetAndLoadExplodedView",3f);
    }
    void ResetAndLoadExplodedView()
    {
        if (explodedView != null)
        {           
            SetExplodedView();
            ExplodedView_Reset_Full();
            searchableList.ResetResults();         
        }
    }

 
    void DisableAllModules() {
        stepsMain.gameObject.SetActive(false);
        explodedView.gameObject.SetActive(false);
        //Use Module Manager
    }
    public void ExplodedView_Reset()
    {
        Debug.Log("<color=blue>Reset Exploded View State once</color>");
        if (explodedView != null)
        {
            GetExplodedView().ResetOneLevel();
            if (sequenceExim != null)
            {
                bool alreadyReset = sequenceExim.IsCurAsmEqualToSavedMainAsm() && sequenceExim.IsCurDismEqualToSavedMainDism();
                if (!alreadyReset)
                {
                    Debug.Log("<color=blue>Reset Exploded View Sequence </color>");
                    sequenceExim.ResetSequence();
                    sequenceExim.SetSelectionToSteps();
                }
            }
        }
        else
        {
            SetStepsView();
        }
    }

    public void ExplodedView_Reset_Full()
    {
        Debug.Log("<color=blue>Reset Exploded View State</color>");
        if (explodedView != null)
        {// REMOVE AFTER
            GetExplodedView().ResetFull();
            if (sequenceExim != null)
            {
                sequenceExim.ResetSequence();
                sequenceExim.SetSelectionToSteps();
            }
        }
    }

    public void ExplodedView_SetActive(bool state)
    {
        if (state)
            DisableAllModules();
        if(explodedView!=null)
        explodedView.gameObject.SetActive(state);
    }
    #endregion
    public void StepsIfNullExplodedView()
    {
        if (explodedView == null)
            SetStepsView();
    }
    #region ReferenceManager Functions
    public void UpdateReferences()
    {
        _FindScriptReferences();
        UpdateReferenceTo(null);
    }
    private void Reset()
    {
        _FindScriptReferences();
        //   UpdateReferences();
    }
    void _FindScriptReferences()
    {
        //if (assetLoader == null)
        //    assetLoader = FindObjectOfType<AssetLoader>();
        if (cam == null)
            cam = FindObjectOfType<ExteriorCam>();
        if (stepsExim == null)
            stepsExim = FindObjectOfType<StepsEximProcessor>();
        if (sequenceExim == null)
            sequenceExim = FindObjectOfType<PartSequenceEximProcessor>();
        if (idToStepFn == null)
            idToStepFn = FindObjectOfType<PartIdToStepFunction>();
        if (stepListScreen == null)
            stepListScreen = FindObjectOfType<StepListScreen>();
        if (stepMgrAssnmnt == null)
            stepMgrAssnmnt = FindObjectOfType<StepManagerAssignments>();
        Debug.Log("Updated Script References");
    }
    public void UpdateReferenceTo(GameObject g) => _UpdateReferences(g);
    public void UpdateReferencesTo(VWSectionModule module) => _UpdateReferences(module);
    void _UpdateReferences(GameObject g)
    {
        Debug.Log("GNAME: " + g.name);
        if (cam)
        {
            cam.target = g.transform;
            // reset 
            cam.enabled = false;
            cam.enabled = true;
        }
        if (g == null)
        {
            Debug.LogError("<color=red> NULL !</color>");
            g = GetRequiredGameObject();
        }
        Steps gSteps = g.GetComponentInChildren<Steps>();
        if (gSteps == null)
            gSteps = g.GetComponent<Steps>();
        if (gSteps != null)
            SetForStepsRnR(gSteps);
        else
            Debug.Log("NO STEPS FOUND");

        ExplodedView gExplodedView = g.GetComponentInChildren<ExplodedView>();
        if (gExplodedView != null)
            SetforExlodedView(gExplodedView);
        else
        Debug.Log("NO Exploded View");
                   
        SetExplodedView();
    }
   
    void _UpdateReferences(VWSectionModule module)
    {
        Debug.Log($"Updating via Module: <b>{module.gameObject.name} : {module.gameObject}</b>");
        //Debug.Log("STK: 0");
        if (cam)
        {
            if(module.explodedView)
            cam.target = module.explodedView.transform;
            // reset 
            cam.enabled = false;
            cam.enabled = true;
        }
        //Debug.Log("STK: 1");
        Steps gSteps = module.stepMain;
        ExplodedView gExplodedView = module.explodedView;
        SetFileReferences(module);
        //Debug.Log("STK: 2");
        if (gSteps)
        {
            //Debug.Log("STK: 3");
            Debug.Log("Localization in Process..");
            screenLinker.GetModuleLoader().SetMainSteps(gSteps);
            //Debug.Log("STK: 4");
            //   if(screenLinker.GetLocalizationSupport().enableTranslation)
            if (screenLinker.GetModuleLoader().SelectedModuleAvailable())
            {
                //Debug.Log("STK: 5");
                screenLinker.GetLocalizationSupport().onComplete = () =>
                {
                    //Debug.Log("STK: 6");
                    Debug.Log("<color=red>[VWREF]</color> Complete Localization");
                    SetForStepsRnR(gSteps);
                };
                //Debug.Log("STK: 7");
                screenLinker.GetModuleLoader().DownloadLanguageSupport();
            }
            else
                SetForStepsRnR(gSteps);
            //if(screenLinker.GetLocalizationSupport().enableTranslation)
            //screenLinker.GetLocalizationSupport().TranslateNow(gSteps); 
            //      SetForStepsRnR(gSteps);
            //Debug.Log("STK: 9");
        }
        else
        {
            //Debug.Log("STK: 10");
            vwsm = FindObjectOfType<VWSectionModule>();
            if (vwsm)
            {
                if (vwsm.stepMain)
                    SetForStepsRnR(vwsm.stepMain);
                SetFileReferences(vwsm);
            }
            else
                Debug.Log("NO STEPS FOUND");

            //Debug.Log("STK: 11");
        }
        //Debug.Log("STK: 12");

        if (gExplodedView != null)
        {
            if (module.servicablePartsFile)
                gExplodedView.partListFile = module.servicablePartsFile;
            SetforExlodedView(gExplodedView);
        }
        else
            Debug.Log("NO Exploded View");

        SetExplodedView();
    }
    void SetFileReferences(VWSectionModule module)
    {
        moduleName = "";
        if (module != null)
            if (module.GetModuleName().Length > 0)
                moduleName = module.GetModuleName();
        if(moduleName.Length<1)
        moduleName = moduleManager.GetSelectedModuleName();
        if (module.explodedView != null && module.stepMain != null)
        {
            sequenceExim.Clear();
            module.stepMain.gameObject.SetActive(false);
            module.explodedView.gameObject.SetActive(true);
        }
        selectedPartName = moduleName;
        vwsm = module;
        if (loadFRef)
        {
            Invoke("SetFRef", 1f);
        }
   
    }
    void SetFRef()
    {
        //Debug.Log("<color=blue> File Ref </color>");
   // GetLocalisedSteps();
        if (partList)
            if (vwsm.servicablePartsFile != null)
            {
                if (explodedView == null)
                    explodedView = vwsm.explodedView;
                if (explodedView != null)
                    explodedView.partListFile = vwsm.servicablePartsFile;
                else
                    Debug.Log("NO Exploded view");
                partList.LoadData(vwsm.servicablePartsFile);
            }
        if (sequenceExim)
        {
            sequenceExim.main = vwsm.stepMain;
            if (vwsm.partSequenceFile != null)
                sequenceExim.LoadData(vwsm.partSequenceFile);
        }
        if (stepsExim)
        {
            if (loadAsmDism)
            {
                stepsExim.main = vwsm.stepMain;
                //  vwsm = vwsm;
                Invoke("LoadStepData", 2f);
                //if(module.assemblyFile!=null && module.dismantlingFile!=null)
                //stepsExim.LoadData(module.assemblyFile, module.dismantlingFile);
                //sequenceExim.LoadStepsFrom(module.stepMain);
            }
        }
    }
 

    private void OnTranslationRecieved(string arg0)
    {
        Debug.Log("Recieved Translation :" + arg0);
    }

    void LoadStepData()
    {
        Debug.Log("<color=blue> Load Step Data </color>");
        if (vwsm.assemblyFile != null && vwsm.dismantlingFile != null)
        stepsExim.LoadData(vwsm.assemblyFile, vwsm.dismantlingFile);
        sequenceExim.LoadStepsFrom(vwsm.stepMain);

    }
    void SetforExlodedView(ExplodedView gExplodedView)
    {

        explodedView = gExplodedView;
        idToStepFn.explodedView = new GameObject[] { explodedView.gameObject };
        
        if (stepMgrAssnmnt != null)
            gExplodedView.resetOneLevelUi = stepMgrAssnmnt.explodedViewResetUi;
        gExplodedView.idToStep = idToStepFn;
        gExplodedView.cam = cam;
        cam.target = gExplodedView.transform;
        gExplodedView.cameraPivot = gExplodedView.gameObject;
        gExplodedView.highlighter = highlighter;
        gExplodedView.Load();
      //  gExplodedView.label = stepMgrAssnmnt.explodedViewLabel;
    }
    void SetForStepsRnR(Steps gSteps)
    {
        stepsMain = gSteps;
        Debug.Log("Has Steps with " + gSteps.GetTotalSteps() + " steps");
      
        if (idToStepFn)
            idToStepFn.stepsView = new GameObject[] { gSteps.gameObject };
        if (stepListScreen != null)
            stepListScreen.SetStepsReference(gSteps);
        if (stepMgrAssnmnt != null)
            stepMgrAssnmnt.stepMain = gSteps;
        if (screenLinker)
            screenLinker.SetStepReference(gSteps);
    }
   GameObject GetRequiredGameObject()
    {
        GameObject g = null;
        Debug.LogError("<color=red>No Gameobject to Update References, Attempting Update again..</color>");
        vwsm = FindObjectOfType<VWSectionModule>();
        Steps sG = FindObjectOfType<Steps>();
        ExplodedView exG = FindObjectOfType<ExplodedView>();
        if (sG != null)
            g = sG.gameObject;
        if (exG != null)
            g = exG.gameObject;
        return g;
    }
    #endregion
}
