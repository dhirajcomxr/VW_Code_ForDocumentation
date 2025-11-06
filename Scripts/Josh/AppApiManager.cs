using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AppApiManager : MonoBehaviour
{
    public string editorId= "mac99990";
    public Ticket currentTicket;
    public static bool EDITOR_MODE = true;
    VehicleCluster[] clusters;
    [SerializeField] bool enableDebug = true;
    [SerializeField] Response currResponse;
    [SerializeField]
    ServerResponse response;

    [SerializeField]public ServerData serverData;
    public Save_DT_Log Save_Log;
    [SerializeField] VehicleDetails curVehicleDetails;
    [SerializeField] ServerVehicleModuleFileRef[] vehicle_modules;
    public UnityEvent<string> onReqError;
    [SerializeField] string[] bundleAddresses;
    public ServerType serverType;
    [SerializeField]
   // [SerializeField]
    List<KeyValuePair<string, string>> sendData;
    // [SerializeField]
    List<UnityWebRequest> errorRequests;

    bool useForm = false;
    string languageCode = "en";

    public static string ticketToEscalate = "";
    #region Data Classes
    public enum ServerType
    {
        Development, Staging, Production
    }
    [System.Serializable]
    class RequestHistory
    {
        public string url;
        public List<KeyValuePair<string, string>> data;
        public ApiCallStatus responseCall;
    }
    public delegate void ApiCallStatus(string text);
    public delegate void RecieveData(ServerData data);
    public event RecieveData OnRecieveVehicleDetails, OnRecieveTechnicians;
    public event ApiCallStatus OnApiResponse, OnLogin, OnLogout, OnVehicleDetails,OnForgot;
    public UnityAction<string> OnLogToScreen;
    public List<Ticket> openTickets;
    private string modelCode = "";
    private string curServerUrl = "";
    // Ticket currentTicket;
    static class ServerUrls
    {
        public static string
            development = "https://dev.in/api",
            //development = "https://compeny.com/api",
            staging = "https://idstaging.com/api",
            production = "https://id.com/api";
    }
    static class ApiUrls
    {
        public static string
             saveFcm = "save-fcm/",
            login = "login/",
            logout = "logout/",
            forgotPassword = "forgot-password/",

            getTechnicians = "get-technicians/",
            getVehicleMasters = "get-vehicle-masters/",
            saveTicketClusters = "save-ticket-clusters/",
            getVehicle = "get-vehicle/",
            getVehicleDetails = "get-vehicle-details/",
            getVehicleDetailsWVin = "get-vehicle-details-without-vin/",
            createTicket = "create-ticket/",
            getTicketDetails = "get-ticket-details/",
            getTickets = "get-tickets/",
            saveTicketDiagStep = "save-ticket-diag-step/",
            getDiagTree = "get-diagnostic-trees/",
            translate = "translate/",
            uploadTicketFiles = "upload-ticket-files/",
            sendLogToMail = "email-ticket-log/",
            createSystemTicket = "create-system-ticket/",
            createItTicket = "create-it-ticket/",
            escalateTicket = "escalate-ticket/",
           /// Dhiraj _ 6-6-2024 _ New api for reassigning the ticket
            reassignTicket = "reassign-ticket/",
           ///
            updateTicketStatus = "update-ticket-status/",
            updateTicket = "update-ticket/",
            getAllDts = "get-all-dts/",
        createManTechTicket = "create-man-tech-ticket/";
    }
    [System.Serializable]
    class UserData
    {
        public string
            fcm_id = "",            //FCM Registration ID
            make = "",              //Make / Brand of the Device
            model = "",             //Model of the device
            os = "",                //Device OS
            version = "",           //Device OS Version
            mac = "",               //MAC Address
            username = "",          //Username
            password = "",          //Password
            session_id = "",        //Session ID
            vehicle_id = "",        //Vehicle ID generated from sending VIN
            vin = "",               //VIN
            short_desc = "",        //Short Desc (Max Length 100)
            long_desc = "",         //Long Desc (Max Length 1000)
            email = "",             //Email ID for Forgot Password
            ticket_id = "",         //Ticket ID
            ticket_status = "",     //Open / Closed / Paused
            is_escalated = "",      //Y / N
            escalation_remark = "", //Escalation Remark
            offset = "",            //Get-Tickets:No. of records to skip
            limit = "";             //Get-Tickets :Default is 10


    }
    [System.Serializable]
    public static class UserDataKV
    {

        public static KeyValuePair<string, string>

            api_key = new KeyValuePair<string, string>("api_key", "TestingAPIKey12345"), //API Key
            fcm_id = new KeyValuePair<string, string>("fcm_id", ""),                      //FCM Registration ID
            make = new KeyValuePair<string, string>("make", ""),                           //Make / Brand of the Device
            model = new KeyValuePair<string, string>("model", ""),                          //Model of the device
            os = new KeyValuePair<string, string>("os", ""),                               //Device OS
            version = new KeyValuePair<string, string>("version", ""),                     //Device OS Version
            mac = new KeyValuePair<string, string>("mac", ""),                             //MAC Address
            user_id = new KeyValuePair<string, string>("user_id", ""),                   //Username
            password = new KeyValuePair<string, string>("password", ""),                   //Password
            session_id = new KeyValuePair<string, string>("session_id", ""),                //Session ID
            vehicle_id = new KeyValuePair<string, string>("vehicle_id", ""),               //Vehicle ID generated from sending VIN
            vin = new KeyValuePair<string, string>("vin", ""),                             //VIN
            image_data = new KeyValuePair<string, string>("image_data", ""),               //Image string in b64
            short_desc = new KeyValuePair<string, string>("short_desc", ""),               //Short Desc (Max Length 100)
            long_desc = new KeyValuePair<string, string>("long_desc", ""),                  //Long Desc (Max Length 1000)
           // kms_done = new KeyValuePair<string, string>("kms_done",""),
            email = new KeyValuePair<string, string>("email", ""),                         //Email ID for Forgot Password
            ticket_id = new KeyValuePair<string, string>("ticket_id", ""),                  //Ticket ID
            ticket_status = new KeyValuePair<string, string>("ticket_status", ""),         //Open / Closed / Paused
            ticket_type = new KeyValuePair<string, string>("ticket_type", ""),             //System / IT
            is_escalated = new KeyValuePair<string, string>("is_escalated", ""),            //Y / N
            escalation_remark = new KeyValuePair<string, string>("escalation_remark", ""),  //Escalation Remark
            offset = new KeyValuePair<string, string>("offset", ""),                       //Get-Tickets:No. of records to skip
            event_type = new KeyValuePair<string, string>("event_type", ""), //Yes rnr / diagnostic_tree / wiring_harness
  process_name = new KeyValuePair<string, string>("process_name", ""),//   No dismantle / assembly / wiring harness
  loaded_module_ids = new KeyValuePair<string, string>("loaded_module_ids", ""),// No  Comma separated ids
  step_number = new KeyValuePair<string, string>("step_number", ""),// No Step Number
  part_id = new KeyValuePair<string, string>("part_id", ""),// No Unique part id
  diagnostic_tree_id = new KeyValuePair<string, string>("diagnostic_tree_id", ""), // No  Diagnostic Tree ID
  activity_unique_number = new KeyValuePair<string, string>("activity_unique_number", ""),//  No Activity Unique Number
  addon_data = new KeyValuePair<string, string>("addon_data", ""),// No  Any additional info in JSON format
  file_ids = new KeyValuePair<string, string>("file_ids", ""),// No  Uploaded File Ids
            step_description = new KeyValuePair<string, string>("step_description", ""),
            question = new KeyValuePair<string, string>("question", ""),
            answer = new KeyValuePair<string, string>("answer", ""),
            ticket_files = new KeyValuePair<string, string>("ticket_files", ""),
            language_code = new KeyValuePair<string, string>("language", "en"),
            limit = new KeyValuePair<string, string>("limit", "");                         //Get-Tickets :Default is 10
    }

    #endregion
    #region Server Response Classes
    [System.Serializable]
    public class Ticket
    {
        public string ShortDescription;
        public string TicketID;
        public string VehicleID;
        public string LongDescription;
        public string TicketType;
        public string TicketUID;
        public string TicketStatus;
        public string ClosedDateTime;
        public string ExperienceRating;
        public string AddedDateTime;
        public string UpdatedDateTime;
        public string IsEscalatedToL1 = "";
        public string EscalationRemark;
        public string ClosingRemark;
        public string VIN;
    }
    [System.Serializable]
    public class ProfileInfo
    {
        public string FirstName;// { get; set; }
        public string LastName;// { get; set; }
        public string Mobile;// { get; set; }
        public string EmailID;// { get; set; }
    }
    [System.Serializable]
    public class Save_DT_Log
    {
        public string VIN, FirstName, LastName, Email, Comment;
    }

    [System.Serializable]
    public class Technician
    {
        public string UserID, FirstName, LastName, ProfilePicture, Email;
    }
    [System.Serializable]
    public class VehicleDetails
    {
        public string VehicleID;
        public string VIN;
        public string CommissionNumber;
        public string ModelCode;
        public string ModelName;
        public string EngineNumber;
        public string Engine;
        public string HVAC;
        public string Transmission;
        public string VariantName;
        public string ProductionDate;
        public string ColorCode;
        public string ColorDesc;
        public string SaleDate;
        public string DeliveryDate;
        public string CustomerCity;
        public string CustomerState;
        public string DealershipCode;
        public string DealershipName;
    }
    [System.Serializable]
    public class TicketDetails
    {
        public string TicketID;
        public string ShortDescription;
        public string LongDescription;
        public string TicketStatus;
        public string ClosedDateTime;
        public string ExperienceRating;
        public string AddedDateTime;
        public string IsEscalatedToL1;
        public string EscalationRemark;
        public string ClosingRemark;
    }
    [System.Serializable]
    public class TicketHistory
    {
        public string Activity;
        public string TicketStatus;
        public string AddedDateTime;
    }
    [System.Serializable]
    public class TicketDiagStep
    {
        public string session_id,
  ticket_id,
  event_type, //Yes rnr / diagnostic_tree / wiring_harness
  process_name,//   No dismantle / assembly / wiring harness
  loaded_module_ids,// No  Comma separated ids
  step_number,// No Step Number
  part_id,// No Unique part id
  diagnostic_tree_id, // No  Diagnostic Tree ID
  activity_unique_number,//  No Activity Unique Number
  addon_data,// No  Any additional info in JSON format
  file_ids;// No  Uploaded File Ids
    }
    [System.Serializable]
    public class ResponseData
    {

    }
    [System.Serializable]
    public class ServerData : ResponseData
    {
        public int attempts_left;// { get; set; }
        public string account_locked;// { get; set; }
        public string session_id;// { get; set; }
        public string ticket_id;
        public string ticket_uid;
        public string ticket_type;
        public string message;
        public string vin;
        public string brand;
        public ProfileInfo profile_info;
        //  public TicketDetails ticket_details;
        public Ticket ticket_details;
        public VehicleDetails vehicle_details;
        public Technician[] technicians;
        public VehicleMasters masters;
        public List<Ticket> tickets;
        public int total_records;
        public int records_returned;
        public bool load_more;

        public List<TicketHistory> ticket_history;

        public Diagnostic_Trees[] diagnostic_trees;
    }
    [System.Serializable]

    public class Diagnostic_Trees
    {
        public string DiagnosticTreeID; // { get; set; }
        public string IssueTitle; // { get; set; }
        public string IssueDescription; // { get; set; }
        public string DTFVersion; // { get; set; }
        public string DTStatus; // { get; set; }   
        public string FilesContainer;
        public string DT_JSON;
      
    }
    public class LoginResponse : ResponseData
    {
        public int attempts_left;// { get; set; }
        public string account_locked;// { get; set; }
        public string session_id;// { get; set; }
        public ProfileInfo profile_info;// { get; set; }
    }
    [System.Serializable]
    public class Response
    {
        public string status;
        public string message;
        public string app_ver;
        public bool force_upgrade;
        public bool maintenance;
        //    public ResponseData response_data;
    }
    [System.Serializable]
    public class ServerFileUpload
    {
        public string message, file_ids;
    }
    [System.Serializable]
    public class ServerResponseVehDetRespData
    {
        public string message;
        public string vin;
        public VehicleDetails vehicle_details;
        public ServerVehicleModuleFileRef[] vehicle_modules;
        public string[] android_asset_bundles, windows_asset_bundles;
    }
    [System.Serializable]
    public class ServerResponseVehDetails : Response
    {
        public ServerResponseVehDetRespData response_data;
    }
    [System.Serializable]
    public class ServerFileUploadResponse : Response
    {
        //public string status;
        //public string message;
        //public string app_ver;
        //public bool force_upgrade;
        //public bool maintenance;
        public string brand;
        public ServerFileUpload response_data;
    }
    [System.Serializable]
    public class ServerResponse : Response
    {
        //public string status;
        //public string message;
        //public string app_ver;
        //public bool force_upgrade;
        //public bool maintenance;
        public string brand;
        public ServerData response_data;
    }
    [System.Serializable]
    public class ServerLoginResponse : Response
    {
        [SerializeField]
        public LoginResponse response_data;
    }
    [System.Serializable]
    public class ServerResponseVehicleMasters : Response
    {
        [SerializeField]
        public VehicleMasters response_data;
    }
    [System.Serializable]
    public class ServerVehicleModule
    {
        //     public string name;
        public string ModuleName;
        public string ModuleID;

    }
    [System.Serializable]
    public class ServerVehicleModuleFileRef
    {
        //     public string name;
        public string ModuleName;
        public string ModuleID,
        ModuleAddress
            , ModuleType, ModuleDescription,
            AssemblyFileName
            , DismantlingFileName,
            ServiceablePartsFileName,
            PartSequenceFileName,
            ModuleVersion;
    }
    [System.Serializable]
    public class VehicleCluster
    {
        public string ClusterName;
        public string ClusterID;

    }
    [System.Serializable]
    public class VehicleMasters
    {
        public string[] models, variants, model_years;
        public VehicleCluster[] clusters;
        public ServerVehicleModuleFileRef[] vehicle_modules;
       // public List<ManualModuleSelectHelper.VarType> variant_types;
        public List<ManualModuleSelectHelper.VarType> variant_types;
        [System.Serializable]
        public class ModuleSections
        {
            //public ServerVehicleModule[] engine, suspension,
            //                transmission, hvac, body, brakes;

            //[FormerlySerializedAs("1.5l engine wiring harness")]
            //public ServerVehicleModule[] wiringharness1_5l;
            //[FormerlySerializedAs("1l engine wiring harness")]
            //public ServerVehicleModule[] wiringharness1l;
            //[FormerlySerializedAs("central wiring harness")]
            //public ServerVehicleModule[] centralwiringharness;
        }
    }
    [System.Serializable]
    public class Checkstatus
    {
        public string status;
    }
    #endregion
    #region API Functions
    void SetServerUrl(ServerType type)
    {
        switch (type)
        {
            case ServerType.Development:
                curServerUrl = ServerUrls.development;
                break;
            case ServerType.Staging:
                curServerUrl = ServerUrls.staging;
                break;
            case ServerType.Production:
                curServerUrl = ServerUrls.production;
                break;
            default:
                break;
        }
    }
    public ServerType GetServerType() => serverType;
    public void SetServerType(ServerType type)
    {
        Debug.Log("Setting Server to " + type.ToString());
        serverType = type;
        SetServerUrl(type);
    }
    public VehicleDetails manualVehicleDetails;
    public void SetManualVehicalDetails()
    {
        curVehicleDetails = manualVehicleDetails;
    }

    public string GetCurrentServerUrl() => curServerUrl;


    public void ForgotPassword(string username,string Email)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.fcm_id.Value(GetFCMID()), UserDataKV.mac.Value(GetUniqueId()),
            UserDataKV.user_id.Value(username), UserDataKV.email.Value(Email) });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.forgotPassword, sendData, OnResponseForgot));
    }

    public void OnResponseForgot(string Data)
    {
        Debug.Log("On Response Forgot " + Data);
        if (Data.Length > 0)
        {
          //  serverData.profile_info = GetResponse(data).response_data.profile_info;
          // serverData.session_id = GetResponse(data).response_data.session_id;
           // serverData.message = GetResponse(data).response_data.message;
           OnForgot(response.status + ":" + serverData.message);// + ":" + response.message.ToString());
        }
        else
            DebugLog("No Response from server!");

    }
    public void Login()
    {
        //sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        //{
        //    UserDataKV.api_key,
        //    UserDataKV.fcm_id.Value(GetFCMID()), UserDataKV.mac.Value(GetUniqueId()),
        //    UserDataKV.username.Value("josh"), UserDataKV.password.Value(GetUserPass()) });
        //StartCoroutine(Send(GetCurrentServerUrl()+ApiUrls.login, sendData, OnResponseLogin));
        Login("REdZU0Q1RHZ5dUhvcTMvQUdoUlROUT09", GetUserPass());
    }
    public void Login(string username, string password)
    {
        //   Debug.Log("U:" + username + "::p:" + password+"::f:"+GetFCMID()+"::m:"+GetUniqueId());
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.fcm_id.Value(GetFCMID()), UserDataKV.mac.Value(GetUniqueId()),
            UserDataKV.user_id.Value(username), UserDataKV.password.Value(password) });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.login, sendData, OnResponseLogin));
    }
    public void Logout()
    {
        if (GetSessionId().Length < 1)
            DebugLog("Cannot Logout without Session ID");
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId())
     });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.logout, sendData, OnResponseLogout));
    }

    //Get list of technicians from the server
    public void GetTechnicians()
   => GetTechnicians(OnResponseTechnicians);

    // Fetch technician profile from server
    public void GetTechnicians(UnityAction<string> returnFn)
    {
        if (GetCurrentServerUrl().Length < 1)
            SetServerUrl(serverType);
        Debug.Log("On " + serverType.ToString() + " Server, getting technicians info from "
            + GetCurrentServerUrl() + ApiUrls.getTechnicians);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
       {
            UserDataKV.api_key,
          //  UserDataKV.session_id.Value(GetSessionId()),
          //  UserDataKV.vin.Value("MEXA15600FT063992")
          UserDataKV.mac.Value(GetUniqueId()),
            //MEXA15600FT063992
            //MEXA15600FT064012
            //MEXA15600FT064124
       });
      //  Debug.Log("MAC: " + GetUniqueId());
   //     StartCoroutine(Send("http://idstaging.savwipl.com/apiget-technicians/", sendData, returnFn));
       StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTechnicians, sendData, returnFn));
    }


    public void GetVehicleMasters(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.language_code.Value(GetLanguageCode())
     });
        if (GetModelCode().Length > 0)
        {
            Debug.Log("Model Code: " + GetModelCode());
            sendData.Add(new KeyValuePair<string, string>("model_code", GetModelCode()));
        }
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicleMasters, sendData, returnFn));
    }

    public void GetVehicleMastersForTorque(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.language_code.Value("en")
     });
        if (GetModelCode().Length > 0)
        {
            Debug.Log("Model Code: " + GetModelCode());
            sendData.Add(new KeyValuePair<string, string>("model_code", GetModelCode()));
        }
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicleMasters, sendData, returnFn));
    }
    private string GetModelCode()
    {
        return modelCode;
    }
    public void SetModelCode(string code)
    {
        Debug.Log("Code      "+code);
        modelCode = code;
    }
    private string GetLanguageCode()
    {
        return languageCode;
    }

    public void GetVehicleDetails()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
       {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vin.Value("MEXA15600FT063992")
            //MEXA15600FT063992
            //MEXA15600FT064012
            //MEXA15600FT064124
       });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicle, sendData, OnResponseVehicleDetails));
    }

    public void GetVehicleDetailsImg(string imgB64, UnityAction<string> responseFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
       {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.image_data.Value(imgB64)
            //MEXA15600FT063992
            //MEXA15600FT064012
            //MEXA15600FT064124
       });
        if (languageCode.Length > 0)
            sendData.Add(UserDataKV.language_code.Value(languageCode));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicleDetails, sendData, (obj) => {
            OnVehicleDetailsRecieved(obj);
            responseFn(obj);
        }));
    }

    //Fetch a specfic vehicle details from the server
    public void GetVehicleDetails(string vin, UnityAction<string> responseFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vin.Value(vin)
         //MEXA15600FT063992
         //MEXA15600FT064012
         //MEXA15600FT064124
     });
        if (languageCode.Length > 0)
            sendData.Add(UserDataKV.language_code.Value(languageCode));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicleDetails, sendData, (obj) => {
            OnVehicleDetailsRecieved(obj);
            responseFn(obj);
        }));
    }

    public void GetVehicleDetailsWVin(string vehicleID,string modelCode,string modelDescription, string engineNumber, UnityAction<string> responseFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(vehicleID),            
             new KeyValuePair<string, string>("model_code",modelCode),
             new KeyValuePair<string, string>("model_descr",modelDescription),
             new KeyValuePair<string, string>("engine_number",engineNumber),
     });        
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getVehicleDetailsWVin, sendData, (obj) => {           
            responseFn(obj);
        }));

    }

    public void CreateTicket(string shortDesc, string kms)
    {
        CreateTicket(shortDesc,kms,  OnResponseCreateTicket);
    }

    //Call a function to create a ticket and send it to server
    public void CreateTicket(string shortDesc, string kmsDone, UnityAction<string> returnFn)
    {
        Debug.Log("Create Ticket for: " + shortDesc);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(GetVehicleId()),
             new KeyValuePair<string, string>("engine_code",GetVehicleEngineCode()),
             new KeyValuePair<string, string>("transmission",GetVehicleGearboxCode()),
             new KeyValuePair<string, string>("model_code",GetVehicleModelCode()),
             
           // UserDataKV.kms_done.Value(kmsDone),
            UserDataKV.short_desc.Value(shortDesc)
   });
            sendData.Add(new KeyValuePair<string, string>("kms_done", kmsDone.ToString()));
        if (GetVehicleEngineNumber().Length > 0)
            sendData.Add(new KeyValuePair<string, string>("engine_number", GetVehicleEngineNumber()));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.createTicket, sendData, returnFn));
    }
    public void GetTickets()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId())
      });
        Debug.Log("Send Data "+sendData.ToString());
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, OnResponseGetTickets));
    }
    public void EscalateCurrentTicket(UnityAction<string> respFn, string inputText = "")
    {
        EscalateTicket(GetTicketId(), respFn, inputText);
        
    }
    public void EscalateTicket(string ticketId, UnityAction<string> respFn, string inputText = "")
    {
        Debug.Log("Escalating Ticket: " + ticketId + " with desc: " + inputText);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(ticketId),
            UserDataKV.short_desc.Value(inputText)
        });
        Debug.Log(UserDataKV.session_id.Value(GetSessionId()));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.escalateTicket, sendData, respFn));

        
    }

    public void CloseTicket(UnityAction<string> respFn, string inputText = "")
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(GetTicketId()),
            UserDataKV.ticket_status.Value("Closed"),
            UserDataKV.short_desc.Value(inputText)
        });
        Debug.Log(UserDataKV.session_id.Value(GetSessionId()));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.updateTicketStatus, sendData, respFn));

        
    }
    /// <summary>
    /// Dhiraj 
    /// 4-6-2024
    /// new method for creating Reassign ticket for IT Tickets
    /// <param name="ticketId"></param>
    /// <param name="respFn"></param>

    public void ReassignCurrentTicket(UnityAction<string> respFn)
    {
        ReassignTicket(GetTicketId(), respFn);
    }
    public void ReassignTicket(string ticketId, UnityAction<string> respFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(ticketId),
            UserDataKV.ticket_status.Value("reassign")
        });
        Debug.Log(UserDataKV.session_id.Value(GetSessionId()));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.reassignTicket, sendData, respFn));
    }

    /// </summary>

    //Get open tickets from API
    public void GetOpenTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            //UserDataKV.ticket_status.Value("Open"),
            UserDataKV.ticket_status.Value("Open Session"),
      });
        Debug.Log(UserDataKV.session_id.Value(GetSessionId()));
        Debug.Log(" Get Tickets 123   "+ GetCurrentServerUrl().ToString() + ApiUrls.getTickets);
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, returnFn));
    }

    //Get close tickets from API
    public void GetClosedTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_status.Value("Closed"),
      });
        Debug.Log(" Get Escalated Tickets New    " + GetCurrentServerUrl().ToString() + ApiUrls.getTickets);
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, returnFn));
    }

    //Get escalated tickets from API
    public void GetEscalatedTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_status.Value("Escalated"),
      });
        Debug.Log(" Get Escalated Tickets New    " + GetCurrentServerUrl().ToString() + ApiUrls.getTickets);
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, returnFn));
    }
    public void GetReassignedTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_status.Value("Reassign"),
      });
        Debug.Log(" Get Escalated Tickets New    " + GetCurrentServerUrl().ToString() + ApiUrls.getTickets);
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, returnFn));
    }
    public void GetResolvedTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_status.Value("Resolved"),
      });
        Debug.Log(" Get Escalated Tickets New    " + GetCurrentServerUrl().ToString() + ApiUrls.getTickets);
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTickets, sendData, returnFn));
    }
    public void GetTranslations(string moduleId, string processType, string textData, string languages, bool updateTranslations, UnityAction<string> responseFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
{
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           new KeyValuePair<string, string>("module_id",moduleId),
           new KeyValuePair<string, string>("process_type",processType),
           new KeyValuePair<string, string>("text",textData),
           new KeyValuePair<string, string>("target_languages",languages),

});
        if (updateTranslations)
            sendData.Add(new KeyValuePair<string, string>("update_cache", "y"));
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.translate, sendData, responseFn));
    }
    public void GetTicketDetails(string ticketId, UnityAction<string> responseFn)
    {

        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId())
  });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTicketDetails, sendData, responseFn));
    }
    public void GetTicketDetails()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId())
  });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getTicketDetails, sendData, OnResponse));
    }

    //Select cluster and sent it to server
    public void SaveTicketClusters(string clusterIds, UnityAction<string> respFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId()),
           new KeyValuePair<string, string>("cluster_ids",clusterIds)
  });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.saveTicketClusters, sendData, respFn));
    }
    public void PauseTicket(string resumeLink) => UpdateTicket(resumeLink, "Paused");
    public void UpdateTicket(string resumeLink)
        => UpdateTicket(resumeLink, "Open Session");
    public void SaveTicketProgress(KeyValuePair<string, string>[] keyValues)
    {       
        string ticketId = GetTicketId();
        if (ticketId.Length > 0) // changing ticketID to 
        {
            sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
            {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(GetVehicleId()),
            UserDataKV.ticket_id.Value(GetTicketId())
                });
            sendData.AddRange(keyValues);
            Debug.Log("Save Ticket Progress"+sendData);
            StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.saveTicketDiagStep, sendData, OnResponseSaveTicket));
        }
        else
            Debug.LogError("No Ticket");
    }

    public void SaveComment(string CMT)
    {
        string ticketId = GetTicketId();
        if (ticketId.Length > 0)
        {
            sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
            {
                UserDataKV.api_key,
                UserDataKV.session_id.Value(GetSessionId()),
                UserDataKV.vehicle_id.Value(GetVehicleId()),
                UserDataKV.ticket_id.Value(GetTicketId()),
                UserDataKV.addon_data.Value(CMT)

            });

            StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.saveTicketDiagStep, sendData, OnResponseSaveComment));

        }
        else
            Debug.LogError("No Ticket");

    }
    
    
    public void CreateSystemTicket(string shortDesc, byte[] img, string ticketType,  UnityAction<string> resp)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_type.Value(ticketType),
            //UserDataKV.ticket_id.Value(GetTicketId()),
             new KeyValuePair<string, string>("short_desc",shortDesc)
     //       new KeyValuePair<string,string>("ticket_files",file)
        });
        Debug.Log("Screenshot size:" + img.Length / 1000 + " kb");
        Debug.LogError(UserDataKV.ticket_id.Value(GetTicketId()) + "TICKET ID");
        Debug.LogError(UserDataKV.ticket_type.Value(ticketType));
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",img)
            };
        StartCoroutine(SendWithBody(GetCurrentServerUrl() + ApiUrls.createSystemTicket, sendData, bodyData, resp));
    }
    public void CreateITTicket(string shortDesc, byte[] img, string ticketType, UnityAction<string> resp)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_type.Value(ticketType),
            //UserDataKV.ticket_id.Value(GetTicketId()),
             new KeyValuePair<string, string>("short_desc",shortDesc)
     //       new KeyValuePair<string,string>("ticket_files",file)
        });
        Debug.Log("Screenshot size:" + img.Length / 1000 + " kb");
        Debug.LogError(UserDataKV.ticket_id.Value(GetTicketId()) + "TICKET ID");
        Debug.LogError(UserDataKV.ticket_type.Value(ticketType));
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",img)
            };
        StartCoroutine(SendWithBody(GetCurrentServerUrl() + ApiUrls.createItTicket, sendData, bodyData, resp));
    }

    public void CreateManTechTicket(string shortDesc, string ticketType,string vehicalId, UnityAction<string> resp)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_type.Value(ticketType),            
            //UserDataKV.ticket_id.Value(GetTicketId()),
            new KeyValuePair<string, string>("vehicle_id",vehicalId),
             new KeyValuePair<string, string>("short_desc",shortDesc)
     //       new KeyValuePair<string,string>("ticket_files",file)
        });
        //Debug.Log("Screenshot size:" + img.Length / 1000 + " kb");
        Debug.LogError(UserDataKV.ticket_id.Value(GetTicketId()) + "TICKET ID");
        Debug.LogError(UserDataKV.ticket_type.Value(ticketType));
