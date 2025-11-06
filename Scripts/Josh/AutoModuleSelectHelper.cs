using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using mixpanel;
public class AutoModuleSelectHelper : MonoBehaviour
{
    [SerializeField] StartScreen3dView select3D;
    [SerializeField] AppApiManager apiManager;
    [SerializeField] ScreenLinker linker;
    [SerializeField] DeviceCameraControl cameraControl;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] InputField vinInput;
    [SerializeField] ManualSelector manualSelector;
    public Dropdown hvacSelector;
    public string carName;
    //public string carVariantName;
   // [SerializeField] string getVehicalNameResumeTicket;
    [System.Serializable]
    public class VinHistories
    {
        public List<VinHistoryPage> historyPages;
    }
    [SerializeField]
    public VinHistories vinHistory;
    //[SerializeField]
    //public List<VinHistoryPage> vinHistory;
    [SerializeField] string testString;
    [SerializeField] ImageLabelList vinList;
    int historyLimit = 3;
    string b64;
    [System.Serializable]
    public class VinHistoryPage
    {
        public string vin, color, model;
        public VinHistoryPage(string _vin,string _color,string _model)
        {
            vin = _vin;
            color = _color;
            model = _model;
        }
    }
    // Start is called before the first frame update
    private void OnEnable()
    {
        if(!linker)
        linker = FindObjectOfType<ScreenLinker>();
     //   apiManager.OnRecieveVehicleDetails += OnRecieveVinDetails;
      
    }
    [SerializeField] bool testVinHString = false;
    void Start()
    {
        //     testString = JsonUtility.ToJson(vinHistory.ToArray());
        Init();
        testString = JsonUtility.ToJson(vinHistory);  //get the old vin history
        Debug.Log("Vin History: " + testString);

    }
    public void Init()
    {
        linker.GetApiManager().ResetTicket();
        GetVinHistory(); 
    }
    public void GetVinImage()
    {
        cameraControl.OpenCam(true);
        cameraControl.onImageCapture += SendVinb64Now;
      
        Mixpanel.Track("VIN_Scanner_Enabled");
    }

    //Send vin number manually to get a specific vehicle 
    public void SendEnteredVin(InputField enterVin)
    {
        if (enterVin.text.Length > 0)
        {
            apiManager.onReqError.AddListener(OnReqError);
            apiManager.GetVehicleDetails(enterVin.text, OnVehDetailResp);
            screenManager.SetLoadingState(true);
            Mixpanel.Track("VIN_Entered_Manually");

        }
        else
            screenManager.Toast("Please Enter VIN to continue");
    }
   public void SelectVIN(int vinHNum)
    {
        vinInput.text = vinHistory.historyPages[vinHNum].vin;
    }
    private void OnApplicationPause(bool pause)
    {
        SaveVinHistory();
    }
    private void OnApplicationQuit()
    {
        SaveVinHistory();
    }


    #region Vin History

    void GetVinHistory()
    {
        Debug.Log("Getting VIN History..");

        string savedVins = PlayerPrefs.GetString("vinHistory");
        if (savedVins.Length > 0)
        {
            //    Debug.Log("Saved Data: " + savedVins);
            vinHistory = JsonUtility.FromJson<VinHistories>(savedVins);
        }

        else
        {
            vinHistory = new VinHistories();
            vinHistory.historyPages = new List<VinHistoryPage>();

        }
        if (vinHistory.historyPages.Count > 0)
        {
            List<string> vins = new List<string>();
            for (int i = 0; i < vinHistory.historyPages.Count; i++)
            {
                Debug.Log(vinList);
                Debug.Log(vinHistory.historyPages[i].vin.ToString()+"VIN Number History..............");
                vins.Add(vinHistory.historyPages[i].vin);              
            }
            List<string> VinsUniq = vins.Distinct().ToList();
            VinsUniq.Reverse();
            vinList.Load(VinsUniq.ToArray());
        }
    }

    //Add the old VIN number after entering it manually
    void AddVinToHistory(AppApiManager.VehicleDetails vDetails)
    {
        bool found = false;
        int foundOn = -1;
      //  if (vinHistory.historyPages.Count < 1)
            vinHistory.historyPages.Add(new VinHistoryPage(vDetails.VIN, vDetails.ColorDesc, vDetails.ModelName));
      
        if (vinHistory.historyPages.Count > historyLimit)
            EnforceLimit();
        SaveVinHistory();
        Debug.Log("Added :" + vDetails.VIN);
       
        void EnforceLimit()
        {
            if(vinHistory.historyPages.Count>historyLimit)
            vinHistory.historyPages.RemoveRange(0, historyLimit);
        }
    }

    //Save the entered VIN number
    void SaveVinHistory()
    {
        Debug.Log("Saving VIN History..."+vinHistory.historyPages.Count); 
        string vinHString = JsonUtility.ToJson(vinHistory);

        Debug.Log("Saving " + vinHString);
        PlayerPrefs.SetString("vinHistory", vinHString);
        PlayerPrefs.Save();
    }
    #endregion
    private void SendVinb64Now()
    {
        cameraControl.onImageCapture -= SendVinb64Now;
        Texture img = cameraControl.GetImage();
        if (img != null)
        {
            b64 = TextUtils.Base64Encode((Texture2D)img);
            //    apiManager.OnRecieveVehicleDetails += ApiManager_OnRecieveVehicleDetails;
            apiManager.GetVehicleDetailsImg(b64, OnVehDetailResp);
            apiManager.onReqError.AddListener(OnReqError);
            screenManager.SetLoadingState(true);
        }
    }
    void OnReqError(string msg)
    {
        Debug.Log("On REQ Error..........");
        screenManager.SetLoadingState(false);
        screenManager.DebugToScreen("error: "+msg);
      
        apiManager.onReqError.RemoveListener(OnReqError);
    }
    public void SetLocalisation(string arg0)
    {
        AppApiManager.ServerResponseVehDetails serverResp = AppApiManager.GetServerResponseVehicleDetails(arg0);
        linker.GetModuleLoader().SetModuleReferences(AppApiManager.GetServerResponseVehicleDetails(arg0).response_data.vehicle_modules);
    }

    //Get vechicle details from the server
    private void OnVehDetailResp(string arg0)
    {
        
        Debug.Log("[SERVER RESP] [Get Vehicle Detail] " + arg0);
        apiManager.onReqError.RemoveListener(OnReqError);
        screenManager.SetLoadingState(false);
        AppApiManager.ServerResponseVehDetails serverResp = AppApiManager.GetServerResponseVehicleDetails(arg0);
        Debug.Log("[Get Vehicle Detail]" + serverResp.response_data.vehicle_details.ModelName);

       // manualSelector.openharness(serverResp.response_data.vehicle_details.ModelName);//open harness

        carName = serverResp.response_data.vehicle_details.ModelName;
        linker.GetModuleLoader().SetModuleReferences(AppApiManager.GetServerResponseVehicleDetails(arg0).response_data.vehicle_modules);
        if (serverResp.status.Contains("fail"))
        {
            Mixpanel.Track("VIN_Details_Not_Received");
            screenManager.Toast(serverResp.response_data.message);
            if (serverResp.response_data.message.Length > 0)
            {
                screenManager.Toast(serverResp.response_data.message);
                //string vin = serverResp.response_data.vin;
                //if (vin.Length > 0)
                //    screenManager.Toast("Could not retrieve details for this VIN " + serverResp.response_data.vin);
                //else
                //    screenManager.Toast("Could not find VIN in this image!");
                if(serverResp.response_data.vin.Length>0)
                vinInput.text = serverResp.response_data.vin;
            }
        }
        else
        if (serverResp.message != null)
            if (serverResp.message.Length > 0)
            {
                Debug.Log("OnVehDetailResp..............");
                screenManager.Toast(serverResp.message);
                screenManager.SelectScreen(ScreenLinker.ScreenID.VIN_INPUT);
            }
           
        if (serverResp.response_data.vehicle_details.VIN != null)
            if (serverResp.response_data.vehicle_details.VIN.Length > 0)
            {
                Debug.Log("response_data.vehicle_details.VIN ");
                AddVinToHistory(serverResp.response_data.vehicle_details);
                Debug.Log("<color=blue>[VEH-DET]</color>"+JsonUtility.ToJson(serverResp.response_data.vehicle_details));  //Fetch details of a specific vehicle
                apiManager.SetCurrentVehicleDetails(serverResp.response_data.vehicle_details);
                OnRecieveVehicleDetails(serverResp.response_data.vehicle_details);
                Mixpanel.Track("VIN_Details_Received");
            }
       
        //  screenManager.DebugToScreen("Recieved response: " + arg0);
  //     OnRecieveVehicleDetails(AppApiManager.GetServerResponse(arg0).response_data);
    }
  
    //Enable vehicle screen and set all the details on it. 
    private void OnRecieveVehicleDetails(AppApiManager.VehicleDetails data)
    {
        Debug.Log(data.ToString());
        linker.GetManualSelector().SetModuleGroups();
        linker.GetVehicleDetailsScreen().Load(data);
      
        Load(data);
        Debug.Log("OnRecieveVehicleDetails..............");
      //  screenManager.SelectScreen(ScreenLinker.ScreenID.VEHICLE_DETAILS);        
    }
    void OnRecieveVinDetails(AppApiManager.ServerData serverData)
    {
        Debug.Log("Got sD Response for :" + JsonUtility.ToJson(serverData));
        Load(serverData.vehicle_details);
    }

    //Load a car with all the details fetched below
   public void Load(AppApiManager.VehicleDetails vehicleDetails)
    {
        Debug.Log("Got vD Response for :" + JsonUtility.ToJson(vehicleDetails));
        //string[] modules = new string[] { vehicleDetails.Engine,
        //    vehicleDetails.Transmission,vehicleDetails.HVAC};
        //if (select3D.CarExist(vehicleDetails.ModelName))
        //    select3D.SelectCar(select3D.GetCarWithName(vehicleDetails.ModelName));   
        // SelectModules(modules);
        ManualModuleSelectHelper.VarType variantType = new ManualModuleSelectHelper.VarType();
        variantType.variant = vehicleDetails.VariantName;
        Debug.Log("Vehical Variant Name: " + variantType.variant);
        linker.SetSelectedVariant(variantType);
        linker.ShowVehicleDetails(vehicleDetails);
        LoadVehicleAndModules(vehicleDetails);
    }

    public void LoadVehicleAndModules(AppApiManager.VehicleDetails vehicleDetails)
    {
        string[] modules = new string[] { vehicleDetails.Engine,
            vehicleDetails.Transmission,vehicleDetails.HVAC, vehicleDetails.VariantName};
        //carVariantName = vehicleDetails.VariantName;
        linker.GetSelectedVariant().variant = vehicleDetails.VariantName;
        Debug.Log("MODEL:"+vehicleDetails.ModelName+
            ", ENG:"+vehicleDetails.Engine+", TRANS:"+vehicleDetails.Transmission +
            ", HVAC:"+vehicleDetails.HVAC + 
            "Variant: " + vehicleDetails.VariantName
            );

       
        if (select3D.CarExist(vehicleDetails.ModelName))
        {
            select3D.SelectCar(select3D.GetCarWithName(vehicleDetails.ModelName));
            //manualSelector.openharness(vehicleDetails.ModelName);   
        }
        Debug.Log("LoadVehicleAndModules.................." + vehicleDetails.ToString());
        SelectModules(vehicleDetails);
    }
    bool useStringBase = true;

    void SelectModules(AppApiManager.VehicleDetails vehDetails)
    {
        if (useStringBase)
        {
            Debug.Log("Selecting Via String");
            select3D.SelectModuleFromGroups(vehDetails.Engine, vehDetails.Transmission, vehDetails.HVAC);
        }
        else
        {
            Debug.Log("<color=red>Engine Selection:</color> " + vehDetails.Engine);
            int selectedEngine = select3D.GetModuleIdFor(vehDetails.Engine);
            int selectedGearbox = select3D.GetModuleIdFor(vehDetails.Transmission);
            int selectedHvac = select3D.GetModuleIdFor(vehDetails.HVAC);
            int selectedVariantName = select3D.GetModuleIdFor(vehDetails.VariantName);
            //if (selectedEngine < 0)
            //    Debug.Log(moduleNames[0].ToString());
            //if (selectedGearbox < 0)
            //    Debug.Log(moduleNames[1].ToString());
            //if (selectedHvac < 0)
            //    Debug.Log(moduleNames[2].ToString());
            Debug.Log("<color=blue> Engine  </color>" + selectedEngine);
            Debug.Log("<color=blue> Gearbox </color>" + selectedGearbox);
            Debug.Log("<color=blue> HVAC </color>" + selectedHvac);
            Debug.Log("<color=blue> Variant_Name </color>" + selectedVariantName);
            select3D.SelectActiveModule(selectedEngine, 0, 1);
            select3D.SelectActiveModule(selectedGearbox, 4, 7);
            select3D.SelectActiveModule(selectedHvac, 2, 3);
            Debug.Log("<color=blue> Gearbox </color>" + selectedGearbox);
        }
        select3D.InitialiseDropdown();
    }
}
