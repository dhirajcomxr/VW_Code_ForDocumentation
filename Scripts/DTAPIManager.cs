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

public class DTAPIManager : MonoBehaviour
{
    public GameObject dtListObjectPrefab;
    public Transform dtListTransform;
    public TextAsset asset;
    public GameObject treeStepCreator;
    public GameObject dtListWindow;
    public Ticket currentTicket;
    public static bool EDITOR_MODE = true;
    VehicleCluster[] clusters;
    [SerializeField] bool enableDebug = true;
    [SerializeField]
    ServerResponse response;
    [SerializeField] ServerData serverData;
    [SerializeField] VehicleDetails curVehicleDetails;
    // [SerializeField]
    List<KeyValuePair<string, string>> sendData;
    // [SerializeField]
    List<UnityWebRequest> errorRequests;
    bool useForm = false;

    string textFile;

    void Start()
    {
        //Debug.Log(asset.ToString());
        textFile = asset.ToString();
        Invoke("Login", 0.5f);
        //Invoke("CallSaveDT", 3f);

    }

    void CallSaveDT()
    {
        SaveDT("UserID", "Battery Issue - Engine", "This is about battery", "battery", textFile, "", "1.11", "a");
    }
    #region Data Classes


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
    public event ApiCallStatus OnApiResponse, OnLogin, OnVehicleDetails;
    public UnityAction<string> OnLogToScreen;
    public List<Ticket> openTickets;

    public static string currentSession;
    // Ticket currentTicket;
    static class ApiUrls
    {
        public static string
             saveFcm = "https://Compeny.com/api//save-fcm/",
            login = "https://Compeny.com/api//login/",
            logout = "https://Compeny.com/api//logout/",
            forgotPassword = "https://Compeny.com/api//forgot-password/",

            getTechnicians = "https://Compeny.com/api//get-technicians/",
            getVehicleMasters = "https://Compeny.com/api//get-vehicle-masters/",
            saveTicketClusters = "https://Compeny.com/api//save-ticket-clusters/",
            getVehicle = "https://Compeny.com/api//get-vehicle/",
            getVehicleDetails = "https://Compeny.com/api//get-vehicle-details/",
            createTicket = "https://Compeny.com/api//create-ticket/",
            getTicketDetails = "https://Compeny.com/api//get-ticket-details/",
            getTickets = "https://Compeny.com/api//get-tickets/",
            saveTicketDiagStep = "https://Compeny.com/api//save-ticket-diag-step/",
            getDiagTree = "https://Compeny.com/api//get-diagnostic-trees/",
            uploadTicketFiles = "https://Compeny.com/api//upload-ticket-files/",
            sendLogToMail = "https://Compeny.com/api//email-ticket-log/",
            createSystemTicket = "https://Compeny.com/api//create-system-ticket/",
            escalateTicket = "https://Compeny.com/api//escalate-ticket/",
            updateTicket = "https://Compeny.com/api//update-ticket/",
            getAllDts = "https://Compeny.com/api//get-all-dts/",
            saveDt = "https://Compeny.com/api//save-dt/";
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

            api_key = new KeyValuePair<string, string>("api_key", "TestAPIKey"),
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
            email = new KeyValuePair<string, string>("email", ""),                         //Email ID for Forgot Password
            ticket_id = new KeyValuePair<string, string>("ticket_id", ""),                  //Ticket ID
            ticket_status = new KeyValuePair<string, string>("ticket_status", ""),         //Open / Closed / Paused
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
        limit = new KeyValuePair<string, string>("limit", "");                         //Get-Tickets :Default is 10



    }

    #endregion
    #region Server Response Classes
    [System.Serializable]
    public class Diagnostic_Trees
    {
        public string DiagnosticTreeID; // { get; set; }
        public string IssueTitle; // { get; set; }
        public string IssueDescription; // { get; set; }
        public string DTFVersion; // { get; set; }
        public string DTStatus; // { get; set; }   

        public string DT_JSON;
    }
    
    [System.Serializable]
    public class Ticket
    {
        public string ShortDescription;
        public string TicketID;
        public string VehicleID;
        public string LongDescription;
        public string TicketType;
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
    public class Technician
    {
        public string UserID, FirstName, LastName, ProfilePicture;
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
        public ModuleSections modules;
        [System.Serializable]
        public class ModuleSections
        {
            public ServerVehicleModule[] engine, suspension,
                            transmission, hvac, body, brakes;

