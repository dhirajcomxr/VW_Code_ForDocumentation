using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// Sample Use:
/// var  bundleLoader = BundleLoader.Get().load(nameAddresses[i].address)
///                  .onCompleteCall(DownloadCompleteFn); //with parameters(gameobject)
    
/// </summary>
public class BundleLoader : MonoBehaviour
{
    ModuleLoadManager moduleLoad;
        GameObject loader_Go;
       [SerializeField] string bundleAddress;
        GameObject result;
    public long downloadedBytes = 0,totalSize=0;
    float progressPercent = 0;
    AsyncOperationHandle<long> sizeCalculator;
        AsyncOperationHandle<GameObject> objectLoader;
        UnityAction<GameObject> onDownloadComplete;
    UnityAction<string, GameObject> onDownloadAddressAndResult;
        UnityAction<float> onProgressPercent;
    UnityAction<string> onErrorForAddress;
        UnityAction onError;

    VWModuleManager vwm;
    StartScreen3dView SS3V;


    public bool bundleLoaded;
    public static BundleLoader Get()
    {
        return new GameObject("Bundle Loader").AddComponent<BundleLoader>();
    }
    public BundleLoader Get(string address)
    {
        BundleLoader b = Get();
        b.load(address, Complete);
        return b;
    }

    public void start()
    {
        moduleLoad = FindObjectOfType<ModuleLoadManager>();
        vwm = FindAnyObjectByType<VWModuleManager>();
        SS3V = FindAnyObjectByType<StartScreen3dView>();
       //Debug.LogError("Bundle Loader scripts start");
        StartCoroutine(GetSize());
        StartCoroutine(StartLoading());
    }
    /// <summary>
    ///  Returns an Instance of Bundle Loader, for use to Stop download or Clear Bundle
    /// </summary>
    /// <returns>Bundle Loader</returns>
    public BundleLoader startRef()
    {
        start();
      //  StartCoroutine(StartLoading());
        return this;
    }
    /// <summary>
    /// loads the given address
    /// </summary>
    /// <param name="loadAddress"></param>
    /// <returns>the addressable address to load</returns>
    /// 
    //Load bundle using address
    public BundleLoader load(string loadAddress)
    {
        bundleAddress = loadAddress;
        string[] sections = loadAddress.Split('/');
        gameObject.name = "Bundle Loader for " + sections[sections.Length - 1];
        Debug.Log($"Loading <b>{gameObject.name}</b>");

        return this;
    }
    public BundleLoader load(string address, UnityAction<GameObject> returnFunction)
    {
       
        onDownloadComplete = returnFunction;
        load(address);
      
        start();
        //   objectLoader = Addressables.LoadAssetAsync<GameObject>(address);

        //  Addressables.InstantiateAsync(modName).Completed += ModuleLoadTestComplete;
        //objectLoader.Completed += (obj) =>
        //        {
        //            Debug.Log("Using Address Based Loading Task:" + objectLoader.DebugName);
        //            //loaderTask = obj;
        //            //UpdateAvailableObjects(i, obj, returnFunction, spawn);
        //            result = obj.Result;
        //            onDownloadComplete(result);
        //        };

        return this;
    }


    public void CRCTesting()
    {
        //Debug.Log($"CRC : {objectLoader.Status} || {objectLoader.IsDone}");
        if(objectLoader.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log($"Crash : Async Operation Status {objectLoader.Status}");
            vwm.isBundleLoaderClosed = true;
            vwm.OnCancelDownload();            
            /*  if (vwm.isSubMod)
              {
                  //SS3V.OpenRnRForSubModule(vwm.lastModuleId);
                  vwm.SelectModule(vwm.lastModuleId);
              }
              else
              {
                  //SS3V.OpenRnRForModule(vwm.lastModuleId);
                  vwm.SelectModule(vwm.lastModuleId);
              }*/


        }
            //Debug.Log($"CRC : Download completed now loading");
    }


    //Download a specific bundle
    private IEnumerator StartLoading()
        {
        // Obtain AsyncOperationHandle
        //yield return new WaitForSeconds(1f);
        objectLoader = Addressables.LoadAssetAsync<GameObject>(bundleAddress);
        
        //  yield return objectLoader;
        ///  objectLoader = Addressables.InstantiateAsync(bundleAddress, transform,false,true);
        //  Addressables.InstantiateAsync(modName).Completed += ModuleLoadTestComplete;        

        // Pause Until successful
        while (!objectLoader.IsDone)
        {
           // Debug.Log($"<color=orange> StartLoading Status :  {objectLoader.Status} </color>");
            UpdateDownloadStat(objectLoader.GetDownloadStatus());            
            //var stat = objectLoader.GetDownloadStatus();
            //onProgressPercent?.Invoke(stat.Percent);
            //downloadedBytes = stat.DownloadedBytes;
            yield return null;
        }
        Debug.Log($"<color=orange> StartLoading Status :  {objectLoader.Status} || Task : {objectLoader.Task} || IsDone : {objectLoader.IsDone} </color>");
        objectLoader.Completed += ObjectLoader_Completed;
        #region Fallback
        // FallBack************************************************************
        //while (objectLoader.Status != AsyncOperationStatus.Succeeded)
        //    {
        //    onProgressPercent?.Invoke(objectLoader.PercentComplete);
        //    if (objectLoader.Status == AsyncOperationStatus.Failed)
        //    {
        //        if (onErrorForAddress != null)
        //            onErrorForAddress.Invoke(bundleAddress);
        //        else if(onError!=null)
        //        onError?.Invoke();

        //    }
        //        yield return null;
        //    }

        //Debug.Log("<color=blue>Loaded:</color>" + objectLoader.Result.name);
        Mixpanel.Track("Module_Loaded:","module_name",objectLoader.Result.name);
        //       result = objectLoader.Result;
        //    Complete(bundleAddress, result);
        #endregion
      
        // Release The Asset, because we are done with it
        //   Addressables.Release(objectLoader);
    }


