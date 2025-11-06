using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using mixpanel;
public class LocalizationSupport : MonoBehaviour
{
    public bool enableTranslation = false;
    public bool testHead = false;
    public string testLink;
    public Language selectedLangauge;
    public Steps main;
   
    [SerializeField] bool translateNow = false;
    public UnityAction onComplete;
    
    int totalFiles;
    public enum Language
    {
        English,Hindi, Gujarati,Tamil
    }
    public enum LanguageShortCode
    {
       en, hi,gu, ta
    }
  [SerializeField]  AppApiManager apiManager;
    [SerializeField] ScreenLinker linker;


    //Get a language using Enum from the above function
    public void LanguageList(Dropdown dropdown)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(Enum.GetNames(typeof( Language))));
        dropdown.SetValueWithoutNotify((int)selectedLangauge);
        dropdown.onValueChanged.AddListener(SetLangauge);
      
    }
    public void SaveLanguage(Dropdown dropdown)
    {
        SetLangauge(dropdown.value);
    }
    public void SetLangauge(int option) 
    {
        selectedLangauge = (Language)option;
        apiManager.SetLanguage(GetSelectedLanguageShortCode());
    }
    public string GetSelectedLanguageShortCode()
    {
        string result = ((LanguageShortCode)((int)selectedLangauge)).ToString();
        Mixpanel.Track("Language_Selected","language_type",result);
        Debug.Log("Selected Language is :"+result);
        return result;
    }
   
    int filesDone = 0;
    public void TranslateNow(Steps stepsMain, string asmFileLink, string dismFileLink)
    {
        //Debug.LogError("Step Main ="+stepsMain+"  asmFileLink ="+asmFileLink+"   dismFileLink ="+dismFileLink);
        totalFiles = 0;

        main = stepsMain;
        if (asmFileLink.Length > 0)
        {
            Debug.Log("<color=blue>[TRANSLATE]</color> ASM LINK: " + asmFileLink);
            Debug.Log("<color=blue>[TRANSLATE]</color> ASM StepCOUNT: " + asmFileLink.Length);
            totalFiles++;
            Downloader.Get().Download(asmFileLink).OnComplete(DownloadedAsm);
        }
        if (dismFileLink.Length > 0)
        {
            totalFiles++;
            Debug.Log("<color=blue>[TRANSLATE]</color> DISM LINK: " + dismFileLink);
              Debug.Log("<color=blue>[TRANSLATE]</color> DSM StepCOUNT: " + dismFileLink.Length);
            Downloader.Get().Download(dismFileLink).OnComplete(DownloadedDism);
        }
        if (totalFiles > 0)
        {
            linker.GetScreenManager().SetLoadingState(true);
            UpdateLoadProgress();
        }
        else
        {
            if (selectedLangauge != Language.English)
                linker.GetScreenManager().Toast(selectedLangauge.ToString()+" translation for this module coming soon..");
            Complete();
        }
    }

    public void TranslateNowForTorque(string asmFileLink, string dismFileLink)
    {
        //Debug.LogError("Step Main ="+stepsMain+"  asmFileLink ="+asmFileLink+"   dismFileLink ="+dismFileLink);
        totalFiles = 0;
        if (asmFileLink.Length > 0)
        {
            //Debug.LogWarning("<color=blue>[TRANSLATE]</color> ASM LINK: " + asmFileLink);
            totalFiles++;
            Downloader.Get().Download(asmFileLink).OnComplete(DownloadedAsmForTorque);
        }
        if (dismFileLink.Length > 0)
        {
            totalFiles++;
            //Debug.LogWarning("<color=blue>[TRANSLATE]</color> DISM LINK: " + dismFileLink);
            Downloader.Get().Download(dismFileLink).OnComplete(DownloadedDismTorque);
        }
        /*if (totalFiles > 0)
        {
            linker.GetScreenManager().SetLoadingState(true);
            UpdateLoadProgress();
        }
        else
        {
            if (selectedLangauge != Language.English)
                linker.GetScreenManager().Toast(selectedLangauge.ToString() + " translation for this module coming soon..");
            Complete();
        }*/
    }

    void DownloadedAsm(Downloader downloader)
    {
        List<Step> asmSteps = linker.GetStepsEximProcessor().GetLocaleStepsFromFile(new TextAsset(downloader.textData));
        downloader.Close();
        Debug.LogError($"ASM Count : {asmSteps.Count}");
        Debug.LogError(main.assemblySteps.Count + " Main step count Count     ");
        Mixpanel.Track("StepList_Loaded ","assembly",asmSteps.Count);
        if (main.assemblySteps.Count == asmSteps.Count)
            linker.GetStepsEximProcessor().Translate(main.assemblySteps, asmSteps);
        else
            linker.GetScreenManager().DebugToScreen("size mismatch for assembly!");
        filesDone++;
        UpdateLoadProgress();
    }
    void DownloadedDism(Downloader downloader)
    {
        List<Step> dismSteps = linker.GetStepsEximProcessor().GetLocaleStepsFromFile(new TextAsset(downloader.textData));
        downloader.Close();
        Debug.LogError($"DSM Count : {dismSteps.Count}");
        Debug.LogError(main.steps.Count + " Main  dis  step count Count     ");
        Mixpanel.Track("StepList_Loaded","disassembly",dismSteps.Count);
        if (main.steps.Count==dismSteps.Count)
            linker.GetStepsEximProcessor().Translate(main.steps, dismSteps);
        else
            linker.GetScreenManager().DebugToScreen("size mismatch for dismantling!");
        filesDone++;
        UpdateLoadProgress();
    }

    void DownloadedAsmForTorque(Downloader downloader)
    {
        List<Step> asmSteps = linker.GetStepsEximProcessor().GetLocaleStepsFromFile(new TextAsset(downloader.textData));
        downloader.Close();
        //Debug.LogError(asmSteps.Count + " ASM Count     ");
        //  Debug.LogError(main.assemblySteps.Count + " Main step count Count     ");
        //Mixpanel.Track("StepList_Loaded ", "assembly", asmSteps.Count);
        if (main.assemblySteps.Count == asmSteps.Count)
            linker.GetStepsEximProcessor().TranslateForTorque(main.assemblySteps, asmSteps);
        //else
        //    linker.GetScreenManager().DebugToScreen("size mismatch for assembly!");
        //filesDone++;
        //UpdateLoadProgress();
    }
    void DownloadedDismTorque(Downloader downloader)
    {
        List<Step> dismSteps = linker.GetStepsEximProcessor().GetLocaleStepsFromFile(new TextAsset(downloader.textData));
        downloader.Close();
        //Debug.LogError(dismSteps.Count + " DSM Count     ");
        //Debug.LogError(main.steps.Count + " Main  dis  step count Count     ");
        //Mixpanel.Track("StepList_Loaded", "disassembly", dismSteps.Count);
        if (main.steps.Count == dismSteps.Count)
            linker.GetStepsEximProcessor().TranslateForTorque(main.steps, dismSteps);
        //else
        //    linker.GetScreenManager().DebugToScreen("size mismatch for dismantling!");
        //filesDone++;
        //UpdateLoadProgress();
    }
    void UpdateLoadProgress()
    {
       // Debug.LogError("Loading bar....");
        linker.GetScreenManager().Loading(100*(filesDone+0.1f/totalFiles)* 1.0f);
        if (filesDone >= totalFiles)
        {
            //linker.GetScreenManager().loadingAnimated.SetActive(true);
            Complete();
         
        }
       //Debug.Log("Loading splash ON");
        linker.GetScreenManager().loadingAnimated.SetActive(true);
        Invoke("Anima", 3.0f);
    }
    public void Anima()
    {
      //  linker.GetScreenManager().loadingAnimated.SetActive(false);
        linker.GetScreenManager().SetLoadingState(false);
        //Debug.Log("Loading splash OFF");
    }
    void Complete()
    {   
        onComplete?.Invoke();
            onComplete = null;
    }
 
}