            [FormerlySerializedAs("1.5l engine wiring harness")]
            public ServerVehicleModule[] wiringharness1_5l;
            [FormerlySerializedAs("1l engine wiring harness")]
            public ServerVehicleModule[] wiringharness1l;
            [FormerlySerializedAs("central wiring harness")]
            public ServerVehicleModule[] centralwiringharness;
        }
    }
    #endregion
    #region API Functions
    public void Login()
    {
        //sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        //{
        //    UserDataKV.api_key,
        //    UserDataKV.fcm_id.Value(GetFCMID()), UserDataKV.mac.Value(GetUniqueId()),
        //    UserDataKV.username.Value("josh"), UserDataKV.password.Value(GetUserPass()) });
        //StartCoroutine(Send(ApiUrls.login, sendData, OnResponseLogin));
        Login("MGx6WC8rMUMySG4vYXl0NDRZaEJsZz09", GetUserPass());
    }
    public void Login(string username, string password)
    {
        //   Debug.Log("U:" + username + "::p:" + password+"::f:"+GetFCMID()+"::m:"+GetUniqueId());
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.fcm_id.Value(GetFCMID()), UserDataKV.mac.Value(GetUniqueId()),
            UserDataKV.user_id.Value(username), UserDataKV.password.Value(password) });
        StartCoroutine(Send(ApiUrls.login, sendData, OnResponseLogin));

    }

    public void GetAllDT()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(currentSession) 
        });
        StartCoroutine(Send(ApiUrls.getAllDts, sendData, OnResponseAllDTs));
    }

    public void SaveDT(string dtId, string issueTitle, string issueDesc, string dtTags, string dtJSONFile, string dtFiles, string dtFileVersion, string dtStatus)
    {
        //Debug.Log("saving Progress for Ticket with ticketId: " + GetTicketId());

        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(currentSession),
             new KeyValuePair<string, string>("dt_id",dtId) });
            sendData.Add(new KeyValuePair<string, string>("issue_title", issueTitle));
            sendData.Add(new KeyValuePair<string, string>("issue_desc", issueDesc));
            sendData.Add(new KeyValuePair<string, string>("dt_tags", dtTags));
            //sendData.Add(new KeyValuePair<string, string>("dt_json_file", dtJSONFile));
            //sendData.Add(new KeyValuePair<string, string>("dt_files", dtFiles));
            sendData.Add(new KeyValuePair<string, string>("dt_file_version", dtFileVersion));
            sendData.Add(new KeyValuePair<string, string>("dt_status", dtStatus));

        var bytes = Encoding.UTF8.GetBytes(textFile);
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("dt_files",bytes)
            };

        List<KeyValuePair<string, byte[]>> jsonData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("dt_json_file",asset.bytes)
            };

        //StartCoroutine(Send(ApiUrls.saveDt, sendData, OnSaveDT));
        StartCoroutine(SendWithBodyForDT(ApiUrls.saveDt, sendData, bodyData, jsonData, OnSaveDT));
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
        StartCoroutine(Send(ApiUrls.logout, sendData, OnResponseLogout));
    }
    public void GetTechnicians()
    {
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
        StartCoroutine(Send(ApiUrls.getTechnicians, sendData, OnResponseTechnicians));
    }
    public void GetVehicleMasters(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
     {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
     });
        StartCoroutine(Send(ApiUrls.getVehicleMasters, sendData, returnFn));
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
        StartCoroutine(Send(ApiUrls.getVehicle, sendData, OnResponseVehicleDetails));
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
        StartCoroutine(Send(ApiUrls.getVehicleDetails, sendData, responseFn));
    }
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
        StartCoroutine(Send(ApiUrls.getVehicleDetails, sendData, responseFn));
    }
    public void CreateTicket(string shortDesc)
    {
        CreateTicket(shortDesc, OnResponseCreateTicket);
    }
    public void CreateTicket(string shortDesc, UnityAction<string> returnFn)
    {
        Debug.Log("Create Ticket for: " + shortDesc);
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(GetVehicleId()),
             new KeyValuePair<string, string>("engine_number",GetVehicleEngineNumber()),
             new KeyValuePair<string, string>("engine_code",GetVehicleEngineCode()),
             new KeyValuePair<string, string>("transmission",GetVehicleGearboxCode()),
             new KeyValuePair<string, string>("model_code",GetVehicleModelCode()),
            UserDataKV.short_desc.Value(shortDesc)
   });
        StartCoroutine(Send(ApiUrls.createTicket, sendData, returnFn));
    }
    public void GetTickets()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId())
      });
        StartCoroutine(Send(ApiUrls.getTickets, sendData, OnResponseGetTickets));
    }
    public void GetOpenTickets(UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
      {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.ticket_status.Value("Open"),
      });
        StartCoroutine(Send(ApiUrls.getTickets, sendData, returnFn));
    }

    public void GetTicketDetails(string ticketId, UnityAction<string> responseFn)
    {

        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId())
  });
        StartCoroutine(Send(ApiUrls.getTicketDetails, sendData, responseFn));
    }
    public void GetTicketDetails()
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId())
  });
        StartCoroutine(Send(ApiUrls.getTicketDetails, sendData, OnResponse));
    }
    public void SaveTicketClusters(string clusterIds, UnityAction<string> respFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(GetTicketId()),
           new KeyValuePair<string, string>("cluster_ids",clusterIds)
  });
        StartCoroutine(Send(ApiUrls.saveTicketClusters, sendData, respFn));
    }
    public void EscalateCurrentTicket(UnityAction<string> respFn)
        => EscalateTicket(GetTicketId(), respFn);
    public void EscalateTicket(string ticketId, UnityAction<string> respFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
  {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
           UserDataKV.ticket_id.Value(ticketId),
  });
        StartCoroutine(Send(ApiUrls.escalateTicket, sendData, respFn));
    }
    public void UpdateTicket(string resumeLink)
        => UpdateTicket(resumeLink, "Open Session");
    public void SaveTicketProgress(KeyValuePair<string, string>[] keyValues)
    {
        string ticketId = GetTicketId();
        if (ticketId.Length > 0)
        {
            sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
    {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            UserDataKV.vehicle_id.Value(GetVehicleId()),
            UserDataKV.ticket_id.Value(GetTicketId())
                });
            sendData.AddRange(keyValues);
            StartCoroutine(Send(ApiUrls.saveTicketDiagStep, sendData, OnResponseSaveTicket));
        }
        else
            Debug.LogError("No Ticket");
    }
    public void CreateSystemTicket(string shortDesc, byte[] img, UnityAction<string> resp)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
 {

            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
             new KeyValuePair<string, string>("short_desc",shortDesc)
     //       new KeyValuePair<string,string>("ticket_files",file)
 });
        Debug.Log("Screenshot size:" + img.Length / 1000 + " kb");
        List<KeyValuePair<string, byte[]>> bodyData = new List<KeyValuePair<string, byte[]>>{
            new KeyValuePair<string, byte[]>("ticket_files",img)
            };
        StartCoroutine(SendWithBody(ApiUrls.createSystemTicket, sendData, bodyData, resp));
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

        StartCoroutine(Send(ApiUrls.saveTicketDiagStep, sendData, OnSaveTicketDTStep));
    }

    private void OnSaveTicketDTStep(string arg0)
    {
        Debug.Log("ON-SAVE-TICKET-DT" + arg0);
    }

    private void OnSaveDT(string arg0)
    {
        Debug.Log("ON-SAVE-DT" + arg0);
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

        StartCoroutine(SendWithBody(ApiUrls.uploadTicketFiles, sendData, bodyData, OnResponse));
        void OnResponse(string msg)
        {
            Debug.Log("<color=blue>[UPLOAD-TICKET-FILE]</color>" + msg);
            responseFn(msg);
            //   OnResponseVehicleDetails(msg);
        }
    }

    private void OnResponseUploadFile(string text)
    {
        throw new NotImplementedException();
    }

    public void SendLogToEmail(string ticketId, UnityAction<string> returnFn)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
   {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(GetSessionId()),
            new KeyValuePair<string,string>("ticket_id",ticketId)
   });
        StartCoroutine(Send(ApiUrls.sendLogToMail, sendData, returnFn));
    }
    public void GetDTIdForTerm(string search)
    {
        sendData = new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[]
        {
            UserDataKV.api_key,
            UserDataKV.session_id.Value(currentSession),
            new KeyValuePair<string,string>("dt_id",search)
        });
        StartCoroutine(Send(ApiUrls.getDiagTree, sendData, OnResponseGetDT));
    }

   

    public void UpdateTicket(string resumeLink, string state)
    {
        Debug.Log("Updating Ticket: " + currentTicket.ShortDescription);
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
        StartCoroutine(Send(ApiUrls.updateTicket, sendData, OnResponseUpdate));
    }
    public bool OnTicket() => currentTicket.TicketID != "";
    public bool isLoggedIn() => GetSessionId().Length > 1;
    public string GetResponseStatus() => response != null ? response.status : "";

    #region Response Functions
    public static ServerResponse GetServerResponse(string data)
    {
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

    public static ServerResponseVehicleMasters GetServerResponseVehicleMasters(string data)
    {
        ServerResponseVehicleMasters resp = JsonUtility.FromJson<ServerResponseVehicleMasters>(data);
        //    DebugLog("status:" + response.status + " message:" + response.message);
        //    DebugLog("Full Response:" + data);
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

    private void OnResponseTechnicians(string data)
    {
        DebugLog("Technicians Response :" + data);
        serverData.technicians = GetResponse(data).response_data.technicians;
        OnRecieveTechnicians.Invoke(serverData);
    }
    private void OnResponseGetDT(string data)
    {
        Debug.Log("Got DT ID: " + data);

        serverData.diagnostic_trees = GetResponse(data).response_data.diagnostic_trees;

        string currentDT = serverData.diagnostic_trees[0].DT_JSON;
       // treeStepCreator.GetComponent<TestTreeStepCreator>().GenerateTreeFromText(currentDT);
        dtListWindow.SetActive(false);
    }

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
    private void OnResponseCreateTicket(string data)
    {
        DebugLog("Got Create Ticket Response:" + data);

        serverData.ticket_id = GetResponse(data).response_data.ticket_id;

        //   currentTicket.TicketID = GetResponse(data).response_data.ticket_id;
        string msg = GetResponse(data).response_data.message;
        if (msg.Length > 0)
            LogToScreen(serverData.message);
        currentTicket.TicketID = serverData.ticket_id;
    }
    private void OnResponseLogin(string data)
    {
        DebugLog("Login Response:" + data);

        serverData.profile_info = GetResponse(data).response_data.profile_info;
        serverData.session_id = GetResponse(data).response_data.session_id;
        serverData.message = GetResponse(data).response_data.message;
        currentSession = serverData.session_id;
        //Debug.Log("Current session id : " + currentSession);
        //Debug.Log(serverData.session_id);
        //OnLogin(response.status + ":" + serverData.message);// + ":" + response.message.ToString());
    }

    private void OnResponseAllDTs(string data)
    {
        //DebugLog("DTs: " + data);
        serverData.diagnostic_trees = GetResponse(data).response_data.diagnostic_trees;
        TextAsset DTText = new TextAsset(serverData.diagnostic_trees[0].DT_JSON);
         

        for(int i = 0; i < serverData.diagnostic_trees.Length; i++)
        {
            GameObject dtListObject;
            dtListObject = Instantiate(dtListObjectPrefab);
            dtListObject.transform.SetParent(dtListTransform);
            dtListObject.transform.localScale = Vector3.one;
            dtListObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = serverData.diagnostic_trees[i].IssueTitle;
            string problemStatement = serverData.diagnostic_trees[i].DiagnosticTreeID;
            dtListObject.GetComponent<Button>().onClick.AddListener(() => GetDTIdForTerm(problemStatement));
        }

        Debug.Log(serverData.diagnostic_trees[0].IssueTitle);
    }
    public VehicleCluster[] GetClusters()
    {
        VehicleCluster[] result = null;
        if (clusters != null)
            if (clusters.Length > 0)
                result = clusters;
        return result;

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
  
    // Update is called once per frame
    IEnumerator Send(string url, List<KeyValuePair<string, string>> data, UnityAction<string> apiCall)
    {
        Debug.Log("URL : " + url);
        //   DebugLog("Opening Link:" + url);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        foreach (KeyValuePair<string, string> k in data)
        {
            if (k.Value == "")
                Debug.LogError("value for " + k.Key + " missing");
            else

                formData.Add(new MultipartFormDataSection(k.Key, k.Value));

        }
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
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

    IEnumerator SendWithBodyForDT(string url, List<KeyValuePair<string, string>> data, List<KeyValuePair<string, byte[]>> body, List<KeyValuePair<string, byte[]>> jsonFile, UnityAction<string> apiCall)
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
        foreach (var item in jsonFile)
        {
            if (item.Key.Length > 0)
            {
                Debug.Log(item.Value.Length);
                //       formData.Add(new MultipartFormDataSection(item.Key, item.Value));
                formData.Add(new MultipartFormFileSection("dt_json_file", item.Value, "test.txt", "text/plain"));
                

            }
        }
        foreach (var item in body)
        {
            if (item.Key.Length > 0)
            {

                //       formData.Add(new MultipartFormDataSection(item.Key, item.Value));
                formData.Add(new MultipartFormFileSection("dt_files", item.Value, "test.jpeg", "image/jpeg"));

            }
        }
        
        //   formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //  formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            DebugLog(www.error);
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
    private string GetUniqueId()
    {
        //  return SystemInfo.deviceUniqueIdentifier;
        return "mac9999";
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
        return "1";
    }
    private string GetSessionId()
    {
        if (serverData.session_id == null)
            Debug.Log("SVRDATA IS NULL");
        return serverData.session_id != null ? serverData.session_id : "";
    }
    private string GetCurrentTicketStatus()
    {
        return "Open";
    }
    private string GetShortDesc()
    {
        return "test short description";
    }
    private string GetTicketId()
    {
        return currentTicket.TicketID;
    }
    private string GetVehicleId()
    {
        //     return serverData.vehicle_details.VehicleID;
        return curVehicleDetails.VehicleID;
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
        return curVehicleDetails.EngineNumber;
    }
    private string GetVehicleModelCode()
    {
        return curVehicleDetails.ModelCode;
    }
    public ProfileInfo GetProfileInfo() => serverData.profile_info;

    #endregion
    public void SetCurrentVehicleDetails(VehicleDetails vd)
    {
        curVehicleDetails = vd;
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