    private void UpdateDownloadStat(DownloadStatus stat)
    {
        progressPercent = stat.Percent;
        downloadedBytes = stat.DownloadedBytes;
     //   onProgressPercent?.Invoke(stat.Percent);
    }
    private void ObjectLoader_Completed(AsyncOperationHandle<GameObject> obj)
    {
        Debug.Log($"<color=black><b>Object Loader Completed !!!</b></color>");
        result = obj.Result;
        Complete(bundleAddress, result);
       
    }

    public IEnumerator GetSize()
    {

       sizeCalculator = Addressables.GetDownloadSizeAsync(bundleAddress);        
        yield return sizeCalculator;
        //if (sizeCalculator.Result > 0)
        //{
      //  Debug.LogError("Size Calculator...." + sizeCalculator.Result);

        SizeCalculator_Completed(sizeCalculator);
        
      //  Debug.Log("<color=green>[BUNDLE LOADER]</color>Size of " + bundleAddress + " is " + sizeCalculator.Result / 1000000f + " mb");
    }
   
    private void SizeCalculator_Completed(AsyncOperationHandle<long> obj)
    {
        sizeCalculator.Completed -= SizeCalculator_Completed;
        if (obj.Result > 0)
        {
            totalSize = obj.Result;
            //Debug.Log("<color=green>[BUNDLE LOADER]</color>Download size for " + bundleAddress + " is "+obj.Result+" bytes " + (float)(obj.Result / 1000000) + " mb");
            Mixpanel.Track("Downloading_Module","module_name",moduleLoad.modNameToSend);
            
        }
        else
        {
            //Debug.Log("<color=green>[BUNDLE LOADER]</color>" + bundleAddress + " is already downloaded!");
            Mixpanel.Track(" Module_Exists:","module_name",moduleLoad.modNameToSend);
        }    
       Addressables.Release(sizeCalculator);
       // StopCoroutine(GetSize());

    }
    public long GetTotalDownloadSize() => totalSize;
    public long GetDownloadedBytes() => downloadedBytes;
    public float GetLoadPercent()
        {
        if (objectLoader.IsValid())
            return progressPercent;
        else
            return -1;
        }
    public void Stop()
    {
        Clear();
        StopCoroutine(StartLoading());
  
    }
   
   
    public BundleLoader onProgress(UnityAction<float> onProgressUpdate)
    {
        onProgressPercent = onProgressUpdate;
        return this;
    }

    void Complete(GameObject g)
    {
        Complete("", g);
    }
    void Complete(string add,GameObject g)
    {
        //Debug.Log("<color=blue>Loaded:</color>" + g.name);
        if (onDownloadAddressAndResult != null)
        {
            Debug.Log($"onDownloadAddressAndResult : Excute {g.name}");
            onDownloadAddressAndResult?.Invoke(add, g);
        }
        else
        {
            Debug.Log($"onDownloadComplete : Excute {g.name}");
            onDownloadComplete?.Invoke(g);
        }
           
     
    }
    
    public BundleLoader onCompleteCall(UnityAction<GameObject> returnFunction)
        {
            onDownloadComplete = returnFunction;
            return this;
        }
    public BundleLoader onCompleteCall(UnityAction<string,GameObject> returnAddressAndGameObject)
    {
        onDownloadAddressAndResult = returnAddressAndGameObject;
        return this;
    }
    public BundleLoader onErrorCall(UnityAction errorFunction)
        {
            onError = errorFunction;
            //   Addressables.Release(objectLoader);
            return this;
        }
    public BundleLoader onErrorCall(UnityAction<string> errorFunctionWithAddress)
    {
        onErrorForAddress = errorFunctionWithAddress;
        //   Addressables.Release(objectLoader);
        return this;
    }
    public void Clear()
    {
        Debug.Log("Cleared:" + objectLoader);
        onDownloadComplete = null;
        onDownloadAddressAndResult = null;
        onProgressPercent = null;
        onErrorForAddress = null;
        onError = null;
        End();
    }
  public  void End()
    {
        Debug.Log("Cleared_End:" + objectLoader);
        Addressables.ReleaseInstance(objectLoader);
        if (objectLoader.IsValid())
            Addressables.Release(objectLoader);

        //StartCoroutine(ImmediateUnloadModule());
        Destroy(gameObject);
    }
  
    IEnumerator ImmediateUnloadModule()
    {
        Debug.Log("ImmediateUnloadModule:" + objectLoader);
        yield return new WaitForSeconds(5f);
        DestroyImmediate(gameObject);
        yield return new WaitForSeconds(1f);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
