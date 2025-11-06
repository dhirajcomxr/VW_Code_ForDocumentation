using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.ResourceManagement;
using System;
using Object = UnityEngine.Object;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Events;
using System.Text;
using mixpanel;
//using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
//[ExecuteInEditMode]
public class AssetLoader : MonoBehaviour {
    [SerializeField] StorageManagement storageManagement;
    [SerializeField] VWModuleManager vwm;
    [SerializeField]
    bool useMainCatalog = false, loadRemoteCatConfig = false, editorTest = false,
        useAndroid = false;
    [SerializeField] string resp;
    [SerializeField] bool downloadNow = false, loadNow = false, loadMod = false, loadingRemote = false, isDwnlding = false;
    [SerializeField] MainCatalog mainCat;
    float dwnldStatus = 0;
    static string filePath = "";// Application.persistentDataPath + "/" +
                                //   "id" + "/";
    public bool multiProjectLoading = false;
    public string remoteCatalogUrl = "",
        remoteModName,
        winCatLink = "", andCatLinkDev = "", andCatLinkStg = "", andCatLinkPro = "",
        testUrl = "";

    public string andCatLink = "";

    public string[] catalogLinks;
    public Test curCatalog;
    public StringList windowsCatalogs, androidCatalogs;
    //public string[] moduleAddresses;
    public List<NameAddress> nameAddresses;
    public int modId = -1, catId = -1;
    public List<string> nameList, sceneAddresses;
    public List<Sprite> sl, getSpriteNames;
    public Dictionary<string, Sprite> spriteSet;
    public List<string> imageNames, ipImageNames;
    AsyncOperationHandle<GameObject> al, loaderTask;
    IEnumerator spriteDownloadStat;
    AsyncOperationHandle downloadDependencies;
    public delegate void PostObjectLoad(GameObject g);
    public delegate void OnLoadComplete();
    public event OnLoadComplete finishedLoading;
    public delegate void PostObjectLoadId(int id, GameObject g);
    [SerializeField] AssetReference[] remoteObjects;
    [SerializeField] GameObject[] availableRemoteObjects;
    int[] missingIds;
    public delegate void RecieveText(string main);
    public RecieveText onMainCatalogRecieved, onDebug;
    [SerializeField] public BundleLoader bundleLoader;
    bool useNewLoader = true;
    public bool bundleLoaded;

    [System.Serializable]
    public class MainCatalog {
        public string[] catLinks;
        public List<NameAddress> nameAdd;
        public MainCatalog(string[] links, List<NameAddress> addresses) {
            this.catLinks = links;
            this.nameAdd = addresses;
        }
    }

