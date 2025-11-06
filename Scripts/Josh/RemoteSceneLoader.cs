using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using mixpanel;

public class RemoteSceneLoader : MonoBehaviour
{
  //  public AssetReference scene;
    AsyncOperationHandle<SceneInstance> loader;
    UnityAction<Scene,LoadSceneMode> onComplete;
    [SerializeField] double totalBytes = 0, downloadedBytes = 0;
    [SerializeField] float percentComplete = 0,taskPercent;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] ModuleHarnessManager mHM;
    bool isLoading = false;
    [SerializeField] ScreenLinker linker;
    public string sceneAddress;
    private AutoModuleSelectHelper autoModule;
    private RemoteSceneLoader sceneLoader;
    private ManualSelector manualSelector;
    private CentralHarnessLinker harnessLinker;
    
    public static RemoteSceneLoader Get(string newName)
    {
        Debug.Log($"Loader from get method {newName}");
        string sceneName = System.IO.Path.GetFileName(newName);
        return new GameObject("Scene Loader for "+sceneName).AddComponent<RemoteSceneLoader>();        
    }
    public static RemoteSceneLoader Get()
    {
        return Get("Scene Loader");
    }
    public RemoteSceneLoader Get(string address,UnityAction<Scene,LoadSceneMode> onLoadComplete)
    {
        RemoteSceneLoader r = Get("Scene Loader:"+address);
        r.Load(address, onLoadComplete);
        return r;
    }

    private void Awake()
    {
        mHM = FindObjectOfType<ModuleHarnessManager>();
        linker = FindObjectOfType<ScreenLinker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
         autoModule = FindObjectOfType<AutoModuleSelectHelper>();
        manualSelector = FindObjectOfType<ManualSelector>();
    }
    public RemoteSceneLoader Load(string address, UnityAction<Scene, LoadSceneMode> onLoadComplete)
    {
        Debug.Log("Starting Scene Load for " + address);
       
        sceneAddress = address;
        onComplete = onLoadComplete;
       // GetLinker().GetScreenManager().SetLoadingState(true,End);
        StartCoroutine(StartLoading());        
        return this;
    }

    /* private IEnumerator StartLoading()
     {        
         // Obtain AsyncOperationHandle
         loader = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive, true);

         linker.loadingSpash.SetActive(true);

         yield return new WaitUntil(() => loader.IsDone);

         linker.mainCamera.SetActive(false);        
         Debug.Log($"Remote Scene Load StartLoading : {sceneAddress}");
         Debug.Log($"Loader Status : {loader.Status}");

 *//*        if (loader.Status == AsyncOperationStatus.Failed)
         {
             Debug.Log($"Inside Loader Status Consition : {loader.Status}");
             UIScreenManager uIScreenManager = FindObjectOfType<UIScreenManager>(true);
             mHM = FindObjectOfType<ModuleHarnessManager>(true);
             mHM.OnBack();
             uIScreenManager.SelectScreen(15);


             yield return null;
             #region Fallback

             Debug.Log("<color=red>Download Error:</color>" + loader.DebugName);
             #endregion
         }*//*

             GetLinker().GetScreenManager().loadingAnimated.SetActive(true);
             loader.Completed += RemoteSceneLoader_Completed;

             while (!loader.IsDone)
             {
                 UpdateDownloadStat(loader.GetDownloadStatus());
                 UpdateTaskPercent(loader.PercentComplete);
                 yield return null;
             }
             #region Fallback

             Debug.Log("<color=blue>Loaded:</color>" + loader.DebugName);
             #endregion




         // Pause Until successful

     }*/

    private IEnumerator StartLoading()
    {
        // Obtain AsyncOperationHandle
        GetLinker().GetScreenManager().loadingAnimated.SetActive(true);
        Debug.Log($"Remote Scene Load StartLoading : {sceneAddress}");
        loader = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive, true);
        Debug.Log($"Loader Status : {loader.Status}");
        if (loader.Status == AsyncOperationStatus.Failed)
        {
            StartScreen3dView startScreen3DView = FindObjectOfType<StartScreen3dView>(true);
            startScreen3DView.sceneSpecific.SetActive(true);
            startScreen3DView.networkWarnning.SetActive(true);
            loader.Completed += RemoteSceneLoader_Failed;
            //  yield return null;
        }
        else
        {
            StartScreen3dView startScreen3DView = FindObjectOfType<StartScreen3dView>(true);
            startScreen3DView.sceneSpecific.SetActive(false);
            loader.Completed += RemoteSceneLoader_Completed;
            //   yield return null;
        }
        while (!loader.IsDone)
        {
            UpdateDownloadStat(loader.GetDownloadStatus());
            UpdateTaskPercent(loader.PercentComplete);
            yield return null;
        }
        #region Fallback
        Debug.Log("<color=blue>Loaded:</color>" + loader.DebugName);
        #endregion
        // Pause Until successful
    }

    private void UpdateTaskPercent(float taskPerc)
    {
        taskPercent = taskPerc;
    }
    private void UpdateDownloadStat(DownloadStatus stat)
    {
        if (stat.DownloadedBytes > 0)
            downloadedBytes = stat.DownloadedBytes;
        if (stat.Percent > 0)
            percentComplete = stat.Percent;
        else
            percentComplete = taskPercent;
    }
    public void End()
    {
        isLoading = false;
        StopCoroutine(StartLoading());
        Addressables.Release(loader);
        if (loader.IsValid())
        Addressables.UnloadSceneAsync(loader,true);
     
        Destroy(gameObject);
        
    }
    private ScreenLinker GetLinker()
    {
        if (linker == null)
            linker = FindObjectOfType<ScreenLinker>();
        return linker;
    }
    private void Update()
    {
        //float perc = loader.PercentComplete;
        //  loadingText.text = "Loading : " + (percentComplete * 100).ToString("F1")+" %";
        if (isLoading)
        {
            GetLinker().GetScreenManager().Loading(percentComplete);
            GetLinker().GetScreenManager().DebugToScreen("Downloaded: " + (downloadedBytes / (1000000)).ToString("F2") + " mb");
        }
   //   fillImage.fillAmount=  loader.PercentComplete;
    }
    private void RemoteSceneLoader_Completed(AsyncOperationHandle<SceneInstance> obj)
    {
        Debug.Log("Loader status Failed but executed ");
        loader.Completed -= RemoteSceneLoader_Completed;
        
        //isLoading = false;
        GetLinker().GetScreenManager().loadingAnimated.SetActive(false);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GetLinker().GetScreenManager().SetLoadingState(false);
            Debug.Log("Loaded Scene");
            onComplete?.Invoke(obj.Result.Scene, LoadSceneMode.Additive);
        }
     
    }
    private void RemoteSceneLoader_Failed(AsyncOperationHandle<SceneInstance> obj)
    {
        loader.Completed -= RemoteSceneLoader_Failed;
        StopCoroutine(StartLoading());
        GetLinker().GetScreenManager().loadingAnimated.SetActive(false);
        UIScreenManager uIScreenManager = FindObjectOfType<UIScreenManager>(true);
        uIScreenManager.SelectScreen(15);
        //isLoading = false;

        isLoading = false;
        Destroy(gameObject);
    }
}
