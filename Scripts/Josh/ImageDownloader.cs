using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ImageDownloader : MonoBehaviour
{
    static bool ENABLE_GLOBAL_LOGS = false;
    bool enableLog = true, cached=false;
    private int progress;
    private string url, uniqueHash;
    private UnityAction<int> onDownloadProgressChange;
    private UnityAction onDownloadedAction;
    private UnityAction<Texture> onComplete;
    static string filePath = Application.persistentDataPath + "/" +
          "VW-ID" + "/";
    private UnityAction onStartAction;
    private UnityAction OnLoadedAction;
    private UnityAction<string> onErrorAction;
    private UnityAction onEndAction;

    static Dictionary<string, ImageDownloader> processing; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Actions
    public ImageDownloader withStartAction(UnityAction action)
    {
        this.onStartAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On start action set : " + action);

        return this;
    }

    public ImageDownloader withDownloadedAction(UnityAction action)
    {
        this.onDownloadedAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On downloaded action set : " + action);

        return this;
    }

    public ImageDownloader withDownloadProgressChangedAction(UnityAction<int> action)
    {
        this.onDownloadProgressChange = action;

        if (enableLog)
            Debug.Log("[Davinci] On download progress changed action set : " + action);

        return this;
    }

    public ImageDownloader withLoadedAction(UnityAction action)
    {
        this.OnLoadedAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On loaded action set : " + action);

        return this;
    }

    public ImageDownloader withErrorAction(UnityAction<string> action)
    {
        this.onErrorAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On error action set : " + action);

        return this;
    }

    public ImageDownloader withEndAction(UnityAction action)
    {
        this.onEndAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On end action set : " + action);

        return this;
    }
    #endregion
    public static ImageDownloader Get()
    {
        GameObject g = new GameObject("Image Downloader");
      return  g.AddComponent<ImageDownloader>();
    }
    public void load(string url)
    {

    }
    public void start()
    {
        onStartAction?.Invoke();
        StartCoroutine(Downloader());
    }
    private IEnumerator Downloader()
    {
        if (enableLog)
            Debug.Log("[Davinci] Download started.");

#if UNITY_2018_3_OR_NEWER
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
#else
        var www = new WWW(url);
#endif

        while (!www.isDone)
        {
            if (www.error != null)
            {
                error("Error while downloading the image : " + www.error);
                yield break;
            }

#if UNITY_2018_3_OR_NEWER
            progress = Mathf.FloorToInt(www.downloadProgress * 100);
#else
            progress = Mathf.FloorToInt(www.progress * 100);
#endif
            if (onDownloadProgressChange != null)
                onDownloadProgressChange.Invoke(progress);

            if (enableLog)
                Debug.Log("[Davinci] Downloading progress : " + progress + "%");

            yield return null;
        }
        

#if UNITY_2018_3_OR_NEWER
        if (www.error == null)
            File.WriteAllBytes(filePath + uniqueHash, www.downloadHandler.data);
        
#else
        if (www.error == null)
            File.WriteAllBytes(filePath + uniqueHash, www.bytes);
#endif

        www.Dispose();
        www = null;

        if (onDownloadedAction != null)
            onDownloadedAction.Invoke();
        // Got Image!!
      //  loadSpriteToImage();

        processing.Remove(uniqueHash);
    }

    void error(string msg)
    {
        onErrorAction?.Invoke(msg);
    }
    public static void ClearCache(string url)
    {
        try
        {
            File.Delete(filePath + CreateMD5(url));

            if (ENABLE_GLOBAL_LOGS)
                Debug.Log($"[Davinci] Cached file has been cleared: {url}");
        }
        catch (Exception ex)
        {
            if (ENABLE_GLOBAL_LOGS)
                Debug.LogError($"[Davinci] Error while removing cached file: {ex.Message}");
        }
    }
    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
    private void finish()
    {
        if (enableLog)
            Debug.Log("[Davinci] Operation has been finished.");

        if (!cached)
        {
            try
            {
                File.Delete(filePath + uniqueHash);
            }
            catch (Exception ex)
            {
                if (enableLog)
                    Debug.LogError($"[Davinci] Error while removing cached file: {ex.Message}");
            }
        }

        if (onEndAction != null)
            onEndAction.Invoke();

        Invoke("destroyer", 0.5f);
    }

    private void destroyer()
    {
        Destroy(gameObject);
    }
}
