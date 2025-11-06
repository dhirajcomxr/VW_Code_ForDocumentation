using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ModuleLoadManager : MonoBehaviour
{
    BundleLoader bundleLoader;
    [SerializeField] GameObject currentLoaded;
    UnityAction onDownloadComplete;
    [SerializeField] AppApiManager.ServerVehicleModuleFileRef selectedModuleRef;

    string curModName;
    public string modNameToSend;
    Steps main;
    VWSectionModule sectionModule;

    [SerializeField] ScreenLinker linker;
  [SerializeField]  AppApiManager.ServerVehicleModuleFileRef[] moduleReferences;
    private void Reset()
    {
        linker = FindObjectOfType<ScreenLinker>();
    }
    public bool ModulesAvailable() => moduleReferences.Length > 0;
    public bool SelectedModuleAvailable()
    {
        bool result = false;
        if (selectedModuleRef != null)
            result = selectedModuleRef.ModuleID.Length > 0;
        return result;

    }
    public void SetModuleReferences(AppApiManager.ServerVehicleModuleFileRef[] refs)
    {
        moduleReferences = refs;
    }
    public List<string> GetModuleNamesFor(string modName)
    {
        List<string> resultNames = new List<string>();
        for (int i = 0; i < moduleReferences.Length; i++)
        {
            if (moduleReferences[i].ModuleName.Contains(modName))
                resultNames.Add(moduleReferences[i].ModuleName);
        }
        return resultNames;
    }
    public void SelectModule(string modName, string modAddress)
    {
        Debug.Log("Mod Address = " + modAddress);
        selectedModuleRef = null;
        bool foundModule = false;
        for (int i = 0; i < moduleReferences.Length; i++)
        {
            if (modName.Length > 0)
            {
                curModName = modName;
                if (moduleReferences[i].ModuleName.Contains(modName))
                {
                    if (!foundModule)
                    {

                        selectedModuleRef = moduleReferences[i];
                        modName = selectedModuleRef.ModuleName;
                        foundModule = true;
                    }
                    else
                        Debug.LogError("Multiple Modules With Same Name!!");
                }
            }
            if (modAddress.Length > 0)
            {
                if (GetAddress(moduleReferences[i].ModuleAddress) == modAddress)
                {
                    selectedModuleRef = moduleReferences[i];
                    curModName = selectedModuleRef.ModuleName;
                    foundModule = true;
                }
            }
        }
        if (foundModule)
        {
            //Debug.Log("<color=blue>[LANGUAGE]</color>Selected Module: " + selectedModuleRef.ModuleName);
            modNameToSend = selectedModuleRef.ModuleName;
        }
        else 
        { 
            Debug.Log("<color=red>[LANGUAGE]" + moduleReferences.Length + "</color>Did not find Selected Module: " + modName + " or Address:" + modAddress);
        }
    }
    public void DownloadModule(string modName,UnityAction downloadComplete)
    {
        Debug.Log("Downloading " + modName + " via ModLoadMan");
        onDownloadComplete = downloadComplete;
        curModName = modName;
        bool foundModule = false;
        for (int i = 0; i < moduleReferences.Length; i++)
        {
            if (moduleReferences[i].ModuleName.Contains(modName))
            {
                if (!foundModule)
                {
                    selectedModuleRef = moduleReferences[i];
                    curModName = selectedModuleRef.ModuleName;
                    foundModule = true;
                }
                else
                    Debug.LogError("Multiple Modules With Same Name!!");
            }
        }
        if (foundModule)
        {
            Debug.Log("FOUND MODULE: " + modName);
            curModName = selectedModuleRef.ModuleName;
            DownloadSelectedModule();
        }
        else
            Debug.LogError("NO Module Found for " + modName);

    }
    void DownloadSelectedModule()
    {
        Debug.Log("Downloaded Module "+selectedModuleRef.ModuleName);
       // Mixpanel.Track("Module_Loaded" + selectedModuleRef.ModuleName);
        if (currentLoaded!=null)
        Destroy(currentLoaded);
        main = null;
        sectionModule = null;
        linker.GetScreenManager().SetLoadingState(true);
        DownloadFromAddress("Assets/Prefabs/Modules/" + selectedModuleRef.ModuleAddress + ".prefab", AssignmentPostDownload);
    }
    string GetAddress(string addressSection)
    {
        return "Assets/Prefabs/Modules/" + addressSection + ".prefab";
    }
    void AssignmentPostDownload()
    {
        onDownloadComplete?.Invoke();
        onDownloadComplete = null;
        linker.GetScreenManager().SetLoadingState(false);
        sectionModule = currentLoaded.GetComponent<VWSectionModule>();
        if (sectionModule != null)
        {
            if (sectionModule.stepMain != null)
                main = sectionModule.stepMain;
        }
        else
            main = currentLoaded.GetComponentInChildren<Steps>();
        if (main)
        {
                DownloadLanguageSupport();         
        }
        if(sectionModule!=null)
        linker.GetReferenceManager().UpdateReferencesTo(sectionModule);
        else
        {
            linker.GetReferenceManager().UpdateReferenceTo(currentLoaded);
            linker.GetScreenManager().SelectScreen(ScreenLinker.ScreenID.EXPLODED_VIEW_RNR);
     /*       linker.GetScreenManager().loadingAnimated.SetActive(true);
            linker.GetScreenManager().LoadingAnimationClose(5f);*/

        }
       
    }
    public void SetMainSteps(Steps newSteps)
    {
        main = newSteps;
    }
   public void DownloadLanguageSupport()
    {
        //  bool canTranslate = selectedModuleRef!=null;
        if (main == null)
        {
            Debug.LogError("Step scripts not found NOT");
        }

        int moduleIndex = 0;
        for(int i = 0; i< ManualSelector.mastersForTorque.vehicle_modules.Length; i++)
        {
            if(selectedModuleRef.ModuleID == ManualSelector.mastersForTorque.vehicle_modules[i].ModuleID)
            {
                moduleIndex = i;
            }
        }

        linker.GetLocalizationSupport().TranslateNow(main, selectedModuleRef.AssemblyFileName, selectedModuleRef.DismantlingFileName);
        //linker.GetLocalizationSupport().TranslateNowForTorque(ManualSelector.mastersForTorque.vehicle_modules[moduleIndex].AssemblyFileName, ManualSelector.mastersForTorque.vehicle_modules[moduleIndex].DismantlingFileName);
        StartCoroutine(TranslateCorr(moduleIndex));
        //if (canTranslate && selectedModuleRef.DismantlingFileName.Length > 0)
        //{
        //    linker.GetLocalizationSupport().TranslateNow(main, selectedModuleRef.AssemblyFileName, selectedModuleRef.DismantlingFileName);
        //}
        //else 
        //if(linker.GetLocalizationSupport()
        //if (linker.GetLocalizationSupport().selectedLangauge != LocalizationSupport.Language.English)
        //{
        //    linker.GetScreenManager().Toast(linker.GetLocalizationSupport().selectedLangauge.ToString() + " translation for " + curModName + " coming soon");
        //}
    }

    IEnumerator TranslateCorr(int index)
    {
        yield return new WaitForSeconds(2f);
        linker.GetLocalizationSupport().TranslateNowForTorque(ManualSelector.mastersForTorque.vehicle_modules[index].AssemblyFileName, ManualSelector.mastersForTorque.vehicle_modules[index].DismantlingFileName);

    }

    public void DownloadFromAddress(string address,UnityAction returnFunction)
    {
        if (bundleLoader != null)
        {
            Debug.Log("<B>Crash : Download From Address</b>");
            bundleLoader.Stop();
            Debug.Log("<B>Crash : Crashed Here 1</b>");
            Resources.UnloadUnusedAssets();
            Debug.Log("<B>Crash : Crashed Here 2</b>");
        }
        bundleLoader = BundleLoader.Get().load(address)
              .onCompleteCall((add, obj) => {
                  //   Instantiate(obj);
                  Debug.Log("Loaded:" + obj.name + " from " + add);
                  currentLoaded = Instantiate(obj,transform);
                  returnFunction?.Invoke();
              })
              .onProgress(LoadPercent)
              .onErrorCall(LoadError).startRef();
    }

    private void LoadError()
    {
        linker.GetScreenManager().Toast("Error downloading.");
    }

    private void LoadPercent(float arg0)
    {
        linker.GetScreenManager().Loading(arg0);
    }
}