/*        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",img)
            };*/
        StartCoroutine(SendWithBody(GetCurrentServerUrl() + ApiUrls.createManTechTicket, sendData, resp));
    }

    public void SaveTicketProgress(string eventT, string processN = null, string loadedModId = null,
    string stepNum = null, string partId = null, string diagnosticTreeId = null, string aun = null, string addonData = null, string fileIds = null)
    {
        Debug.Log("saving Progress for Ticket with ticketId: " + GetTicketId());

        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
           {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(GetVehicleId()),
            UserDataKV.ticket_id.Value(GetTicketId()),
             new KeyValuePair<string, string>("event_type",eventT) });
        if (processN != null)
            sendData.Add(new KeyValuePair<string, string>("process_name", processN));
        if (loadedModId != null)
            sendData.Add(new KeyValuePair<string, string>("loaded_module_ids", loadedModId));
        if (stepNum != null)
            sendData.Add(new KeyValuePair<string, string>("step_number", stepNum));
        if (partId != null)
            sendData.Add(new KeyValuePair<string, string>("part_id", partId));
        if (diagnosticTreeId != null)
            sendData.Add(new KeyValuePair<string, string>("diagnostic_tree_id", diagnosticTreeId));
        if (aun != null)
            sendData.Add(new KeyValuePair<string, string>("activity_unique_number", aun));
        if (addonData != null)
            sendData.Add(new KeyValuePair<string, string>("addon_data", addonData));
        if (fileIds != null)
            sendData.Add(new KeyValuePair<string, string>("file_ids", fileIds));

        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.saveTicketDiagStep, sendData, OnSaveTicketDTStep));
    }
    public void UploadTicketFile(byte[] file, UnityAction<string> responseFn)
    {


        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(GetTicketId())
     //       new KeyValuePair<string,string>("ticket_files",file)
   });
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",file)
            };

        StartCoroutine(SendWithBody(GetCurrentServerUrl() + ApiUrls.uploadTicketFiles, sendData, bodyData, OnResponse));
        void OnResponse(string msg)
        {
            Debug.Log("<color=blue>[UPLOAD-TICKET-FILE]</color>" + msg);
            responseFn(msg);
            //   OnResponseVehicleDetails(msg);
        }
    }

    public void UploadTicketFileforVideo(byte[] file, UnityAction<string> responseFn)
    {


        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(GetTicketId())
     //       new KeyValuePair<string,string>("ticket_files",file)
   });
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",file)
            };

        StartCoroutine(SendWithBodyforVideo(GetCurrentServerUrl() + ApiUrls.uploadTicketFiles, sendData, bodyData, OnResponse));
        void OnResponse(string msg)
        {
            Debug.Log("<color=blue>[UPLOAD-TICKET-FILE]</color>" + msg);
            responseFn(msg);
            //   OnResponseVehicleDetails(msg);
        }
    }

    public void UploadTicketFileforAudio(byte[] file, UnityAction<string> responseFn)
    {


        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_id.Value(GetTicketId())
     //       new KeyValuePair<string,string>("ticket_files",file)
   });
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",file)
            };

        StartCoroutine(SendWithBodyforAudio(GetCurrentServerUrl() + ApiUrls.uploadTicketFiles, sendData, bodyData, OnResponse));
        void OnResponse(string msg)
        {
            Debug.Log("<color=blue>[UPLOAD-TICKET-FILE]</color>" + msg);
            responseFn(msg);
            //   OnResponseVehicleDetails(msg);
        }
    }
    public void SendLogToEmail(string ticketId, UnityAction<string> returnFn, int logSent = 0)
    {
        Debug.Log("Log ID : "+logSent);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            new KeyValuePair<string,string>("ticket_id",ticketId),
            new KeyValuePair<string,string>("logSent",logSent.ToString()),
            new KeyValuePair<string,string>("cus_desc",GetComponent<ScreenLinker>()._escalationRemark)
        });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.sendLogToMail, sendData, returnFn));
    }
    public void GetAllDT()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId())
        });
        StartCoroutine(Send(ApiUrls.getAllDts, sendData, OnResponseAllDTs));
    }

    //fetch data from the server
    public void GetDTIdForTerm(string search, UnityAction<string> returnFn)
    {
        Debug.Log("Search Term"+search);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {   
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            new KeyValuePair<string,string>("search_term",search) 
        });
        
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.getDiagTree, sendData, returnFn));
    }


    public void UpdateTicket(string resumeLink, string state)
    {
        Debug.Log(resumeLink+"       Updating Ticket: " + currentTicket.ShortDescription);
        currentTicket.LongDescription = resumeLink;
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(currentTicket.TicketID),
           UserDataKV.short_desc.Value(currentTicket.ShortDescription),
           UserDataKV.long_desc.Value(resumeLink),
           UserDataKV.ticket_status.Value(state)
  });
        if (isEscalated())
        {
            sendData.Add(UserDataKV.is_escalated.Value(currentTicket.IsEscalatedToL1));
            sendData.Add(UserDataKV.escalation_remark.Value(currentTicket.EscalationRemark));
        }
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.updateTicket, sendData, OnResponseUpdate));
    }
    public void UpdateTicketStatus(string remark,UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId()),
           UserDataKV.ticket_status.Value(GetCurrentTicketStatus()),
          new KeyValuePair<string, string>("remark",remark)
        });
        StartCoroutine(Send(GetCurrentServerUrl() + ApiUrls.updateTicketStatus, sendData, returnFn));
    }
    public bool OnTicket()
    {
        bool result = false;
        if (currentTicket != null)
            result = currentTicket.TicketID != "";
        return result;
    }

    public bool isLoggedIn() => GetSessionId().Length > 1;
    public string GetResponseStatus() => response != null ? response.status : "";
    #region Response Functions

    public static ServerResponse GetServerResponse(string data)
    {
        //Debug.Log(data);
        ServerResponse resp = JsonUtility.FromJson<ServerResponse>(data);
        //    DebugLog("status:" + response.status + " message:" + response.message);
        //    DebugLog("Full Response:" + data);
        return resp;
    }
    public static ServerFileUploadResponse GetFileUploadResponse(string data)
    {
        ServerFileUploadResponse resp = JsonUtility.FromJson<ServerFileUploadResponse>(data);
        return resp;
    }
    public static ServerResponseVehDetails GetServerResponseVehicleDetails(string vehDet)
    {
        ServerResponseVehDetails vehDetResp = JsonUtility.FromJson<ServerResponseVehDetails>(vehDet);
        return vehDetResp;
    }
    //Get all the vehicle details
    public void OnVehicleDetailsRecieved(string detString)
    {
        Debug.Log("[VEHICLE-DETAILS] " + detString);
        /*Checkstatus checkstatus = JsonUtility.FromJson<Checkstatus>(detString);*/
        if (detString.Contains("API URL"))
        {
            FindObjectOfType<UIScreenManager>().Toast("Unable to fetch data");
            FindObjectOfType<UIScreenManager>().SelectScreen(9);
            FindObjectOfType<UIScreenManager>().SetLoadingState(false);
        }
        else
        {
            ServerResponseVehDetails vehDetResp = JsonUtility.FromJson<ServerResponseVehDetails>(detString);
            vehicle_modules = vehDetResp.response_data.vehicle_modules;
            curVehicleDetails = vehDetResp.response_data.vehicle_details;
            bundleAddresses = vehDetResp.response_data.android_asset_bundles;
        }

      
    }
    public bool HasVehicleDetails()
    {
        bool nonNull = curVehicleDetails != null;
        if (nonNull)
            nonNull = curVehicleDetails.VehicleID.Length > 0;

        return nonNull;
    }
    public VehicleDetails GetCurVehicleDetails()
    {
        return curVehicleDetails;
    }
    private void OnSaveTicketDTStep(string arg0)
    {
        Debug.Log("ON-SAVE-TICKET-DT" + arg0);
    }
    private void OnResponseUploadFile(string text)
    {
        throw new NotImplementedException();
    }
    public static ServerResponseVehicleMasters GetServerResponseVehicleMasters(string data)
    {
        ServerResponseVehicleMasters resp = JsonUtility.FromJson<ServerResponseVehicleMasters>(data);
        return resp;
    }
    private ServerResponse GetResponse(string data)
    {
        ServerResponse resp = JsonUtility.FromJson<ServerResponse>(data);
        response = resp;
        //    DebugLog("status:" + response.status + " message:" + response.message);
        //    DebugLog("Full Response:" + data);
        return resp;
    }
    private void OnResponseUpdate(string data)
    {
        response = GetResponse(data);
        Debug.Log("UPDATE RESPONSE: " + response.status);
    }
    private void OnResponse(string data)
    {
        DebugLog("Response :" + data);
        response = JsonUtility.FromJson<ServerResponse>(data);
    }
    private void OnResponseVehicleDetails(string data)
    {
        DebugLog("<color=blue>On Vehicle Details Response:</color>" + data);

        serverData.vehicle_details = GetResponse(data).response_data.vehicle_details;
        OnRecieveVehicleDetails.Invoke(serverData);
    }
    private void OnResponseVehicleDetailsTranslate(string data)
    {
        DebugLog("<color=blue>On Vehicle Details Response:</color>" + data);

        serverData.vehicle_details = GetResponse(data).response_data.vehicle_details;
        OnRecieveVehicleDetails.Invoke(serverData);
    }
    private void OnResponseTechnicians(string data)
    {
        DebugLog("Technicians Response :" + data);
        serverData.technicians = GetResponse(data).response_data.technicians;
        OnRecieveTechnicians?.Invoke(serverData);
    }
    private void OnResponseAllDTs(string data)
    {
        Debug.Log(data);
    }
  
    private void OnResponseGetDT(string data)
    {
        serverData.diagnostic_trees = GetResponse(data).response_data.diagnostic_trees;
        //TextAsset DTText = new TextAsset(serverData.diagnostic_trees[0].DT_JSON);

       /* for (int i = 0; i < serverData.diagnostic_trees.Length; i++)
        {
         


        }*/

            //Debug.Log("Got DT ID: " + data);
            Debug.Log("Issue Tiltle:  "+serverData.diagnostic_trees[0].IssueTitle);
    }
    private void OnResponseSaveComment(string data)
    {
        Debug.Log("Saved Comment!: " + data);
        //  serverData.tickets = GetResponse(data).response_data.tickets;
    }

    //Save ticket from DT and RnR
    private void OnResponseSaveTicket(string data)
    {
        Debug.Log("Saved Ticket!: " + data);
        serverData.tickets = GetResponse(data).response_data.tickets;
        DebugLog("Got Tickets Response :" + serverData.tickets.Count);
    }
    private void OnResponseOpenTickets(string data)
    {
        Debug.Log("Open Tickets:" + data);
    }
    private void OnResponseGetTickets(string data)
    {
        serverData.tickets = GetResponse(data).response_data.tickets;
        DebugLog("Got Tickets Response :" + serverData.tickets.Count);
    }
    public void OnCurrentTicketEscalated(string resp)
    {
        if (GetServerResponse(resp).status.ToLower().Contains("success"))
            ResetTicket();
    }
    public void ResetTicket()
    {
        Debug.Log("Reset Ticket");
      
        currentTicket = new Ticket();
        curVehicleDetails = new VehicleDetails();
    }
    private void OnResponseCreateTicket(string data)
    {
        DebugLog("Got Create Ticket Response:" + data);

        serverData.ticket_id = GetResponse(data).response_data.ticket_id;

        serverData.ticket_uid = GetResponse(data).response_data.ticket_uid;
       //Debug.LogError( serverData.ticket_uid = GetResponse(data).response_data.ticket_uid + "TICKET UID") ;
        //   currentTicket.TicketID = GetResponse(data).response_data.ticket_id;
        string msg = GetResponse(data).response_data.message;
        if (msg.Length > 0)
            LogToScreen(serverData.message);
        currentTicket.TicketID = serverData.ticket_id;
    }

    //Get the data of logged in profile
    private void OnResponseLogin(string data)
    {
        DebugLog("Login Response:" + data);
        if (data.Length > 0)
        {
            Save_Log.FirstName = GetResponse(data).response_data.profile_info.FirstName;
            Save_Log.LastName= GetResponse(data).response_data.profile_info.LastName;
            Save_Log.Email= GetResponse(data).response_data.profile_info.EmailID;
            serverData.profile_info = GetResponse(data).response_data.profile_info;
            serverData.session_id = GetResponse(data).response_data.session_id;
            serverData.message = GetResponse(data).response_data.message;
            OnLogin(response.status + ":" + serverData.message);// + ":" + response.message.ToString());

            serverData.brand = GetResponse(data).response_data.brand;
        }
        else
            DebugLog("No Response from server!");
    }


    public VehicleCluster[] GetClusters()
    {
        VehicleCluster[] result = null;
        if (clusters != null)
            if (clusters.Length > 0)
                result = clusters;
        return result;

    }

    public string GetNameForModule(int id)
    {
        if (id < vehicle_modules.Length)
            return vehicle_modules[id].ModuleName;
        else
        {
            Debug.LogError("No module found At: " + id);
            return "";
        }
    }
    public void SetClusters(VehicleCluster[] _clusters)
    {
        clusters = _clusters;
    }
    private void OnResponseLogout(string data)
    {
        DebugLog("Logout Success:" + GetResponse(data).message);
        serverData = null;
        currentTicket = null;
    }
    #endregion
    #endregion
    public List<Ticket> TicketList() => serverData.tickets;
    #region DataTransfer
    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        DebugLog("Status Code: " + request.responseCode);
    }

    //common function to call an API 
    IEnumerator Send(string url, List<KeyValuePair<string, string>> data, UnityAction<string> apiCall, UnityAction<string> onError = null)
    {
         DebugLog("Opening Link:" + url);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            else

                formData.Add(new MultipartFormDataSection(k.Key, k.Value));
                Debug.Log(k.Key + " ==> " + k.Value);

        }
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(www.error);
            onReqError?.Invoke(www.error);
            DebugLog(www.error + " for link: " + url);
            if (errorRequests == null)
                errorRequests = new List<UnityWebRequest>();
            errorRequests.Add(www);
            DebugLog("Total Error Requests:" + errorRequests.Count);
        }
        else
        {
            DebugLog("Recieved Response");
            //Debug.LogWarning("Form upload complete!----> "+www.downloadHandler.text);
            apiCall(www.downloadHandler.text);

            //   apiCall.Invoke(www.downloadHandler.text);
        }
    }

    IEnumerator SendWithBody(string url, List<KeyValuePair<string, string>> data, UnityAction<string> apiCall)
    {
        Debug.Log("URL  =" + url);
        //   DebugLog("Opening Link:" + url);
        WWWForm form = new WWWForm();
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            //  if(k.)
            formData.Add(new MultipartFormDataSection(k.Key, k.Value));
        }


        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
            onReqError?.Invoke(www.error);
            if (errorRequests == null)
                errorRequests = new List<UnityWebRequest>();
            errorRequests.Add(www);
            DebugLog("Total Error Requests:" + errorRequests.Count);
        }
        else
        {
            DebugLog("Recieved Response");
            //  DebugLog("Form upload complete!"+www.downloadHandler.text);
            apiCall(www.downloadHandler.text);

            //   apiCall.Invoke(www.downloadHandler.text);
        }
    }
    IEnumerator SendWithBody(string url, List<KeyValuePair<string, string>> data, List<KeyValuePair<string, byte[]>> body, UnityAction<string> apiCall)
    {
        Debug.Log("URL  ="+url);
        //   DebugLog("Opening Link:" + url);
        WWWForm form = new WWWForm();
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            //  if(k.)
            formData.Add(new MultipartFormDataSection(k.Key, k.Value));
        }
        foreach (var item in body)
        {
            if (item.Key.Length > 0)
            {

                //       formData.Add(new MultipartFormDataSection(item.Key, item.Value));
                formData.Add(new MultipartFormFileSection("ticket_files[]", item.Value, "test.jpeg", "image/jpeg"));

            }
        }
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
            onReqError?.Invoke(www.error);
            if (errorRequests == null)
                errorRequests = new List<UnityWebRequest>();
            errorRequests.Add(www);
            DebugLog("Total Error Requests:" + errorRequests.Count);
        }
        else
        {
            DebugLog("Recieved Response");
            //  DebugLog("Form upload complete!"+www.downloadHandler.text);
            apiCall(www.downloadHandler.text);

            //   apiCall.Invoke(www.downloadHandler.text);
        }
    }

    IEnumerator SendWithBodyforVideo(string url, List<KeyValuePair<string, string>> data, List<KeyValuePair<string, byte[]>> body, UnityAction<string> apiCall)
    {
        //   DebugLog("Opening Link:" + url);
        WWWForm form = new WWWForm();
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            //  if(k.)
            formData.Add(new MultipartFormDataSection(k.Key, k.Value));
        }
        foreach (var item in body)
        {
            if (item.Key.Length > 0)
            {

                //       formData.Add(new MultipartFormDataSection(item.Key, item.Value));
                formData.Add(new MultipartFormFileSection("ticket_files[]", item.Value, "test.mp4", "video/mp4"));

            }
        }
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
            onReqError?.Invoke(www.error);
            if (errorRequests == null)
                errorRequests = new List<UnityWebRequest>();
            errorRequests.Add(www);
            DebugLog("Total Error Requests:" + errorRequests.Count);
        }
        else
        {
            DebugLog("Recieved Response");
            //  DebugLog("Form upload complete!"+www.downloadHandler.text);
            apiCall(www.downloadHandler.text);

            //   apiCall.Invoke(www.downloadHandler.text);
        }
    }

    IEnumerator SendWithBodyforAudio(string url, List<KeyValuePair<string, string>> data, List<KeyValuePair<string, byte[]>> body, UnityAction<string> apiCall)
    {
        //   DebugLog("Opening Link:" + url);
        WWWForm form = new WWWForm();
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            //  if(k.)
            formData.Add(new MultipartFormDataSection(k.Key, k.Value));
        }
        foreach (var item in body)
        {
            if (item.Key.Length > 0)
            {

                //       formData.Add(new MultipartFormDataSection(item.Key, item.Value));
                formData.Add(new MultipartFormFileSection("ticket_files[]", item.Value, "test.wav", "audio/wav"));

            }
        }
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
            onReqError?.Invoke(www.error);
            if (errorRequests == null)
                errorRequests = new List<UnityWebRequest>();
            errorRequests.Add(www);
            DebugLog("Total Error Requests:" + errorRequests.Count);
        }
        else
        {
            DebugLog("Recieved Response");
            //  DebugLog("Form upload complete!"+www.downloadHandler.text);
            apiCall(www.downloadHandler.text);

            //   apiCall.Invoke(www.downloadHandler.text);
        }
    }
    IEnumerator TestLogin()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post("https://www.my-server.com/myform", formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
    #endregion
    #region Getters
    private string GetFCMID()
    {
        return "fcm9999";
    }
    private bool isEscalated()
    {
        bool result = false;
        if (currentTicket != null)
        {
            if (currentTicket.IsEscalatedToL1.Length > 0)
                result = currentTicket.IsEscalatedToL1.Contains("Y");
            else
                Debug.Log("Is Escalated is:" + currentTicket.IsEscalatedToL1);
        }
        else
            Debug.LogError("Current Ticket is Null");


        return result;
    }

    //   => false;
    public string GetUniqueId()
    {
        if (Application.isEditor)
        {
            //  return SystemInfo.deviceUniqueIdentifier;
            return editorId;
            //return "9999999";
        }
        else
            return SystemInfo.deviceUniqueIdentifier;
    }
    
    private static string GetMacAddress()
    {
        string physicalAddress = "";

        NetworkInterface[] nice = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adaper in nice)
        {

            Debug.Log(adaper.Description);

            if (adaper.Description == "en0")
            {
                physicalAddress = adaper.GetPhysicalAddress().ToString();
                break;
            }
            else
            {
                physicalAddress = adaper.GetPhysicalAddress().ToString();

                if (physicalAddress != "")
                {
                    break;
                };
            }
        }

        return physicalAddress;
    }
    public string GetUserName()
    {
        return "josh";
    }
    private string GetUserPass()
    {
        return "ComXR#";
    }
    private string GetSessionId()
    {
        if (serverData.session_id == null)
            Debug.Log("SVRDATA IS NULL");
        return serverData.session_id != null ? serverData.session_id : "";
    }
    private string GetCurrentTicketStatus()
    {
        return "Open ";
    }
    private string GetShortDesc()
    {
        return "test short description";
    }
    private string GetTicketStatus()
    {
        return currentTicket.TicketStatus;
    }
    public bool HasActiveTicket()
    {
        bool result = false;
        if (currentTicket.TicketID != null)
            if (currentTicket.TicketID.Length > 0)
                result = true;
        return result;
    }
    private string GetTicketId()
    {
        if (currentTicket.TicketID != null)
        {
            Debug.LogError(currentTicket.TicketID + " Current Ticket to escalte");
            ticketToEscalate = currentTicket.TicketID;
            return currentTicket.TicketID;
        }
        else
            return "";
    }

    private string GetVehicleId()
    {
        //     return serverData.vehicle_details.VehicleID;
        string result = "";
        if (currentTicket.VehicleID != null)
        {
            if (currentTicket.VehicleID.Length > 0)
            {
                result = currentTicket.VehicleID;
            }
        }
        if (result.Length < 1)
        {
            if (curVehicleDetails.VehicleID != null)
                if (curVehicleDetails.VehicleID.Length > 0)
                    result = curVehicleDetails.VehicleID;
        }
        if (result.Length < 1)
            Debug.LogError("No Vehicle ID found!");
        return result;

    }
    private string GetVehicleEngineCode()
    {
        return curVehicleDetails.Engine;
    }
    private string GetVehicleGearboxCode()
    {
        return curVehicleDetails.Transmission;
    }

    private string GetVehicleEngineNumber()
    {
        if (curVehicleDetails.EngineNumber.Length > 0)
            return curVehicleDetails.EngineNumber;
        else
        {
            Debug.LogError("No engine number found!");
            return "";

        }
    }
    private string GetVehicleModelCode()
    {
        return curVehicleDetails.ModelCode;
    }
    public ProfileInfo GetProfileInfo() => serverData.profile_info;

    #endregion
    public void SetCurrentVehicleDetails(VehicleDetails vd)
    {
        Debug.Log("Setting Vehicle Details");
        curVehicleDetails = vd;
    }
    public void SetLanguage(string newLanguageCode)
    {
        languageCode = newLanguageCode;
    }
    #region SaveData
    public void SaveData()
    {
        string data = JsonUtility.ToJson(serverData);
        PlayerPrefs.SetString("save", data);
    }
    public bool HasSavedData() => PlayerPrefs.HasKey("save");
    public void LoadSaveData()
    {
        if (HasSavedData())
        {
            string data = PlayerPrefs.GetString("save");
            serverData = JsonUtility.FromJson<ServerData>(data);
            DebugLog("Saved");
        }
    }
    #endregion
    void LogToScreen(string msg)
    {
        OnLogToScreen?.Invoke(msg);
        DebugLog(msg);
    }
    void DebugLog(string msg)
    {
        if (enableDebug)
            Debug.Log(msg);
    }
}
#region EDITOR
#if UNITY_EDITOR
[CustomEditor(typeof(AppApiManager))]
public class AppApiManagerEditor : Editor
{
    AppApiManager.ServerType type;
    int lastType = -100;
    public override void OnInspectorGUI()
    {
        if (EditorApplication.isPlaying)
        {
            if (AppApiManager.EDITOR_MODE) UpdateInEditorMode();
            DrawDefaultInspector();
        }
        else
        {

        }

    }

    void UpdateInEditorMode()
    {
        AppApiManager script = (AppApiManager)target;
        EditorGUILayout.LabelField("Server: ", script.GetServerType().ToString());
        if (EditorApplication.isPlaying)
        {
            //  Debug.Log("ISLOGGEDIN: " + script.isLoggedIn());
            bool loggedin = script.isLoggedIn();
            string status = script.GetResponseStatus();

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Get Technicians"))
                script.GetTechnicians();
            if (status.Length > 0)
                EditorGUILayout.HelpBox("status:" + script.GetResponseStatus(), MessageType.Info);

            EditorGUILayout.EndVertical();
        }
    }
}
#endif
#endregion
