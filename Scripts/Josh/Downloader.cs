using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Linq;

public class Downloader : MonoBehaviour
{
    string downloadUrl = "";
 public string textData = "";
  UnityAction<Downloader> onError, onComplete;
  public static Downloader Get()
    {

        GameObject downloader = new GameObject("[Downloader]");
       return downloader.AddComponent<Downloader>();    
    }
    public Downloader Download(string url)
    {
        downloadUrl = url;
        gameObject.name ="[DOWNLOADER] "+ url.Split('/').Last();
        StartCoroutine(GetText());
        return this;
    }
    public void OnError(UnityAction<Downloader> errorCallback)
    {
        onError = errorCallback;
    }
    public void OnComplete(UnityAction<Downloader> completeCallback)
    {
        onComplete = completeCallback;
    }
    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get(downloadUrl);
        //Debug.Log(downloadUrl + "GET TEXT");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            textData = www.error;
            onError?.Invoke(this);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            textData = www.downloadHandler.text;
            onComplete?.Invoke(this);
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
    public void Close()
    {
        Destroy(gameObject);
    }
}