    bool loadAllCatalogs = false;
    [SerializeField] int loadedCatalog = -1, curLoadingCat = -1;
    #region Delete Later 
    [SerializeField] int loadId = -1;
    [SerializeField] float completePerc = 0;
    [SerializeField] bool LoadIt = false;
    [SerializeField] GameObject egG;
    [SerializeField] int curr = 0;
    [SerializeField] Text dbgText;
    [SerializeField] Image fillImage;
    float timeDelay = 0.25f;
    float delayStat = 0;
    public delegate float LoadPerc();
    public delegate void DwnldSize(int index, long l);
    #endregion
    [System.Serializable]
    public class NameAddress {
        public string name;
        public string address;
        public NameAddress(string newName, string newAddress) {
            name = newName;
            address = newAddress;
        }
    }
    [System.Serializable]
    public class Test {
        public List<string> list;
    }
    private void Start() {
        if (FindObjectOfType<AppApiManager>().serverType == AppApiManager.ServerType.Staging) {
            andCatLink = andCatLinkStg;
        }else if (FindObjectOfType<AppApiManager>().serverType == AppApiManager.ServerType.Production)
        {
           Debug.Log("Production     00");
            andCatLink = andCatLinkPro;
        }
        else {
            andCatLink = andCatLinkDev;
        }
    }
    public void InitConfigs() {
        Debug.Log("InitConfigs......");
       loadedCatalog = -1;
        if (FindObjectOfType<AppApiManager>().serverType == AppApiManager.ServerType.Staging)
        {
            andCatLink = andCatLinkStg;
        }else if (FindObjectOfType<AppApiManager>().serverType == AppApiManager.ServerType.Production)
        {
            andCatLink = andCatLinkPro;
        }
        else
        {
            andCatLink = andCatLinkDev;
        }
        if (multiProjectLoading)
            if (useMainCatalog)
                DownloadMain();
            else
                InitMultiProject();
        //...
    }
    void InitMultiProject() {

        DownloadAllCatalogFiles();
        if (availableRemoteObjects.Length < nameAddresses.Count) {

            availableRemoteObjects = new GameObject[nameAddresses.Count];
        }
        ResourceManager.ExceptionHandler = CustomExceptionHandler;

    }
    public string GenerateFile() {
        string result = "";
        //   MainCatalog cat = new MainCatalog(catalogLinks, nameAddresses);
        Debug.LogError("  Catalog links------>> ");
        curCatalog.list = new List<string>(catalogLinks);
        result = JsonUtility.ToJson(curCatalog);
        //  result= JsonUtility.ToJson(cat);
        return result;
    }
    void DownloadMain() {
        // A correct website page.
        //     StartCoroutine(GetRequest("https://www.example.com"));
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            if (editorTest) {

                if (useAndroid) {
                    if (loadRemoteCatConfig)
                        StartCoroutine(GetRequest(andCatLink));
                    else {
                        catalogLinks = androidCatalogs.list.ToArray();
                        Debug.Log("Loading Local");
                        DisplayDebug("Loading Local..");
                        //  nameAddresses = mainCat.nameAdd;
                        InitMultiProject();
                    }
                }
                else
                    StartCoroutine(GetRequest(winCatLink));
            }
            else
                DownloadPlatformBased();
        }
        else {
            DownloadPlatformBased();
        }
        onMainCatalogRecieved += OnMainCatalogDownload;
    }
    void DownloadPlatformBased() {
        Debug.Log("Downloading Main Catalog for " + Application.platform);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
            if (winCatLink.Length > 0)
                StartCoroutine(GetRequest(winCatLink));
        }
        else {
            if (andCatLink.Length > 0)
                StartCoroutine(GetRequest(andCatLink));
        }
        // A non-existing page.
        //    StartCoroutine(GetRequest("https://error.html"));
    }
    void OnMainCatalogDownload(string data) {
        resp = data;
        Debug.Log("Received Data.." + data);
        onMainCatalogRecieved -= OnMainCatalogDownload;
        curCatalog = JsonUtility.FromJson<Test>(data);

        if (curCatalog != null) {
            catalogLinks = curCatalog.list.ToArray();
            catId = 0;
            //  nameAddresses = mainCat.nameAdd;
            InitMultiProject();
        }
    }

    //Get a json file of a specific car
    IEnumerator GetRequest(string uri) {
        Debug.Log("Asset loader    URI   "+uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                    DisplayDebug("<color=red>Internet Connection Error!</color>");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    onMainCatalogRecieved?.Invoke(webRequest.downloadHandler.text);
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
    //Gets called for every error scenario encountered during an operation.
    //A common use case for this is having InvalidKeyExceptions fail silently when a location is missing for a given key.
    void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception) {
        if (exception.GetType() != typeof(InvalidKeyException))
            Addressables.LogException(handle, exception);
        Debug.Log("<color=red> Type:" + exception.GetType() + " Handle:" + handle.DebugName + " message:" + exception.Message + "</color>");
        if (loaderTask.IsValid())
            Debug.Log("loadertask is " + loaderTask.IsValid() + " loader: " + loaderTask.Status);

        if (loaderTask.IsDone) {
            Debug.Log("Loading Done!");

            finishedLoading?.Invoke();
        }
    }

    public void UpdateNameAddresses() {
        //   string[] mods = moduleAddresses;
        string[] mods = new string[20];
        nameAddresses = new List<NameAddress>();
        for (int i = 0; i < mods.Length; i++) {
            if (mods[i].Length > 0) {
                string[] split = mods[i].Split('/');
                string name = split[split.Length - 1]
                    .Replace(".prefab", "").Replace("RNR", "").Replace("EXP", "")
                    .Replace('-', ' ').Replace('_', ' ');
                Debug.Log("Name:" + name);
                nameAddresses.Add(new NameAddress(name, mods[i]));
            }
        }
        Debug.Log("Updated to " + nameAddresses.Count + " addresses");
    }
    public List<string> GetModuleNameMatches(List<string> inputNames) {
        List<string> missingMods = new List<string>();
        for (int i = 0; i < inputNames.Count; i++) {
            bool found = false;
            for (int j = 0; j < nameAddresses.Count; j++) {
                if (nameAddresses[j].name.ToLower().Contains(inputNames[i].ToLower())) {
                    found = true;
                    Debug.Log("Found " + inputNames[i]);
                }
            }
            if (!found)
                missingMods.Add(inputNames[i]);
        }
        return missingMods;
    }
    // Start is called before the first frame update
    public void DT_IMGS() {
        LoadSprites("dt_images");
    }
    void DownloadTestImages() {

        DownloadImages(ipImageNames, "");
    }
    public List<string> GetSceneAddresses() => sceneAddresses;
    public void LoadAllSceneAddresses() {
        sceneAddresses = new List<string>();
        for (int i = 0; i < nameList.Count; i++) {
            if (nameList[i].ToLower().Contains("scene"))
                sceneAddresses.Add(nameList[i]);
        }
        Debug.Log("added " + sceneAddresses.Count + " scene Addresses");
    }


    //Downlaod catlog file from the json fetched from GetRequest function
    public void DownloadAllCatalogFiles() {
        loadAllCatalogs = true;
        nameList = new List<string>();
        if (loadedCatalog < catalogLinks.Length) {
            if (loadedCatalog < 0)
                catId = 0;
            LoadBundleLink(catalogLinks[catId]);
        }

        Debug.Log("<color=blue>loading bundle </color>");
    }
    bool loadSingle = false;
    public void LoadSingleCatalog() {
        if (loadAllCatalogs) {
            loadSingle = true;
            loadAllCatalogs = false;
        }
        Debug.Log("Loading From Remote URL..");
        LoadBundleLink(remoteCatalogUrl);
    }
    void DownloadCatalogFile() {
        if (loadAllCatalogs) {
            if (loadedCatalog < catalogLinks.Length)
            {
             
                LoadBundleLink(catalogLinks[catId]);
            }
               
            else
                Debug.Log("<color=blue>loaded bundles </color>" + catId);
        }
        else
            LoadBundleLink(catalogLinks[catId]);
        //Debug.Log("loading bundle " + catId);
    }
    public void LoadBundleLink(string url) {
    
        if (loadAllCatalogs)
        {
            //Debug.Log("  ---  Download URL ---   " + url);
            Addressables.LoadContentCatalogAsync(url, true).Completed += OnBundleLinkLoadedFromList;
        }   
        else
        {
            //Debug.Log("  ---  Download URL ---   " + url);
            Addressables.LoadContentCatalogAsync(url, true).Completed += OnBundleLinkLoaded;
        }
            
    }

    private void OnBundleLinkLoadedFromList(AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj) {
        if (loadAllCatalogs)
            if (obj.IsDone) {
                loadedCatalog = catId;
                catId++;

                UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator irl = obj.Result;
                IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> irlocations;

                foreach (var item in irl.Keys) {
                    nameList.Add(item.ToString());
                    //  Debug.Log("Found " + item.ToString());
                }

                if (loadedCatalog < (catalogLinks.Length - 1))
                    DownloadCatalogFile();
                else {
                    CheckModuleAddresses();
                    DisplayDebug("<color=blue>Updated main module config</color>");
                    UIScreenManager screenManager = FindObjectOfType<UIScreenManager>();
                    screenManager.loadingAnimated.SetActive(false);
                    Debug.Log("ALL loaded");
                }
            }
    }
    private void OnBundleLinkLoaded(AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj) {
        Debug.Log("<color=blue>bundle loaded </color>" + obj.Result.ToString());
        UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator irl = obj.Result;
        IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> irlocations;
        if (loadSingle) {
            loadAllCatalogs = true;
            loadSingle = false;
        }
        if (irl.Locate(remoteModName, typeof(object), out irlocations))
            Debug.Log("Loaded");
        nameList = new List<string>();
        nameList = UpdateNameList(nameList);
        //if(irlocations.Count>0)
        //{
        //    foreach (var item in irlocations)
        //    {
        //        Debug.Log("PK "+item.PrimaryKey);
        //    }
        //}

        List<string> UpdateNameList(List<string> list) {
            foreach (var item in irl.Keys) {
                list.Add(item.ToString());
                //  Debug.Log("Found " + item.ToString());
            }
            return list;
        }
        //  throw new System.NotImplementedException();
    }

    AsyncOperationHandle<GameObject> remoteLoadHandle;
    public void LoadRemoteModule(string modName, bool spawn = false) {
        //foreach (var item in Addressables.ResourceLocators)
        //{         
        //    foreach (var itemK in item.Keys)
        //    {              
        //        Debug.Log("<color=red>KEY: </color>" + itemK.ToString());
        //    }
        //}

        remoteLoadHandle = Addressables.LoadAssetAsync<GameObject>(modName);
        //  Addressables.InstantiateAsync(modName).Completed += ModuleLoadTestComplete;

        loadingRemote = true;
        remoteLoadHandle.Completed += ModuleLoadTestComplete;

    }
    private void ModuleLoadTestComplete(AsyncOperationHandle<GameObject> obj) {
        remoteLoadHandle.Completed -= ModuleLoadTestComplete;
        loadingRemote = false;
        GameObject g = Instantiate(obj.Result);
        Debug.Log(g.name, g);
        //    throw new System.NotImplementedException();
    }

    //Check the json link address which is fetched from the server
    public void CheckModuleAddresses() {

        List<int> missingIdList = new List<int>();
        if (nameList.Count > 0) {
            for (int i = 0; i < nameAddresses.Count; i++) {
                bool found = false;
                if (nameAddresses[i].address.Length > 0) {
                    for (int j = 0; j < nameList.Count; j++) {
                        if (nameList[j] == nameAddresses[i].address) {
                            if (!found)
                                found = true;
                            else
                                Debug.Log("<color=yellow>Duplicate Found:</color>" + nameAddresses[i].name);
                            //  Debug.Log("<color=green>Found </color>" + nameAddresses[i].name);
                        }
                    }
                    if (!found) {
                        missingIdList.Add(i);
                        //  missingList.Add(nameAddresses[i].name);                     
                    }
                }
            }
            if (missingIdList.Count > 0) {
                foreach (int item in missingIdList) {
                    //Debug.LogError("<color=red> Did not find </color> " + nameAddresses[item].name);
                }
                missingIds = missingIdList.ToArray();
            }
            else
            {
                //Debug.Log("<color=green>Found All Module Addresses</color>");
            }
        }
        else
        {
            //Debug.Log("Catalog List Not Initialised!");
        }
    }

    public void DownloadImages(List<string> imgN, string url, string ext = ".jpg", UnityAction<List<Sprite>> returnFunction = null) {
     
        filePath = Application.persistentDataPath + "/" + "id" + "/";
    
        //Debug.Log("Starting download to " + filePath);
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        // The output of the image    
        imageNames = imgN;
        spriteSet = new Dictionary<string, Sprite>();
        List<Sprite> spriteList = new List<Sprite>();
        // The source image
        StartCoroutine(DownloadAvailableImages(imageNames));

        //Debug.Log("Available Images: " + imageNames.Count);
        // StartCoroutine(CheckIfImagesExist(imageNames));

        IEnumerator CheckIfImagesExist(List<string> imgNames) {
            //Debug.Log("<color=blue>Image URL</color>"+url);
            int totalImages = imgNames.Count;
            int totalLoad = 0;
            List<string> existingNames = new List<string>();
            while (imgNames.Count > 0)
            //   for (int i = 0; i < imgNames.Count; i++)
            {
                int curImgCount = imgNames.Count;
                if (curImgCount > 0)
                    totalLoad = (1 - curImgCount / totalImages);
                Texture2D texture;
                Sprite sprite;
                string uniqueHash = CreateMD5(imgNames[0]);
                if (File.Exists(filePath + uniqueHash)) {
                    byte[] fileData;
                    //Debug.Log("loading " + imgNames[0] + " from storage"+ filePath + uniqueHash);
                    fileData = File.ReadAllBytes(filePath + uniqueHash);
                    texture = new Texture2D(128,128,TextureFormat.RGB24,true);
                    //ImageConversion.LoadImage(texture, fileData);
                    texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                    //Debug.Log("Texture"+texture);
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0,0));
                    sprite.name = imgNames[0];
                    spriteList.Add(sprite);
                    //Debug.Log("Sprite "+sprite.rect);

                    sl.Add(sprite);
                    if (!spriteSet.ContainsKey(imgNames[0]))
                        spriteSet.Add(imgNames[0], sprite);
                    imgNames.RemoveAt(0);
                }
                else { 
                    string curUrl = url + imgNames[0] + ext;
                    //Debug.Log("URL:" + curUrl);
                    var check = UnityWebRequest.Head(curUrl);

                    yield return check.SendWebRequest();
                    /*   if (check.error == null)
                       {
                           Debug.Log("Progress  "+check.downloadProgress);
                           Debug.Log("Processed Successfully");
                       }
                       else
                       {
                           Debug.Log(check.error);
                       }*/
                   
                    if (check.result != UnityWebRequest.Result.Success) {
                       
                        //Debug.Log("Error finding " + imgNames[0] + " from " + curUrl);
                        //Debug.Log("<color=red>Error:</color>" + check.error);
                        imgNames.RemoveAt(0);
                    }
                    else {
                        //Debug.Log("<color=orange>[" + check.result.ToString() + "]</color>" + "Found Image at Url:" + curUrl);
                        if (check.isDone) {
                            //Debug.Log("Progress  " + check.downloadProgress);
                            existingNames.Add(imgNames[0]);
                            imgNames.RemoveAt(0);
                        }
                    }
                }
            }
            if (imageNames.Count == 0) {
             
               // StartCoroutine(DownloadAvailableImages(existingNames));
            }
        }

        IEnumerator DownloadAvailableImages(List<string> imgNames) {
            Debug.Log("Starting downloading " + imgNames.Count + "...");
            while (imgNames.Count > 0) {
                string uniqueHash = CreateMD5(imgNames[0]);
                string curUrl = url + imgNames[0] + ext;
                Texture2D texture = new Texture2D(2, 2);
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(curUrl);
                Debug.Log("DTs Requast " + www);

                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                    Debug.Log("Error in Downloading: " + curUrl);
                    Debug.Log(www.result);
                    imgNames.RemoveAt(0);
                }

                else
       if (www.isDone) {
                    Debug.Log("<color=blue>[" + www.result.ToString() + "]</color>" + www.downloadedBytes / 1000 + " kb: " + "Url:" + curUrl);
                    //    File.WriteAllBytes(filePath + uniqueHash, www.downloadHandler.data);
                    texture = (Texture2D)((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (ext.ToLower().Contains("j"))
                        File.WriteAllBytes(filePath + uniqueHash, texture.EncodeToJPG());
                    else if (ext.ToLower().Contains("png"))
                        File.WriteAllBytes(filePath + uniqueHash, texture.EncodeToJPG());
                    //texture.GenerateMipMaps();
                    //texture.filterMode = FilterMode.Trilinear;
                    //    texture.LoadImage(www.downloadHandler.data);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                    Debug.Log("Texture size: " + texture.width + "x" + texture.height);

                    sprite.name = imgNames[0];
                    //       File.WriteAllBytes(filePath + uniqueHash+ext, sprite.texture.GetRawTextureData());
                    spriteList.Add(sprite);
                    sl.Add(sprite);
                    if (!spriteSet.ContainsKey(imgNames[0]))
                        spriteSet.Add(imgNames[0], sprite);
                    imgNames.RemoveAt(0);
                    Debug.Log("<color=red>[" + uniqueHash + "]</color>");
                }

                //else
                //    Debug.LogError("No Texture !");
            }
            if (imgNames.Count == 0)
                returnFunction?.Invoke(spriteList);
        }

        //   imgNames.RemoveAt(0);
        //      }
        IEnumerator DownloadImgToSprites(List<string> imgNames) {
            int totalLoad = 0;
            int totalImages = imgNames.Count;

            while (imgNames.Count > 0)
            //   for (int i = 0; i < imgNames.Count; i++)
            {
                int curImgCount = imgNames.Count;
                if (curImgCount > 0)
                    totalLoad = (1 - curImgCount / totalImages);
                Texture2D texture;
                Sprite sprite;
                string uniqueHash = CreateMD5(imgNames[0]);
                if (File.Exists(filePath + uniqueHash)) {
                    byte[] fileData;

                    fileData = File.ReadAllBytes(filePath + uniqueHash);
                    texture = new Texture2D(2, 2);
                    //ImageConversion.LoadImage(texture, fileData);
                    texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                    sprite.name = imgNames[0];
                    spriteList.Add(sprite);
                    sl.Add(sprite);
                    if (!spriteSet.ContainsKey(imgNames[0]))
                        spriteSet.Add(imgNames[0], sprite);
                }
                else {
                    string curUrl = url + imgNames[0] + ext;
                    Debug.Log("URL:" + curUrl);
                    var check = UnityWebRequest.Head(curUrl);

                    yield return check.SendWebRequest();

                    if (check.result != UnityWebRequest.Result.Success) {
                        Debug.Log("<color=red>Error:</color>" + check.error);
                    }
                    else {
                        Debug.Log("<color=orange>[" + check.result.ToString() + "]</color>" + "Found Image at Url:" + curUrl);
                        if (check.isDone) {
                            UnityWebRequest www = new UnityWebRequest(curUrl);

                            yield return www.SendWebRequest();
                            if (!www.isDone) {
                                if (www.error.Length > 0)
                                    Debug.LogError(www.error);
                            }

                            if (www.downloadHandler != null)
                                if (www.downloadHandler.isDone) {
                                    Debug.Log("<color=blue>[" + www.result.ToString() + "]</color>" + www.downloadedBytes / 1000 + " kb: " + "Url:" + curUrl);
                                    File.WriteAllBytes(filePath + uniqueHash, www.downloadHandler.data);
                                    texture = new Texture2D(2, 2);
                                    texture.LoadImage(www.downloadHandler.data);
                                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                                    sprite.name = imgNames[0];
                                    //       File.WriteAllBytes(filePath + uniqueHash+ext, sprite.texture.GetRawTextureData());
                                    spriteList.Add(sprite);
                                    sl.Add(sprite);
                                    if (!spriteSet.ContainsKey(imgNames[0]))
                                        spriteSet.Add(imgNames[0], sprite);
                                }
                                else
                                    Debug.LogError("No Texture !");
                        }
                    }
                    imgNames.RemoveAt(0);


                }
            }
            finishedLoading?.Invoke();
            returnFunction?.Invoke(spriteList);
            Debug.Log("<color=green>Finished Downloading and Saved to:" + filePath + "</color>");
        }

    }

    //public IEnumerable<UnityWebRequestAsyncOperation> CheckUrl(string url)
    //{
    //    var check = UnityWebRequest.Head(url);
    //   yield return check.SendWebRequest();
    //    Debug.Log(check.result.ToString());
    //}
    public static string CreateMD5(string input) {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++) {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
    private void OnEnable() {
        //if (getSpriteNames.Count > 0)
        //{
        //    ipImageNames = new List<string>();
        //    for (int i = 0; i < getSpriteNames.Count; i++)
        //    {             
        //        ipImageNames.Add(getSpriteNames[i].texture.name);
        //    }
        //}

    }
    string lastStat;
    private void Update() {
        //        if (downloadNow)
        //        {
        //            downloadNow = false;
        //            Download("dt_images");
        //            isDwnlding = true;
        //        }
        //if (loadingRemote)
        //{
        //  string  curStat = loaderTask.Status.ToString();
        //    if (lastStat != curStat)
        //    {
        //        lastStat = curStat;
        //        Debug.Log("Load Status: " + lastStat);
        //    }
        //}
        if (loadNow) {
            loadNow = false;
            DownloadCatalogFile();
            //  LoadSprites("dt_images");
        }
        if (loadMod) {
            loadMod = false;
            LoadRemoteModule(nameAddresses[modId].address);
        }
        //        //if(isDwnlding)
        //        //dwnldStatus = downloadDependencies.PercentComplete;
    }
    LoadState loadState;
    private AsyncOperationHandle<IList<Sprite>> OnSpritesLoaded;

    public enum LoadState {
        Null, Loading, Loaded
    }
    public int GetTotalReferences() => availableRemoteObjects.Length;
    public string GetNameForModule(int modId) {
        string result = null;
        if (modId >= 0) {
            if (modId < nameAddresses.Count)
                result = nameAddresses[modId].name;
        }
        return result;
    }
    public int GetIdForModuleAddress(string address) {
        int result = -1;
        for (int i = 0; i < nameAddresses.Count; i++) {
            if (address == nameAddresses[i].address)
                return result = i;
        }
        return result;
    }

    public int[] GetIdsForModuleName(string moduleName) {
        List<int> result = new List<int>();

        for (int i = 0; i < nameAddresses.Count; i++) {

            if (nameAddresses[i].name.ToLower().Contains(moduleName.ToLower())) {
                bool isMissing = false;
                if (missingIds != null)
                    for (int j = 0; j < missingIds.Length; j++) {
                        if (i == missingIds[j])
                        {
                            Debug.LogError(nameAddresses[i].name);
                            isMissing = true;
                        }
                    }
                //           Debug.Log("<color=green>Got Module: </color>" + moduleName +" at "+i);
                if (!isMissing)
                    result.Add(i);

            }
        }
        return result.ToArray();
    }
    public string GetAddressForModule(int modId) {
        string result = null;
        if (modId >= 0) {
            if (modId < nameAddresses.Count)
                result = nameAddresses[modId].address;
        }
        return result;
    }
    public static async Task InitAssets<T>(string label, List<T> createdObjs, Transform parent)
            where T : Object {
        var locations = await Addressables.LoadResourceLocationsAsync(label).Task;

        foreach (var location in locations) {
            createdObjs.Add(await Addressables.InstantiateAsync(location, parent).Task as T);
        }
    }
    public static async Task InitAssets<T>(AssetReference[] asset, List<T> createdObjs, Transform parent)
           where T : Object {
        var locations = await Addressables.LoadResourceLocationsAsync(asset).Task;

        foreach (var location in locations) {
            createdObjs.Add(await Addressables.InstantiateAsync(location, parent).Task as T);
        }
    }
    public void GetModuleObject(int id, PostObjectLoadId returnFunction)
        => GetModuleObject(id, returnFunction, true);
    public bool IsModuleDownloaded(int id)
        => availableRemoteObjects[id] != null;
    public GameObject GetIfDownloaded(int id)
        => availableRemoteObjects[id];
    public void GetModuleObject(int i, PostObjectLoadId returnFunction, bool spawn) {

        loadState = LoadState.Loading;
        int localObjsL = availableRemoteObjects == null ? 0 : availableRemoteObjects.Length;
        if (localObjsL < 1)
            availableRemoteObjects = new GameObject[remoteObjects.Length];
        if (availableRemoteObjects[i] != null) {
            Debug.Log("Object Exists Already");
            //if (spawn)
            //    returnFunction(i,Instantiate(availableRemoteObjects[i]));
            //else
            returnFunction(i, availableRemoteObjects[i]);
        }
        else {
           // Debug.Log(" Start Download module -------");
            if (loaderTask.IsValid()) {
                if (loaderTask.IsDone)
                {
                    Debug.Log("<color=orange><b> Loader Task valid </color>");
                    SelectLoading(multiProjectLoading, i, returnFunction, spawn);
                }
                    
            }
            else
            {
                Debug.Log("<color=red> <b> Loader Task Invalid</b> </color>");
                SelectLoading(multiProjectLoading, i, returnFunction, spawn);
            }
               

        }

        void SelectLoading(bool pathOrRef, int k, PostObjectLoadId return_Function, bool inst = false) {
            
            if (pathOrRef) {
                if (useNewLoader)
                {
                    Debug.Log("<color=orange>Download Alt</color>");
                    DownloadAlt();
                    
                }

                else
                {
                    Debug.Log("<color=green>Download From Path</color>");
                    DownloadFromPath();
                    
                }
                  
            }
            else
            {
                Debug.Log("<color=red>Download From Asset Reference</color>");
                DownloadFromAssetReference();
                
            }
            
        }

        //Download a specific module using Path "Assets/Prefabs/Modules/"modulename".prefab"
        void DownloadFromPath() {
            loaderTask = Addressables.LoadAssetAsync<GameObject>(nameAddresses[i].address);
            //  Addressables.InstantiateAsync(modName).Completed += ModuleLoadTestComplete;
            loadingRemote = true;

            loaderTask.Completed += (obj) => {
                //Debug.Log("Using Address Based Loading Task:" + loaderTask.DebugName);
                //     loaderTask = obj;
                UpdateAvailableObjects(i, obj, returnFunction, spawn);
            };
        }

        GameObject bundlerLoaderResponse;
        //Download a specific module using Path "Assets/Prefabs/Modules/"modulename".prefab"
        void DownloadAlt() {            
            if (bundleLoader != null)
            {
                Debug.Log("<B>Crash : Download Alt</b>");
                bundleLoader.Stop();
                bundleLoader = null;
            }

            Debug.Log("Clear previous  Bundle");
           
            bundleLoader = BundleLoader.Get().load(nameAddresses[i].address)
                   .onCompleteCall((add, obj) =>
                   {
                       //Instantiate(obj);
                       if (obj == null)
                       {
                           Debug.Log($"<color=red> Obj Is Null </color>");
                       }
                       else
                       {
                           Debug.Log($"<color=red>Loaded:  <b>{obj.name}</b>  from : { add } </color>");
                           Mixpanel.Track("Download_Completed", "module_name", obj.name);
                           bundlerLoaderResponse = obj;
                           StartCoroutine(CallUpdateAvailableObjects());
                           bundleLoaded = true;
                           
                       }

                   })
                   .onProgress((float val) => { /*Debug.Log("On Progress Loading: " + val);*/ })
                   .onErrorCall(LoadError).startRef();
            
        }

        
        IEnumerator CallUpdateAvailableObjects()
        {
            yield return new WaitForSeconds(1f);
            UpdateAvailableObjects(i, bundlerLoaderResponse, returnFunction, spawn);
            Debug.Log($"<color=brown><b> Called Update Available Objects </b></color>");
            StopCoroutine(CallUpdateAvailableObjects());
        }

        void DownloadFromAssetReference() {
            AssetReference objectRef = remoteObjects[i];
            loaderTask = Addressables.LoadAssetAsync<GameObject>(objectRef);
            loaderTask.Completed //>>
                                 // Addressables.LoadAssetAsync<GameObject>(objectRef).Completed
            += (obj) => {
                Debug.Log("Loader Task:" + loaderTask.DebugName);
                //loaderTask = obj;
                UpdateAvailableObjects(i, obj, returnFunction, spawn);
            };
        }
    }

    void LoadPercent(float perc) {
        loadPerc = perc;
        //Debug.Log("Loading: " + perc);
    }
    public void StopBundleLoading() {
        if (bundleLoader)
        {
            bundleLoader.Stop();
            Debug.Log("<B>Crash : Stop Bundle Loading</b>");
        }
    }
    void LoadError(string errorForFile) {
        Debug.Log("Error! loading File");        
        if (bundleLoader)
        {
            Debug.Log("<B>Crash : Load Error</b>");
            bundleLoader.Stop();
        }
    }
    float loadPerc = -1;
    public float GetLoadingPercent() {
        if (useNewLoader) {
            long dwn = bundleLoader.GetDownloadedBytes();
            long total = 0;
            string totalSizeMB = "";
            if (dwn > 0) {
                if (total < 1) {
                    total = bundleLoader.GetTotalDownloadSize();
                    totalSizeMB = total / 1000000 + " mb";
                    //Debug.Log("Total  MB");
                }
                DisplayDebug("Downloaded: " + bundleLoader.GetDownloadedBytes() / 1000000 + " mb of " + totalSizeMB);
            }
            return bundleLoader.GetLoadPercent();

        }
        //  return loadPerc;
        else
            return (loaderTask.Task != null || loaderTask.IsValid()) ? loaderTask.PercentComplete : 0f;
    }
    private void UpdateAvailableObjects(int i, GameObject g, PostObjectLoadId returnFunction, bool spawn) {
        Debug.Log("<color=red>UpdateAvailableObjects With Gameobject</color>");
        if (availableRemoteObjects[i] != null)
            Debug.LogError("Object to Load exists!, Replacing..");
        if (g == null) {
            Debug.Log("<color=red>null object!</color>");
            finishedLoading?.Invoke();
        }
        //   if (spawn)
        if (availableRemoteObjects[i] == null)
            availableRemoteObjects[i] = Instantiate(g);
        else
            availableRemoteObjects[i] = g;

        loadState = LoadState.Loaded;
        returnFunction(i, availableRemoteObjects[i]);

        Debug.Log("<color=orange>UpdateAvailableObjects With Gameobject End</color>");
        //storageManagement.updatedownloaddetails(nameAddresses[vwm.lastModuleId].address, bundleLoader.totalSize);
    }
    private void UpdateAvailableObjects(int i, AsyncOperationHandle<GameObject> obj, PostObjectLoadId returnFunction, bool spawn) {
        Debug.Log("<color=red>UpdateAvailableObjects With Async</color>");
        if (availableRemoteObjects[i] != null)
            Debug.LogError("Object to Load exists!, Replacing..");
        if (obj.Result == null) {
            Debug.Log("<color=red>null object!</color>");
            finishedLoading?.Invoke();
        }
        //   if (spawn)
        if (availableRemoteObjects[i] == null)
            availableRemoteObjects[i] = Instantiate(obj.Result);
        else
            availableRemoteObjects[i] = obj.Result;

        loadState = LoadState.Loaded;
        returnFunction(i, availableRemoteObjects[i]);

      
    }

    public void Download(string label) {
        StartCoroutine(PreDownload(label));
        Debug.Log("Pre-Downloading: " + label);
    }
    IEnumerator PreDownload(string label) {
        ////Clear all cached AssetBundles
        //Addressables.ClearDependencyCacheAsync(label);

        //Check the download size
        AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(label);
        yield return getDownloadSize;

        //If the download size is greater than 0, download all the dependencies.
        if (getDownloadSize.Result > 0) {
            Debug.Log("Will download Additional " + getDownloadSize.Result + " of data!");
            Addressables.ClearDependencyCacheAsync(label);

            //  AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(label);
            downloadDependencies = Addressables.DownloadDependenciesAsync(label);
            yield return downloadDependencies;
        }
        if (downloadDependencies.IsDone)
            isDwnlding = false;

        //...
    }
    public void LoadSprites(string label) {
        sl = new List<Sprite>();
        spriteSet = new Dictionary<string, Sprite>();
        Addressables.LoadAssetsAsync<Sprite>(label, null).Completed += DidLoad;
    }

    void DidLoad(AsyncOperationHandle<IList<Sprite>> op) {
        if (op.PercentComplete == 1)
            finishedLoading.Invoke();
        foreach (var sprite in op.Result) {
            sl.Add(sprite);
            spriteSet.Add(sprite.name, sprite);
            Debug.Log(op.PercentComplete + " SPRITE NAME IS: " + sprite.name.ToString());
        }
    }

    public Sprite GetSprite(string spriteName) {
        if (spriteSet != null) {
            if (spriteSet.ContainsKey(spriteName))
                return spriteSet[spriteName];
            else {
                Debug.Log(spriteName + " not found!");
                return null;
            }
        }
        else {
            Debug.Log("Spriteset not initialised");
            return null;
        }
    }


    public string searchString = "";
    public List<string> matches;

    public void SearchInList() {
        string s = "";
        matches = new List<string>();
        for (int i = 0; i < nameList.Count; i++) {
            if (nameList[i].ToLower().Contains(searchString.ToLower())) {
                matches.Add(nameList[i]);
            }
        }
        Debug.Log(matches.Count);
    }


    void DisplayDebug(string msg) {
     //   Debug.Log(msg);
        onDebug?.Invoke(msg);
    }
    #region ImageLoading

    public void downloadImage(string url, string pathToSaveImage) {
        WWW www = new WWW(url);
        StartCoroutine(_downloadImage(www, pathToSaveImage));
    }

    private IEnumerator _downloadImage(WWW www, string savePath) {
        yield return www;

        //Check if we failed to send
        if (string.IsNullOrEmpty(www.error)) {
            UnityEngine.Debug.Log("Success");

            //Save Image
            saveImage(savePath, www.bytes);
        }
        else {
            UnityEngine.Debug.Log("Error: " + www.error);
        }
    }

    void saveImage(string path, byte[] imageBytes) {
        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path))) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        try {
            File.WriteAllBytes(path, imageBytes);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
        }
        catch (Exception e) {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    byte[] loadImage(string path) {
        byte[] dataByte = null;

        //Exit if Directory or File does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path))) {
            Debug.LogWarning("Directory does not exist");
            return null;
        }

        if (!File.Exists(path)) {
            Debug.Log("File does not exist");
            return null;
        }

        try {
            dataByte = File.ReadAllBytes(path);
            Debug.Log("Loaded Data from: " + path.Replace("/", "\\"));
        }
        catch (Exception e) {
            Debug.LogWarning("Failed To Load Data from: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }

        return dataByte;
    }
    #endregion
    // Update is called once per frame
    #region Delete Later
    //private void OnEnable()
    //{
    //    if (availableRemoteObjects.Length != remoteObjects.Length)
    //        availableRemoteObjects = new GameObject[remoteObjects.Length];
    //    else
    //        Debug.Log("Same Length");
    //    for (int i = 0; i < remoteObjects.Length; i++)
    //    {
    //        ;//    isDownloaded(i, GetSizeFor);
    //    }
    //    GetGo(GoTest);
    //}
    //void GetSizeFor(int index, long size)
    //{
    //    Debug.Log("Size for " + index + " is:" + size);
    //}
    //void GoTest()
    //{
    //    Debug.Log("Inside function");
    //}
    //public void GetGo(System.Action functionCall)
    //{
    //    Debug.Log(functionCall.Method.Name);
    //    functionCall.Invoke();
    //}

    //public async void GetGameobject(int i, PostObjectLoad loadedObjCall)
    //{
    //    if (availableRemoteObjects[i] != null)
    //        loadedObjCall(availableRemoteObjects[i]);
    //    else
    //    {
    //        List<GameObject> lgo = new List<GameObject>();
    //        await InitAssets(new AssetReference[] { remoteObjects[i] }, lgo, transform);
    //        loadedObjCall(lgo[0]);
    //    }
    //}
    ////void  isDownloaded(int remoteObjId,DwnldSize functionCall)
    //// {
    ////   loaderTask= Addressables.GetDownloadSizeAsync(remoteObjects[remoteObjId]);
    ////     functionCall(remoteObjId, loaderTask.Result);
    //// }
    //public void Next()
    //{
    //    curr++;
    //    if (curr >= remoteObjects.Length)
    //        curr = 0;
    //    LoadAsset(curr);

    //}
    //public void LoadAsset(int index)
    //{

    //    al = Addressables.LoadAssetAsync<GameObject>(remoteObjects[index]);
    //    loadState = LoadState.Loading;
    //    al.Completed += AssetLoader_Completed;
    //    //   Addressables.LoadAssetAsync<GameObject>(egs[index]).Completed += AssetLoader_Completed;
    //}
    //public void LoadAsset(int index, PostObjectLoad getGameObjectFunction)
    //{

    //    al = Addressables.LoadAssetAsync<GameObject>(remoteObjects[index]);
    //    loadState = LoadState.Loading;
    //    al.Completed += AssetLoader_Completed;
    //    //   Addressables.LoadAssetAsync<GameObject>(egs[index]).Completed += AssetLoader_Completed;
    //}


    //private GameObject AssetDownloadedAndLoaded(AsyncOperationHandle<GameObject> obj)
    //{
    //    return Instantiate(obj.Result);
    //}
    //private void AssetLoader_Completed(AsyncOperationHandle<GameObject> obj)
    //{
    //    loadState = LoadState.Loaded;
    //    dbgText.text = "Loaded:" + obj.Result.name;
    //    GameObject loadedObj = obj.Result;
    //    egG = Instantiate(loadedObj);
    //}
    //void _UpdateLoadingPercent()
    //{
    //    delayStat = timeDelay;
    //    dbgText.text = "Loading " + al.PercentComplete.ToString("F2") + "% of " + remoteObjects[curr].SubObjectName;
    //    fillImage.fillAmount = al.PercentComplete;
    //}
    //void Update()
    //{

    //}
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(AssetLoader))]
[CanEditMultipleObjects]
public class AssetLoaderEditor : Editor {
    public string mainCat;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        AssetLoader script = (AssetLoader)target;
        if (GUILayout.Button("Download Catalogs")) {
            script.catId = 0;
            script.DownloadAllCatalogFiles();
        }
        if (GUILayout.Button("Check Module Addresses")) {
            script.CheckModuleAddresses();
        }
        //if(GUILayout.Button("Update Name Addresses"))
        //{
        //    script.UpdateNameAddresses();
        //}
        if (GUILayout.Button("Test Remote Url Catalog")) {
            script.LoadSingleCatalog();
        }
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate File")) {
            mainCat = script.GenerateFile();
        }
        mainCat = GUILayout.TextArea(mainCat);

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Search in Name List")) {
            script.SearchInList();
        }
        //if (GUILayout.Button("Check link"))
        //    script.StartCoroutine(script.CheckUrl(script.testUrl));
    }
}
#endif
