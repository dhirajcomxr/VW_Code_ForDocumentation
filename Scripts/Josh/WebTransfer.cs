using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class WebTransfer : MonoBehaviour
{
    string mainUrl = "http://server/upload/";
    string uid = "";
    string uName = "";
    string saveLocation;

  [SerializeField]  Texture2D img;
    [SerializeField] Texture tex;
    [SerializeField] CameraUtils cam;
    class AuthData
    {
        public static string api_key = "261f3c706777fda1062f945116f6e564";
        public static string link = "https://api.imgbb.com/1/upload";
    }
    private void OnEnable()
    {
        cam.getPhoto += OnCameraClick;
        cam.CapturePhotoNow();
      
    }
    void OnCameraClick(Texture texture)
    {
        cam.getPhoto -= OnCameraClick;
        tex = texture;
        Debug.Log("Got pic");
    }
    void Start()
    {
        saveLocation = "ftp:///home/yyy/x.zip"; // The file path.
       
       // StartCoroutine(PrepareFile());
    }
    IEnumerator SendFile(byte[] imageData, string path)
    {

        WWWForm form = new WWWForm();
        string filename = Path.GetFileName(path);
     //   string filepathname = Data_Manager.steamID + "-" + Data_Manager.steamName + "-" + filename;
        string filepathname = "Files-"+ filename;


        form.AddBinaryData("image", imageData, filepathname, "image/png");
        form.AddField("key", AuthData.api_key);
        form.AddField("steamName", uName);

        using (UnityWebRequest www = UnityWebRequest.Post(AuthData.link, form))
        {
            yield return www.SendWebRequest();

            if (www.result==UnityWebRequest.Result.ConnectionError || www.result==UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Print response
                Debug.Log(www.downloadHandler.text);
                Debug.Log(www.responseCode);
                Debug.Log(www.result.ToString());
            }
        }
    }
    // Prepare The File.
    IEnumerator PrepareFile()
    {
        Debug.Log("saveLoacation = " + saveLocation);

        // Read the zip file.
        WWW loadTheZip = new WWW(saveLocation);

        yield return loadTheZip;

        PrepareStepTwo(loadTheZip);
    }

    void PrepareStepTwo(WWW post)
    {
        StartCoroutine(UploadTheZip(post));
    }

    // Upload.
    IEnumerator UploadTheZip(WWW post)
    {
        // Create a form.
        WWWForm form = new WWWForm();

        // Add the file.
        form.AddBinaryData("myTestFile.zip", post.bytes, "myFile.zip", "application/zip");

        // Send POST request.
        string url = mainUrl;
        WWW POSTZIP = new WWW(url, form);

        Debug.Log("Sending zip...");
        yield return POSTZIP;
        Debug.Log("Zip sent!");
    }
}
