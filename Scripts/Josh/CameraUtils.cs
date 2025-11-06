using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using mixpanel;
public class CameraUtils : MonoBehaviour
{
    // Starts the default camera and assigns the texture to the current renderer
 
    static WebCamTexture backCam;
    [SerializeField] Canvas mainUiCanvas;
    [SerializeField] Text dbgTxt;
    [SerializeField] Text camNum;
    [SerializeField] int useCamId = 1;
    [SerializeField] RawImage viewfinder,previewImg;
    [SerializeField] private Renderer camRenderer;
    public delegate void GetPicture(Texture tex);
    public event GetPicture getPhoto; 
    public Texture image;
    Texture2D photo;
    private void Reset()
    {
        camRenderer = GetComponent<Renderer>();
        viewfinder = GetComponent<RawImage>();
    }
    public void SwitchCam()
    {
        DebugLine("Switch Cam from " + useCamId);
        useCamId++;
        if (useCamId >= WebCamTexture.devices.Length-1)
            useCamId = 0;
        if (camNum) camNum.text = "" +( useCamId+1) + "/" + (WebCamTexture.devices.Length-1);
        backCam.Stop();
        backCam = null;
        Invoke("Webcam", 0.1f);
    }
    public void ResumeCam()
    {
        if (previewImg)
            previewImg.gameObject.SetActive(false);
        backCam.Stop();
        backCam = null;
        Invoke("Webcam", 0.1f);
  
    }
    void Webcam()
    {
        if (backCam == null)
        {
            //  backCam = new WebCamTexture();
            Rect viewRect = new Rect(0, 0, 720 , 480);
            if (viewfinder != null)
                viewRect = viewfinder.GetPixelAdjustedRect();
            backCam = new WebCamTexture(WebCamTexture.devices[useCamId].name,(int)viewRect.width,(int)viewRect.height,30);
        }
        if (viewfinder != null)
        {
            viewfinder.texture = backCam;
        //    viewfinder.SetNativeSize();
        }
        else
            if (camRenderer != null)
            camRenderer.material.mainTexture = backCam;

        if (!backCam.isPlaying)
            backCam.Play();

    }
    Texture GetBlankTexture(Texture cpy)
    {
        Texture texture = new Texture2D(cpy.width, cpy.height, TextureFormat.ARGB32, false);
        // set the pixel values
        Graphics.CopyTexture(cpy, texture);
        //texture.SetPixel(0, 0, Color(1.0, 1.0, 1.0, 0.5));
        //texture.SetPixel(1, 0, Color.clear);
        //texture.SetPixel(0, 1, Color.white);
        //texture.SetPixel(1, 1, Color.black);
        
        return texture;
    }
    public void CapturePhotoNow()
    {

        //Texture afterTex = GetBlankTexture( CapturePhoto());
        StartCoroutine(TakePhoto());
        //  afterTex.name = "Photo";
        //  backCam.Stop();
        //      viewfinder.texture = afterTex;
        //if (afterTex != null)
        //    image = afterTex;
        //else
        //    DebugLine("null Image!");
        //if (previewImg)
        //{
        //    previewImg.texture = image;
        //    previewImg.gameObject.SetActive(true);

        //}
     
     // getPhoto?.Invoke(image);
    }
    public Texture GetPhoto()
    {
         CapturePhotoNow();
        return image;
    }
     Texture CapturePhoto()
    {
        DebugLine("Captured Photo");

        if (backCam)
        {
            if (!backCam.isPlaying)
                backCam.Play();  
        }
        if (viewfinder)
        {
            return viewfinder.texture;
        }
        else
        {
            Debug.LogError("Viewfinder not found!");
            return null;
        }
    }
    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("Take Picture");
        // NOTE - you almost certainly have to do this here:
        if (!backCam.isPlaying)
            backCam.Play();

        yield return new WaitForEndOfFrame();

        // it's a rare case where the Unity doco is pretty clear,
        // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
        // be sure to scroll down to the SECOND long example on that doco page 
        //++
        photo = new Texture2D(backCam.width, backCam.height, TextureFormat.ARGB32, false);
        // set the pixel values
        //++
        Graphics.CopyTexture(backCam, photo);

        //photo = new Texture2D(backCam.width, backCam.height);
        //if(backCam.isPlaying)
        //photo.SetPixels(backCam.GetPixels(),0);

       
        photo.Apply();
        image = photo;
        photo.name = "Photo";
        backCam.Stop();
        if (previewImg)
        {
            previewImg.gameObject.SetActive(true);
            previewImg.texture = photo;
            
        }
        //Encode to a PNG
        byte[] bytes = photo.EncodeToPNG();
        Mixpanel.Track("VIN_Image_Captured");
    //  getPhoto?.Invoke(image);
        //Write out the PNG. Of course you have to substitute your_path for something sensible
        //   File.WriteAllBytes(your_path + "photo.png", bytes);
    }
    //IEnumerator Start()
    //{
    //    findWebCams();

    //    yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
    //    if (Application.HasUserAuthorization(UserAuthorization.WebCam))
    //    {
    //        DebugLine("webcam found");
    //    }
    //    else
    //    {
    //        DebugLine("webcam not found");
    //    }

    //    findMicrophones();

    //    yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
    //    if (Application.HasUserAuthorization(UserAuthorization.Microphone))
    //    {
    //        DebugLine("Microphone found");
    //    }
    //    else
    //    {
    //        DebugLine("Microphone not found");
    //    }
    //    Webcam();
    //}

    void findWebCams()
    {
        foreach (var device in WebCamTexture.devices)
        {
            DebugLine("Name: " + device.name);
        }
    }

    void findMicrophones()
    {
        foreach (var device in Microphone.devices)
        {
            DebugLine("Name: " + device);
        }
    }
    string msgString = "";
    int maxLines=5, numLines = 0;
    Queue ques;
    void DebugLine(string msg)
    {
        if (ques == null)
            ques = new Queue();
        
        ques.Enqueue(msg);
        if (ques.Count >= maxLines)
            ques.Dequeue();
        msgString = "";
        foreach (var item in ques)
        {
            msgString += System.Environment.NewLine + item;       
        }
        if (dbgTxt)
            dbgTxt.text = msgString;
    }

    private void OnEnable()
    {
        if (mainUiCanvas)
            mainUiCanvas.enabled = false;
        Invoke("Webcam", 0.1f);
        //if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        //    InitCam();
        //else
        //{
        //    Application.RequestUserAuthorization(UserAuthorization.WebCam);
        //    StartCoroutine(CheckAuthorization());
        //}

    }

    // IEnumerator CheckAuthorization()
    // {
    //     if (Application.HasUserAuthorization(UserAuthorization.WebCam))
    //     {
    //         InitCam();
    //         yield return new WaitForEndOfFrame();
    //     }
    //     else
    //         DebugLine("Running");
    // }
    // void InitCam()
    // {
    //     DebugLine("Initialising Cam.."+Application.HasUserAuthorization(UserAuthorization.WebCam));
    //     WebCamTexture webcamTexture = new WebCamTexture();
    //     // renderer = GetComponent<Renderer>();
    //     if (camRenderer != null)
    //         camRenderer.material.mainTexture = webcamTexture;
    //     if (rawImage != null)
    //         rawImage.texture = webcamTexture;
    //     webcamTexture.Play();
    // }
    // void UpdateCamera()
    // {

    // }
    public static Texture2D GetScreenshot()
    {
      return  ScreenCapture.CaptureScreenshotAsTexture();
    //    Application.CaptureScreenshot(Application.persistentDataPath + "/" + "ID-" + System.DateTime.Now);
    }
}