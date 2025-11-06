using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VWModuleManager : MonoBehaviour
{
    [SerializeField] string selectedModule = "";
    [Header("Loading Screen Settings")]
    [SerializeField] GameObject[] createdObjs;
    [SerializeField] bool doNotDownload = false;
    [SerializeField] bool usePrefabLoading = false;
    [SerializeField] GameObject[] prefabs;
  //  [SerializeField] GameObject loadingPanelObject;
  //  [SerializeField] Text loadingText;
  //  [SerializeField] Image fillImage;
    float loadingPercDelay = 0.2f, loadPercDelayStat;
    bool isLoading = false;
    [SerializeField] int loadId = -1;
    [SerializeField] bool loadNow = false;
    [SerializeField] float completedPerc = 0f;
    public delegate void OnSelect(int id);
    public event OnSelect onSelect;
    [SerializeField] LocalizationSupport localizationSupport;
    #region References
    [Header("Update References")]
    [SerializeField] VWReferencesManager referenceManager;
    [SerializeField] AssetLoader assetLoader;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] public VWSectionModule curModule;
    int lastModule = -1, curLoadedModuleId = -1;
    
    #endregion
    // Start is called before the first frame update
    private void Reset()
    {
       _FindScriptReferences();
    }
    private void Start()
    {
        assetLoader.onDebug += screenManager.DebugToScreen;
    }
    public int GetTotalPrefabs() => prefabs.Length;
  //  public int GetModuleIdFromName(string moduleName) => assetLoader.GetIdForModuleName(moduleName);
    void _FindScriptReferences()
    {
        if(assetLoader==null)
        assetLoader = FindObjectOfType<AssetLoader>();
        if (referenceManager == null)
            referenceManager = FindObjectOfType<VWReferencesManager>();
        if (screenManager == null)
            screenManager = FindObjectOfType<UIScreenManager>();
        //Debug.Log("Updated Script References");
    }

    public string GetSelectedModuleName()
    {
        Debug.Log("Cur Module ............"+curModule.ToString());
        if (selectedModule.Length < 1)
        {
            if (curModule)
                selectedModule = curModule.GetModuleName();
            if(selectedModule.Length<1)
            selectedModule = assetLoader.GetNameForModule(curLoadedModuleId);
        }
        return selectedModule;
    }
 //   public VWSectionModule GetCurrentModule() => GetSelectedModule();
    public VWSectionModule GetSelectedModule() => curModule;
    void init()
    {
        createdObjs = new GameObject[assetLoader.GetTotalReferences()];
        _FindScriptReferences();
    }
    void SelectLoadedObject(int id)
    {
        //Debug.Log("ID:" + id);

        if (!doNotDownload)
        {
            if (assetLoader.GetIfDownloaded(id) != null)
            {
                Debug.Log("ID:" + id);
                if (createdObjs.Length < assetLoader.GetTotalReferences())
                    init();
                createdObjs[id] = assetLoader.GetIfDownloaded(id);
            }
        }
        VWSectionModule mod = createdObjs[id].GetComponent<VWSectionModule>();
     
        for (int i = 0; i < createdObjs.Length; i++)
        {
            if (createdObjs[i] != null)
                if (i != id)
                    createdObjs[i].SetActive(false);
                else
                    createdObjs[i].SetActive(true);
        }

            if (mod)
            {
                selectedModule = mod.GetModuleName();
                curModule = mod;
                curLoadedModuleId = id;
                if (mod.GetModuleName().Length < 1)
                   selectedModule= assetLoader.GetNameForModule(id);
                referenceManager.UpdateReferencesTo(mod);
                referenceManager.UpdateName();
            }
            else
            {         
                _UpdateReferences(createdObjs[id]);
            }

        isLoading = false;
        screenManager.SetLoadingState(true);
        //Debug.Log("Loading splash ON");
        localizationSupport.Invoke("Anima", 5.0f);
        //  screenManager.SetLoadingState(isLoading);
        //Debug.LogError("SelectScreen.......19");
        screenManager.SelectScreen(ScreenLinker.ScreenID.EXPLODED_VIEW_RNR);
        //  loadingPanelObject.SetActive(false);
    }
    public int GetLastLoadedModuleIndex() => lastModule;
    public void SelectLastObject()
    {
        SelectLoadedObject(lastModule);
    }
    public void DeselectAll()
    {
       // if (usePrefabLoading)
            ResetStates();
        //else
        //{
            for (int i = 0; i < createdObjs.Length; i++)
            {
                if (createdObjs[i] != null)
                {
                    createdObjs[i].SetActive(false);
                Debug.LogWarning("********* Object Disabled *********");
                //      Destroy(createdObjs[i]);
                StartCoroutine(ClearModuleCorr(createdObjs[i]));
            }
            }
     //   }
    }
    void _UpdateReferences(GameObject g)
    {
        referenceManager.UpdateReferenceTo(g);
    }

    public int lastModuleId = 0;
    public bool isBundleLoaderClosed = false;
    public void SelectModule(int moduleId)
    {
        lastModuleId = moduleId;
        if (usePrefabLoading)
        {
            if (screenManager)

                if (moduleId < prefabs.Length && moduleId >= 0)
                {
                    Debug.Log("usePrefabLoading........");
                    screenManager.SetLoadingState(true,assetLoader.StopBundleLoading);
                    OnPrefabLoaded(moduleId, Instantiate(prefabs[moduleId]));
                    selectedModule = assetLoader.GetNameForModule(moduleId);
                }
        }
        else
        {
            Debug.Log($"Selected Module : <b>{moduleId}</b>");
            if (moduleId >= 0)
            {
                if(createdObjs.Length>0)
                if(createdObjs[lastModule]!=null)
                        if(lastModule!=moduleId)
                ClearModule(createdObjs[lastModule]);
                lastModule = moduleId;
            }
            if (createdObjs == null)
                init();
            if (createdObjs.Length < 1)
                init();
        
            if (doNotDownload)
            {
                Debug.Log("doNotDownload........");
                if (createdObjs[moduleId] != null)
                    OnObjectLoad(moduleId, createdObjs[moduleId]);
            }
            else
            {
                GameObject isD = assetLoader.GetIfDownloaded(moduleId);
                if (isD != null)
                {
                    selectedModule = assetLoader.GetNameForModule(moduleId);
                    OnObjectLoad(moduleId, isD);
                    Debug.Log("ID exists: " + moduleId);
                    //  SelectLoadedObject(moduleId);
                }
                else
                {
                    selectedModule = assetLoader.GetNameForModule(moduleId);
                    assetLoader.GetModuleObject(moduleId, OnObjectLoad, false);
                    screenManager.SetLoadingState(true,OnCancelDownload);
                    //    loadingPanelObject.SetActive(true);
                    isLoading = true;
                    
                }
            }
        }
    }
    public void crcLoadModule()
    {
        if (isBundleLoaderClosed)
        {
            SelectModule(lastModuleId);
            GameObject isD = assetLoader.GetIfDownloaded(lastModuleId);
            if (isD != null)
            {
                selectedModule = assetLoader.GetNameForModule(lastModuleId);
                OnObjectLoad(lastModuleId, isD);
                Debug.Log("ID exists: " + lastModuleId);
                //  SelectLoadedObject(moduleId);
            }
            else
            {
                selectedModule = assetLoader.GetNameForModule(lastModuleId);
                assetLoader.GetModuleObject(lastModuleId, OnObjectLoad, false);
                screenManager.SetLoadingState(true, OnCancelDownload);
                //    loadingPanelObject.SetActive(true);
                isLoading = true;

            }
            Debug.Log($"Crash : Is Bundle Loader Closed {isBundleLoaderClosed}");
            isBundleLoaderClosed = false;
        }

    }
    public void clearModulelist()
    {
        if(createdObjs.Length > 0)
            ClearModule(createdObjs[lastModule]);
    }
     void ClearModule(GameObject g)
    {
        Debug.LogWarning("********* Object Destroyed *********");
        Destroy(g);
        StartCoroutine(RunGC());
    }

    IEnumerator ClearModuleCorr(GameObject g)
    {
        yield return new WaitForSeconds(1f);
        Debug.LogWarning("********* Object Destroyed Corr *********");
        Destroy(g);
        StartCoroutine(RunGC());
    }

    IEnumerator RunGC()
    {
        yield return new WaitForEndOfFrame();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    public void OpenAfterLoading(int screenNum)
    {
        Debug.Log("Open After Loading : " + screenNum);       
        screenManager.OpenAfterLoading(screenNum);
     
    }
    public VWSectionModule GetLastLoadedModule()
        => lastLoadedPrefab ? lastLoadedPrefab.GetComponent<VWSectionModule>() : null;
    public void ResetStates()
    {
        if (lastLoadedPrefab != null)
        {
            ClearModule(lastLoadedPrefab);
        }
            
    }
    GameObject lastLoadedPrefab;
    void OnPrefabLoaded(int id,GameObject g)
    {
        ResetStates();
        lastLoadedPrefab = g;
        VWSectionModule mod = g.GetComponent<VWSectionModule>();
        if (mod)
        {
            if (mod.GetModuleName().Length > 0)
                selectedModule = mod.GetModuleName();
            else
            {
                selectedModule = assetLoader.GetNameForModule(id);
            }
            if (selectedModule.Length < 1)
                selectedModule = assetLoader.GetNameForModule(id);
            ObjectReferenceMapper.Instance.Init();
            if(referenceManager)
            referenceManager.UpdateReferencesTo(mod);
          
        }

        isLoading = false;
        onSelect?.Invoke(id);
        if (screenManager)
        {
            Debug.LogError("Loading animation Close");
            screenManager.SetLoadingState(false);
          
        }
 
     
    }
    public void OnCancelDownload()
    {
        isLoading = false;
        completedPerc = 100;
      
        screenManager.SetLoadingState(false);
        assetLoader.StopBundleLoading();
        screenManager.SelectScreen(15);
    }
    void OnObjectLoad(int id,GameObject g)
    {
          
        if (createdObjs[id] != null)
        {
       //   Destroy(createdObjs[id]);
            Debug.Log("<color=red>Error: Object exists</color>");
        }
        if (g == null)
            Debug.Log("<color=red> NULL </color>");
        //   else
        //{
        createdObjs[id] = g;
        g.transform.parent = transform;
        //  }

        isLoading = false;
       
       // Debug.LogError("Loaded " + id);
        SelectLoadedObject(id);       
        onSelect?.Invoke(id);
        
    }

    // Update is called once per frame
    void Update()
    {

        if (isLoading)
        {
            loadPercDelayStat += Time.deltaTime;
            if (loadPercDelayStat > loadingPercDelay)
            {
                UpdateLoadingPercent();
               
                loadPercDelayStat = 0;
            }
        }

        if (assetLoader.bundleLoader)
        {
            assetLoader.bundleLoader.CRCTesting();
        }

        crcLoadModule();
    }
    void UpdateLoadingPercent()
    {
        completedPerc = assetLoader.GetLoadingPercent();
        if(completedPerc<1)
        screenManager.Loading(completedPerc);
        //if (fillImage!=null)
        //    fillImage.fillAmount = completedPerc;
        //if (loadingText != null)
        //    loadingText.text = "Loading: " + (completedPerc * 100).ToString("F1");
    }
  
    void OnLoadGo(GameObject go)
    {
        go.transform.parent = transform;
        Debug.Log("Got GameObject:" + go);
    }
}
