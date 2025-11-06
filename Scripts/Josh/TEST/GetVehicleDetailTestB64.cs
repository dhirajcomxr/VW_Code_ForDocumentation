using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVehicleDetailTestB64 : MonoBehaviour
{
    [SerializeField] public AppApiManager apiManager;
   
    [SerializeField] CameraUtils cam;
    [SerializeField] UiVehicleDetails vehicleDetails;
    [SerializeField] AutoModuleSelectHelper autoModuleSelector;
    [SerializeField] public string b64;
    [SerializeField] public bool sendNow = false;
    [SerializeField] public int detailScreenNumber = 13;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] StartScreen3dView selector3D;
    [SerializeField] Texture testImg;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sendNow)
        {
            sendNow = false;
            SendImage(testImg);        
        }  
    }
    public void SendVinb64Now()
    {
        if (cam.image)
            b64 = TextUtils.Base64Encode((Texture2D)cam.image);
    //    apiManager.OnRecieveVehicleDetails += ApiManager_OnRecieveVehicleDetails;
        apiManager.GetVehicleDetailsImg(b64,OnVehDetailResp);
    }
    public void SendDtImage(Texture2D img)
    {
        if (img != null)
        {
            Debug.Log("Sending Image...");
            apiManager.UploadTicketFile(img.EncodeToJPG(), OnUploadDTImage);
        }
    }

    private void OnUploadDTImage(string arg0)
    {
        Debug.Log("Uploaded DT Image");
    }

  
    public void SendImage(Texture img)
    {
        if (img != null)
        {
            Debug.Log("Sending Image...");
            b64 = TextUtils.Base64Encode((Texture2D)img);
            apiManager.OnRecieveVehicleDetails += ApiManager_OnRecieveVehicleDetails;
            apiManager.GetVehicleDetailsImg(b64,OnVehDetailResp);
        }
    }

    private void OnVehDetailResp(string arg0)
    {
        ApiManager_OnRecieveVehicleDetails(AppApiManager.GetServerResponse(arg0).response_data);
    }

    private void ApiManager_OnRecieveVehicleDetails(AppApiManager.ServerData data)
    {
        Debug.Log(">>>>>>>   "+data.ToString());
        screenManager.SelectScreen(detailScreenNumber);
        vehicleDetails.Load(data);
        autoModuleSelector.Load(data.vehicle_details);
       
    }
}
