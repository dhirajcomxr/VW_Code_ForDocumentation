using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ModuleHarnessManager : MonoBehaviour
{
    [SerializeField] GameObject[] mainSceneObjs;
    public List<NamedHarness> harnesses;
 //   [SerializeField] UIScreenManager screenManager;
   public RemoteSceneLoader sceneLoader;
   public NamedHarness selectedHarness;
    public StartScreen3dView startScreen3D;
    public string moduleHarnessName;

    public CentralHarnessLinker centralHarnessLinker;
    [System.Serializable]
    public class NamedHarness
    {
        public string name;
        public string harnessAddress;
    }
  public void SelectHarness(string name)
    {
       // Debug.LogError("Selected harness Name"+name);
        selectedHarness = null; 
        for (int i = 0; i < harnesses.Count; i++)
        {
            Debug.LogError("Harness  "+harnesses[i].name +" Name --"+name);
            if (harnesses[i].name.ToLower().Contains(name.ToLower()))
                selectedHarness = harnesses[i];
        }
        if (selectedHarness != null)
        {
            Debug.Log("Selected Harness: " + selectedHarness.name);
        }
           
        
    }
    public bool HasHarnessFor(string name)
    {
        bool result = false;
        for (int i = 0; i < harnesses.Count; i++)
        {
            if (harnesses[i].name.ToLower().Contains(name.ToLower()))
                result = true;
        }
        return result;
    }

    public void LoadHarness()
    {    
        if (selectedHarness != null)
        {
            Debug.Log("[HARNESS] Loading Harness for " + selectedHarness.name);
            //SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            if (sceneLoader)
            {
                sceneLoader.End();
            }
            foreach (var obj in mainSceneObjs)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
            //    ComponentCache();
            
            Mixpanel.Track("Downloading_Harness_Module", "module_name", GetStartScreen3DView().GetCurrentCarName());
            UnloadPrevious();
            if (SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            }
            sceneLoader = RemoteSceneLoader.Get(selectedHarness.harnessAddress).Load(selectedHarness.harnessAddress, SceneManager_sceneLoaded);


            //SceneManager.LoadScene(selectedHarness.harnessAddress, LoadSceneMode.Additive);
            //bool activated = SceneManager.SetActiveScene(SceneManager.GetSceneByName(selectedHarness.harnessName));
            //Debug.Log("<color=red>[HARNESS]</color>"+activated);

            //    Invoke("Activate", 1);
            //AppLogger.LogEventDesc(AppLogger.EventType.harness, "Opened Harness for " + selectedHarness.name);
            AppLogger.LogEventDesc(AppLogger.EventType.harness, $"Opened {selectedHarness.name} - {moduleHarnessName}");
            //AppLogger.LogEventDesc("Wiring Harness", $"Opened {selectedHarness.name} - {moduleHarnessName}");            
            Mixpanel.Track("Harness_Download_Complete", "module_name", GetStartScreen3DView().GetCurrentCarName());
        }
    }

     private void UnloadPrevious()
    {
        RemoteSceneLoader oldLoader = FindObjectOfType<RemoteSceneLoader>();
        if (oldLoader)
        {
            oldLoader.End();
            Destroy(oldLoader.gameObject);
        }
    }

    StartScreen3dView GetStartScreen3DView()
    {
        if (startScreen3D == null)
            startScreen3D = FindObjectOfType<StartScreen3dView>();
        return startScreen3D;
    }
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("[HARNESS] Loading ");
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        string cfdname = centralHarnessLinker.selectedFileName;
        if (cfdname.Contains("Slavia"))
        {
            cfdname = cfdname.Replace("Slavia", "");
        }
        ///Debug.Log("CFD NAME..." + cfdname);
        Mixpanel.Track("CFD_Loaded" , "cfd_type", arg0.name + " " + cfdname);
      //  Invoke("Activate", 2f);
        //if (SceneManager.GetActiveScene().buildIndex > 0)
        //{
        //    SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));
        //  //  SceneManager.SetActiveScene(SceneManager.GetSceneByName(arg0.name));
        //    Debug.Log("<color=red>[HARNESS]</color>" + SceneManager.GetActiveScene().name);
        //}
    }

    private void Activate()
    {
        Debug.Log("Loading ");
   //     ComponentCache();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(selectedHarness.harnessAddress));
        Debug.Log("<color=red>[ACTIVE-HARNESS]</color>" + SceneManager.GetActiveScene().name);
    }
   [SerializeField] cakeslice.OutlineEffect cacheOf;
    void ComponentCache()
    {
        GameObject g = new GameObject("TEMP BUFFER OBJECT");
        var outlineEff = FindObjectOfType<cakeslice.OutlineEffect>();
        if(outlineEff!=null)
        {
            Debug.Log("Json: " + JsonUtility.ToJson(outlineEff));
            cacheOf = RuntimeUtils.CopyComponent<cakeslice.OutlineEffect>(outlineEff, g);
          
            Destroy(outlineEff);
        }
        Destroy(g);

    }
    void ResetComponentCache()
    {
        if (cacheOf != null)
            RuntimeUtils.CopyComponent<cakeslice.OutlineEffect>(cacheOf, Camera.main.gameObject);
    }
    public void OnBack()
    {
        Debug.LogError("BACK!");
        Scene scene = new Scene();
        if (SceneManager.sceneCount > 1)
        {
            scene = SceneManager.GetSceneAt(1);
        }
        if (scene.name!=null)
        {
            if (selectedHarness.harnessAddress.Contains(scene.name))
                SceneManager.UnloadSceneAsync(scene.name);
        }
       
        UnloadPrevious();
        //    SceneManager.UnloadSceneAsync(selectedHarness.harnessAddress);
        //if(SceneManager.GetActiveScene()!=SceneManager.GetSceneByBuildIndex(0))
        //    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
        //AppLogger.LogEventDesc(AppLogger.EventType.harness, "Exited Harness for " + selectedHarness.name);
        AppLogger.LogEventDesc(AppLogger.EventType.harness, $"Exited {selectedHarness.name} - {moduleHarnessName}");
        //AppLogger.LogEventDesc("Wiring Harness", $"Exited {selectedHarness.name} - {moduleHarnessName}");
        foreach (var item in mainSceneObjs)
        {
            if (item != null)
            {
                item.SetActive(true);
            }
        }
        ResetComponentCache();
    }
}
