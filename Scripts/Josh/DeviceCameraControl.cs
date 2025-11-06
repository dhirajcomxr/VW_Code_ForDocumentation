using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DeviceCameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] UIScreen camScreen;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] GetVehicleDetailTestB64 getVehDet;
    [SerializeField] AppApiManager apiManager;
    [SerializeField] UiVehicleDetails vehicleDetails;
    [SerializeField] AutoModuleSelectHelper autoModule;
    [SerializeField] CameraUtils cam;
    public UnityAction onImageCapture;
   
    public bool imageCaptured = false;
    [SerializeField] public bool sendNow = false;
    [SerializeField] public int detailScreenNumber = 13;
    [SerializeField] StartScreen3dView selector3D;
    [SerializeField] Texture testImg;
    [SerializeField] Image vinHelperImage;

    public bool dTPhotoUpload;
    public int DTStepNumber;
    public string b64 { get; private set; }

    private void Reset()
    {
        camScreen = GetComponent<UIScreen>();
        cam = GetComponent<CameraUtils>();
        getVehDet = GetComponent<GetVehicleDetailTestB64>();
        vehicleDetails = GetComponent<UiVehicleDetails>();
        autoModule = GetComponent<AutoModuleSelectHelper>();
    }

    public void OpenCam(bool enableVinHelper)
    {
        Debug.LogError(enableVinHelper);
        if (enableVinHelper)
        {
            if (vinHelperImage)
            {
                vinHelperImage.gameObject.SetActive(enableVinHelper);
            }
        }
        else 
        { 
            vinHelperImage.gameObject.SetActive(enableVinHelper);
        }   
        screenManager.SelectScreen(camScreen.index);
    }
    void OnCapture()
    {
        Debug.Log("Confirmed Capture");
    }

    public void ClickForDT()
    {
        Debug.Log("Open Camera in DT!");
        OpenCam(false);
        onImageCapture += SendDtImage;
    }
    public void OnCancelCam()
    {
        onImageCapture = null;
    }
    public void OnCaptureDone()
    {
        onImageCapture?.Invoke();
        Debug.Log($"DTStepNumber : {DTStepNumber} && DTplayer.totalSteps : {DTplayer.totalSteps} && DTStepNumber >= DTplayer.totalSteps-1 {DTStepNumber >= DTplayer.totalSteps - 1}");
        //Dhiraj 17Jan2024 From this => DTStepNumber >= DTplayer.totalSteps-1 to DTStepNumber >= DTplayer.totalSteps
        if (DTStepNumber >= DTplayer.totalSteps)
        {
            camScreen.Close();
        }
        else
        {
            screenManager.Back();
        }
      

     }
    public Texture GetImage() => cam.image;

    public void SendDtImage()
    {
        Debug.Log("[SendDtImage]Sending Image...");
        onImageCapture -= SendDtImage;
        if (cam.image)
        {
            Debug.Log("cam.image found! Sending Image...");
            Texture2D img = (Texture2D)cam.image;
            apiManager.UploadTicketFile(img.EncodeToJPG(), OnUploadDTImage);
        }

    }
    public DTPlayer DTplayer;
    public DiagnosticTreeManager DTmanager;
    public int DTStepNum;
    public GameObject Audiofile;
    private void OnUploadDTImage(string arg0)
    {
        string fileIds = AppApiManager.GetFileUploadResponse(arg0).response_data.file_ids;
        AppLogger.LogDtStepFile("Captured Photograph",fileIds);
        Debug.Log("DT upload Image response:"+arg0);
   
    }
    
    public void SendDtVideo(byte[] VideoPath)
    {
            Debug.Log("Sending Video...");
        // screenManager.SetLoadingState(true);
        screenManager.loadingAnimated.SetActive(true);
        apiManager.UploadTicketFileforVideo(VideoPath, OnUploadDTVideo);

    }
    private void OnUploadDTVideo(string arg0)
    {
       // screenManager.SetLoadingState(false);
        screenManager.loadingAnimated.SetActive(false);
        string fileIds = AppApiManager.GetFileUploadResponse(arg0).response_data.file_ids;
        AppLogger.LogDtStepFile("Captured Video", fileIds);
        Debug.Log("DT upload Video response:" + arg0);


        /*       string nextStepId = DiagnosticStepOutputEvaluator.Evaluate(Step);
                Debug.Log("      "+nextStepId);
                //DTplayer.SetNextStep(nextStepId);*/
        DTplayer.ShowCurrStep();
       // DTmanager.SetNextStep(DTStepNum.ToString());
       // DTplayer.SetNextStep(DTStepNum.ToString());
    }
    public void SendDtAudio(byte[] AudioPath)
    {
        Debug.Log("Sending Audio...");
        //screenManager.SetLoadingState(true);
        screenManager.loadingAnimated.SetActive(true);
        apiManager.UploadTicketFileforAudio(AudioPath, OnUploadDTAudio);
    }
    private void OnUploadDTAudio(string arg0)
    {
        //screenManager.SetLoadingState(false);
        screenManager.loadingAnimated.SetActive(false);
        string fileIds = AppApiManager.GetFileUploadResponse(arg0).response_data.file_ids;
        AppLogger.LogDtStepFile("Captured Audio", fileIds);
        Debug.Log("DT upload Audio response:" + arg0);
    
        Debug.Log("   DTNum   "+DTStepNum.ToString());
        Audiofile.SetActive(false);
        DTplayer.ShowCurrStep();
    //  DTmanager.SetNextStep(DTStepNum.ToString());      
    }
    #region DELETE
    //public void ClickForVIN()
    //{
    //    OpenCam();
    //    onImageCapture += SendVinb64Now;
    //  //  SendVinb64Now();
    //}
    //public void SendVinb64Now()
    //{
    //    onImageCapture -= SendVinb64Now;
    //    if (cam.image)
    //        b64 = TextUtils.Base64Encode((Texture2D)cam.image);
    //    //    apiManager.OnRecieveVehicleDetails += ApiManager_OnRecieveVehicleDetails;
    //    apiManager.GetVehicleDetailsImg(b64, OnVehDetailResp);
    //}
    //public void SendVinImage(Texture img)
    //{
    //    if (img != null)
    //    {
    //        Debug.Log("Sending Image...");
    //        b64 = TextUtils.Base64Encode((Texture2D)img);
    //        apiManager.OnRecieveVehicleDetails += ApiManager_OnRecieveVehicleDetails;
    //        apiManager.GetVehicleDetailsImg(b64, OnVehDetailResp);
    //    }
    //}

    //private void OnVehDetailResp(string arg0)
    //{
    //    screenManager.DebugToScreen("Recieved response: "+arg0);

    //    ApiManager_OnRecieveVehicleDetails(AppApiManager.GetServerResponse(arg0).response_data);
    //}

    //private void ApiManager_OnRecieveVehicleDetails(AppApiManager.ServerData data)
    //{
    //    Debug.Log("" + data.ToString());
    //    vehicleDetails.Load(data);
    //    autoModule.Load(data.vehicle_details);
    //    screenManager.SelectScreen(detailScreenNumber);


    //}
    #endregion
}
