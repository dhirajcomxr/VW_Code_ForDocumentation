using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class VideoAudioUpload : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

   /* public AppApiManager APIManager;
    public string AVpath;
    byte[] AVByte;
    public void SentVideoData()
    {
      //  APIManager.UploadTicketFile(.EncodeToJPG(, OnUploadDTImage);
        // StartCoroutine(Upload());
    }
    private void OnUploadDTVideo(string arg0)
    {
        string fileIds = AppApiManager.GetFileUploadResponse(arg0).response_data.file_ids;
        AppLogger.LogDtStepFile("Captured Photograph", fileIds);
        Debug.Log("DT upload Image response:" + arg0);
    }*/
    /*  IEnumerator Upload()
      {
         byte[] myData = AVByte.ReadAllBytes(AVpath);
          UnityWebRequest www = UnityWebRequest.Put(APIManager.GetCurrentServerUrl(), myData);
          yield return www.SendWebRequest();

          if (www.result != UnityWebRequest.Result.Success)
          {
              Debug.Log(www.error);
          }
          else
          {
              Debug.Log("Upload complete!");
          }
      }*/
    void Update()
    {
        
    }
}
