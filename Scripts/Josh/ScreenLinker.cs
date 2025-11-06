using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;
using mixpanel;
#if UNITY_EDITOR
using UnityEditor;
#endif
public partial class ScreenLinker : MonoBehaviour
{
    [SerializeField] GameObject sendDetailsToMail;
    [SerializeField] bool enableDebug = false;
    [SerializeField] public UIScreenManager screenManager;
    [SerializeField] DiagnosticTreeManager dtManager;
    [SerializeField] PartIdToStepFunction partIdManager;
    [SerializeField] Steps stepsSection;
    [SerializeField] VWModuleManager moduleManager;
    [SerializeField] AppApiManager apiManager;
    [SerializeField] VWReferencesManager referencesManager;
    [SerializeField] AssetLoader assetLoader;
    [SerializeField] StepsEximProcessor stepsExim;
    [SerializeField] AutoModuleSelectHelper autoModuleSelector;
    [SerializeField] LocalizationSupport localizationSupport;
    [SerializeField] ManualModuleSelectHelper manualSelector;
    [Header("UI References")]
    [SerializeField] ProfileScreen profile;
    [SerializeField] TicketScreen ticketScreen;
    [SerializeField] TicketDetailsScreen ticketDetails;

    [SerializeField] UiVehicleDetails vehicleDetailsScreen;
    [SerializeField] TicketCreator ticketCreator;
    [SerializeField] ModuleLoadManager moduleLoader;
    [SerializeField] TechnicianLogin loginManager;
    [SerializeField] StartScreen3dView processSelection;
    [SerializeField] ManualModuleSelectHelper.VarType selectedVariant;
    [SerializeField] SelectCluster selectCluster;

    [Header("Debug")]
    [SerializeField] int totalTickets = -1, totalPausedTickets = -1;
    [SerializeField] ScreenLink savedScreenLink;
    [SerializeField] public string link;
    List<AppApiManager.Ticket> pausedTickets, tickets, openTickets;
    ScreenLink currentScreenLink;
    AppApiManager.Ticket curTicket;
    [SerializeField] List<AppApiManager.Ticket> vehicleTickets, escalatedTickets, closeTickets, reassignedTickets, resolvedTickets, newEscaletedTickets;
    AppApiManager.VehicleMasters vehicleMasters;

    [SerializeField] List<AppApiManager.ServerVehicleModule> vehicleMasterModules;
    [SerializeField] GameObject StartButtonOfSelectedTicket;
    public struct ScreenID
    {
        public static readonly int
             LOGIN = 1,
             TUTORIAL_BEGIN = 3,
             DASHBOARD = 6,
             PROFILE = 7,
             VIN_INPUT = 9,
             DT_END_SCREEN = 12,
             VEHICLE_DETAILS = 13,
             PROCESS_SELECTION = 15,
             EXPLODED_VIEW_RNR = 19,
             ELSA_DISCLAIMER = 21,
             DT_LIST = 26,
             OPEN_TICKETS = 30,
             WIRING_HARNESS = 31,
            TICKET_DETAILS = 34,
            STEPS_RNR = 37,
            DT = 43

            ;

    }
    struct TicketUpdateType
    {
        public static readonly int
             DT = 1, RNR = 2,
        ALL = 3, CANCEL = -1
        ;
    }
    public ManualModuleSelectHelper.VarType GetSelectedVariant() => selectedVariant;

    public void SetSelectedVariant(ManualModuleSelectHelper.VarType newVariant)
    {
        selectedVariant = newVariant;
        Debug.Log("EMPTY OR NOT:" + selectedVariant);
    }
    #region UI Update Section
    [System.Serializable]
    struct ProfileScreen
    {
        public Text name, email, location, contact;
        public InputField email_Input, password_Input;
    }
    [System.Serializable]
    struct TicketDetailsScreen
    {
        public Text ticketId, vin, issue_short, added_Time, last_Updated, ticket_stat, closing_Remark;
        public GameObject vinRef, closingRemarkRef;
    }
    bool STARTED_DT = false, STARTED_RNR = false;
    #region Pause-Resume

    [System.Serializable]
    class ScreenLink
    {
        public int module;
        public string process;
        public int stepNumber = -1;
        public string partId;
        public int dtId = -1;
        //    bool attached = false;
        public string dtActivityNumber;
    }
    private void Reset()
    {
        screenManager = FindObjectOfType<UIScreenManager>();
    }
    private void OnApplicationQuit()
    {
        Debug.Log("Saving");
        apiManager.SaveData();
    }
    void Start()
    {
        apiManager.LoadSaveData();

        if (apiManager.isLoggedIn())
        {
            Debug.Log("is Logged In");
            UpdateProfile();
            GetTicketCounts();
        }
        else
            Debug.Log("LOGIN");
        //      ShowLoginScreen();

        //StartCoroutine(CheckConnectivity());

    }
    public void OpenInternalLink(string text)
     => OpenAppLink(text);
    void OpenAppLink(string appLinkText)
    {
        if (appLinkText.Length < 1)
            screenManager.SelectScreen(ScreenID.PROCESS_SELECTION);
        else
        {
            ScreenLink slink = JsonUtility.FromJson<ScreenLink>(appLinkText);
            OpenScreenLink(slink);
        }
    }
    void OpenScreenLink(ScreenLink sLink)
    {
        if (sLink.module > -1)
            moduleManager.SelectModule(sLink.module);
        else
            Debug.Log("No Module Present");
        currentScreenLink = sLink;
        moduleManager.onSelect += OnModuleLoadComplete;
    }
    void OnModuleLoadComplete(int moduleId)
    {
        Debug.Log("MODULE: " + moduleId + " LOAD COMPLETE");
        if (moduleId != currentScreenLink.module)
            Debug.Log("Module Mismatch between linked module and Loaded Module");
        moduleManager.onSelect -= OnModuleLoadComplete;

        if (currentScreenLink.dtActivityNumber.Length > 0)
        {
            if (currentScreenLink.dtId >= 0)
            {
                dtManager.LoadData(currentScreenLink.dtId);
                if (currentScreenLink.dtActivityNumber.Length > 5)
                    dtManager.LoadAndStartFrom(currentScreenLink.dtId, currentScreenLink.dtActivityNumber);
                else
                    dtManager.LoadAndStart(currentScreenLink.dtId);
                screenManager.SelectScreen(ScreenID.DT);
            }
        }
        if (currentScreenLink.partId.Length > 0)
        {
            partIdManager.PlaySequence(currentScreenLink.partId);
            referencesManager.SetStepsView();
            screenManager.SelectScreen(ScreenID.STEPS_RNR);
            referencesManager.FastForwardToStep(currentScreenLink.stepNumber);

        }

        //   OpenScreenOnCondition(currentScreenLink);
    }
    void OpenScreenOnCondition(ScreenLink sLink)
    {

        if (sLink.dtActivityNumber.Length > 0)
            screenManager.SelectScreen(ScreenID.DT);
        if (sLink.partId != "" && sLink.process != "" && sLink.stepNumber >= 0)
        {
            screenManager.SelectScreen(ScreenID.STEPS_RNR);
            if (sLink.process.ToUpper().Contains("DIS"))
                stepsSection.Dismantling(sLink.stepNumber);
            else
                stepsSection.Assembly(sLink.stepNumber);

        }
    }
    public void SetStepReference(Steps newRef)
       => stepsSection = newRef;
    public string GetScreenLinkString()
    {
        return GetScreenLinkString(GetTicketTypeUpdateType());
    }
    public string GetScreenLinkString(int ticketUpdateMode)
    {
        ScreenLink sl = new ScreenLink();
        if (moduleManager.GetLastLoadedModuleIndex() >= 0)
            sl.module = moduleManager.GetLastLoadedModuleIndex();
        else
            Debug.LogError("Implement Module Selection.");
        if (ticketUpdateMode == TicketUpdateType.DT || ticketUpdateMode == TicketUpdateType.ALL)
        {
            if (dtManager.GetCurrentDtIndex() >= 0)
            {
                sl.dtId = dtManager.GetCurrentDtIndex();
                sl.dtActivityNumber = dtManager.GetCurrentActivityNumber();
            }
        }
        if (ticketUpdateMode == TicketUpdateType.RNR || ticketUpdateMode == TicketUpdateType.ALL)
        {
            sl.partId = partIdManager.GetPartResult();
            sl.process = stepsSection.currentProcess.ToString();
            sl.stepNumber = stepsSection.currentStep;
        }
        savedScreenLink = sl;
        return JsonUtility.ToJson(sl);
    }
    public void SaveState() => link = GetScreenLinkString();

    public void LoadState() => LoadState(link);

    public void LoadState(string linkString)
        => OpenAppLink(linkString);

    public void LoadClusterForTicket()
    {
        if (ticketCreator)
        {
            ticketCreator.LoadClusterUI();
        }
        else
            Debug.LogError("No Ticket Creator Assigned!", gameObject);
    }
    public void ShowEscalateCurrentTicketDialog()
    {
        if (IsCurrentTicketOpen())
        {
            if (apiManager.currentTicket.ShortDescription.Length > 0)
                screenManager.GetDialog().Show("Open Session", "Escalate currently opened ticket with issue: "
                    + apiManager.currentTicket.ShortDescription, "Escalate", EscalateCurrentTicket);
        }
        else
            Debug.Log("No Ticket Open!");
    }

    public bool IsCurrentTicketOpen()
    {
        bool ticketExists = apiManager.currentTicket.VehicleID != null;
        if (ticketExists)
            return apiManager.currentTicket != null ? apiManager.currentTicket.VehicleID.Length > 0 : false;
        else
            return ticketExists;
    }

    public void CreateTicket(InputField issueInput)
    {
        Debug.Log("Creating Ticket");
        if (issueInput.text.Length > 2)
            CreateTicket(issueInput.text);
    }

    int GetTicketTypeUpdateType()
    {
        int cur = screenManager.GetCurrentScreenIndex();
        if (!STARTED_DT)
            STARTED_DT = (cur == ScreenID.DT);
        if (!STARTED_RNR)
            STARTED_RNR = (cur == ScreenID.STEPS_RNR);

        int updateType = (STARTED_DT && STARTED_RNR) ? (TicketUpdateType.ALL) : -1;
        if (STARTED_DT)
            updateType = TicketUpdateType.DT;
        if (STARTED_RNR)
            updateType = TicketUpdateType.RNR;
        return updateType;

    }
    public void CreateTicket(string issueInput)
    {
        if (issueInput.Length > 2)
        {
            apiManager.CreateTicket(issueInput, ticketCreator.kilometer.text);
            apiManager.OnLogToScreen += screenManager.DebugToScreen;
            screenManager.SelectScreen(ScreenID.PROCESS_SELECTION);
        }
    }
    bool quitAfter = false;

    //Logout & Escalate ticket if it's open
    public void CheckTicketStatusOnExit()
    {

        if (IsCurrentTicketOpen())
            screenManager.GetDialog().Show("Open Session", "Do you want to exit?", new List<UIScreenManager.NamedActions>() {
                new UIScreenManager.NamedActions("Yes",()=>{screenManager.GetDialog().Close();
                    //ShowQuitDialog();
                    Mixpanel.Track("User_LoggedOut");
                    Mixpanel.Reset();
                    Invoke("UserLogOut",1f);
                })
            });
        else
        {
            //ShowQuitDialog();
            Mixpanel.Track("User_LoggedOut");
            Mixpanel.Reset();
            Invoke("UserLogOut", 1f);

        }
    }

    void UserLogOut()
    {
        SceneManager.LoadScene(0);
        Debug.Log("Loading first scene");
    }

    //Quit Playing
    public void ShowQuitDialog()
    {
        screenManager.GetDialog().Show("Exit", "Are you sure to quit Interactive Diagnostics", "Quit", () =>
        {
            Mixpanel.Track("Application_Quit");
            Mixpanel.Reset();
            Debug.Log("Quit Application");
            Application.Quit();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        });
    }


    IEnumerator CheckConnectivity()
    {
        WWWForm form = new WWWForm();
        form.AddField("connectivity", "connection");

        UnityWebRequest w = UnityWebRequest.Post("http://54.208.104.117/VW/Services.php?Service=" + "checkConnectivity", form);
        yield return w.SendWebRequest();
        if (w.error == null)
        {
            string itemData = w.downloadHandler.text;
            if (itemData.Contains("success"))
            {
            }
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
#endif
            }
        }
        else
            Debug.Log("Error during upload: " + w.error);
    }

    [HideInInspector] public string _escalationRemark = "";
    [HideInInspector] public string _closingRemark = "";

    public void JustExit()
    {
        
    }
    public void EscalateCurrentTicket()
    {
        if (apiManager.OnTicket())
        {
            Debug.LogError("API Ticket" + apiManager.OnTicket().ToString());
            apiManager.EscalateCurrentTicket(OnCurrentTicketEscalated, _escalationRemark);
            /* apiManager.SendLogToEmail(apiManager.currentTicket.TicketID, (msg) =>
                        {
                            Debug.LogError( apiManager.currentTicket.TicketID + " Ticket ID");
                            if (AppApiManager.GetServerResponse(msg).status.Contains("success"))
                                apiManager.EscalateCurrentTicket(OnCurrentTicketEscalated);
                        });*/
        }
    }

    public void CloseCurrentTicket()
    {
        if (apiManager.OnTicket())
        {
            Debug.LogError("API Ticket" + apiManager.OnTicket().ToString());
            apiManager.CloseTicket(OnCurrentTicketClosed, _closingRemark);
        }
    }

    /// Dhiraj | 5-6-2024 || Send email to all
    public void ReassignCurrentITTicket()
    {
        apiManager.ReassignCurrentTicket(OnCurrentTicketReassign);
    }


    public void OnCurrentTicketReassign(string arg0)
    {
        apiManager.SendLogToEmail(AppApiManager.ticketToEscalate, (msg) =>
        {
            Debug.LogError(AppApiManager.ticketToEscalate + " Ticket ID");
            if (AppApiManager.GetServerResponse(msg).status.Contains("success"))
                Debug.Log($"<color=green> Dhiraj : Email Sent </color>");
            Debug.Log("Email sent");
        });

        Debug.Log("SERVER RESPONSE: " + arg0);
        Debug.Log($"<color=green> Dhiraj : Server response = {arg0} </color>");
        apiManager.OnCurrentTicketEscalated(arg0);
        screenManager.GetDialog().Show("Success!", AppApiManager.GetServerResponse(arg0).response_data.message
            + System.Environment.NewLine
            + "Ticket assigned successfully!");
        Mixpanel.Track("Ticket_Escalated_To_L1");
        if (quitAfter)
            ShowQuitDialog();
        else
            screenManager.SelectScreen(ScreenID.VIN_INPUT);
    }


    private void OnCurrentTicketEscalated(string arg0)
    {
        if (apiManager.OnTicket())
        {
            apiManager.SendLogToEmail(AppApiManager.ticketToEscalate, (msg) =>
            {
                Debug.LogError(AppApiManager.ticketToEscalate + " Ticket ID");
                if (AppApiManager.GetServerResponse(msg).status.Contains("success"))
                    Debug.Log("Email sent");
            }, 1);

            Debug.Log("SERVER RESPONSE: " + arg0);
            apiManager.OnCurrentTicketEscalated(arg0);
            screenManager.GetDialog().Show("Success!", AppApiManager.GetServerResponse(arg0).response_data.message
                + System.Environment.NewLine);
            Mixpanel.Track("Ticket_Closed_To_L1");
            if (quitAfter)
                ShowQuitDialog();
            else
                screenManager.SelectScreen(ScreenID.VIN_INPUT);
        }
    }

    private void OnCurrentTicketClosed(string arg0)
    {
        if (apiManager.OnTicket())
        {
            apiManager.SendLogToEmail(AppApiManager.ticketToEscalate, (msg) =>
            {
                Debug.LogError(AppApiManager.ticketToEscalate + " Ticket ID");
                if (AppApiManager.GetServerResponse(msg).status.Contains("success"))
                    Debug.Log("Email sent");
            }, 1);

            Debug.Log("SERVER RESPONSE: " + arg0);
            apiManager.OnCurrentTicketEscalated(arg0);
            screenManager.GetDialog().Show("Success!", AppApiManager.GetServerResponse(arg0).response_data.message
                + System.Environment.NewLine);
            Mixpanel.Track("Ticket_Escalated_To_L1");
            if (quitAfter)
                ShowQuitDialog();
            else
                screenManager.SelectScreen(ScreenID.VIN_INPUT);
        }
    }

    public void PauseTicket()
    {
        if (apiManager.OnTicket())
        {
            int updateType = GetTicketTypeUpdateType();
            if (updateType > 0)
            {
                DebugLog("Updating Ticket : " + apiManager.currentTicket.ShortDescription);
                apiManager.PauseTicket(GetScreenLinkString(updateType));
                Mixpanel.Track("Ticket_Paused");
            }
        }
    }
    public void UpdateTicket()
    {

        if (apiManager.OnTicket())
        {
            int updateType = GetTicketTypeUpdateType();
            if (updateType > 0)
            {
                DebugLog("Updating Ticket : " + apiManager.currentTicket.ShortDescription);
                apiManager.UpdateTicket(GetScreenLinkString(updateType));
            }
        }
    }

    #endregion

    #region Getters
    public AssetLoader GetAssetLoader()
    {
        if (!assetLoader)
            assetLoader = FindObjectOfType<AssetLoader>();
        return assetLoader;
    }
    public StepsEximProcessor GetStepsEximProcessor()
    {
        if (!stepsExim)
            stepsExim = FindObjectOfType<StepsEximProcessor>();
        return stepsExim;
    }
    public UIScreenManager GetScreenManager()
    {
        if (!screenManager)
            screenManager = FindObjectOfType<UIScreenManager>();
        return screenManager;
    }
    public VWModuleManager GetModuleManager()
    {
        if (!moduleManager)
            moduleManager = FindObjectOfType<VWModuleManager>();
        return moduleManager;
    }
    public VWReferencesManager GetReferenceManager()
    {
        if (!referencesManager)
            referencesManager = FindObjectOfType<VWReferencesManager>();
        return referencesManager;
    }
    public AppApiManager GetApiManager()
    {
        if (apiManager == null)
        {
            apiManager = FindObjectOfType<AppApiManager>();
        }
        return apiManager;
    }
    public AutoModuleSelectHelper GetAutoSelector()
    {
        if (autoModuleSelector == null)
            autoModuleSelector = FindObjectOfType<AutoModuleSelectHelper>();
        return autoModuleSelector;
    }
    public void SelectSvrModule(string modName, string add)
    {
        //Debug.Log(modName+" Selecting Module with Address : " + add);

        if (modName.Length > 0)
            Debug.Log("Selecting Module:" + modName);
        if (add.Length > 0)
            Debug.Log("Selecting Module with Address: " + add);
        moduleLoader.SelectModule(modName, add);
        //    DownloadLanguageAfterModule();
    }
    public ModuleLoadManager GetModuleLoader()
    {
        //Debug.Log("MODULE LOADER INVOKED");
        if (moduleLoader == null)
            moduleLoader = FindObjectOfType<ModuleLoadManager>();
        return moduleLoader;
    }
    public void DownloadLanguageAfterModule()
    {
        Debug.Log("Will Download Language after Module");
        moduleManager.onSelect += ModuleManager_onSelect;
    }

    public ManualModuleSelectHelper GetManualSelector()
    {
        if (manualSelector == null)
            manualSelector = FindObjectOfType<ManualModuleSelectHelper>();
        return manualSelector;
    }

    private void ModuleManager_onSelect(int id)
    {
        Debug.Log("Downloading Language....");
        moduleManager.onSelect -= ModuleManager_onSelect;
        moduleLoader.DownloadLanguageSupport();
    }
    #endregion
    public UiVehicleDetails GetVehicleDetailsScreen() => vehicleDetailsScreen;
    // Start is called before the first frame update
    void ShowLoginScreen() => screenManager.SelectScreen(ScreenID.LOGIN);
    public void Login()
    {
        apiManager.OnLogin += PostLogin;
        apiManager.Login(profile.email_Input.text, profile.password_Input.text);
    }
    public void UpdateVehicleMasters(AppApiManager.VehicleMasters masters)
    {
        if (masters != null)
        {
            vehicleMasters = masters;
            if (vehicleMasters.vehicle_modules != null)
                if (vehicleMasters.vehicle_modules.Length > 0)
                {
                    GetModuleLoader().SetModuleReferences(vehicleMasters.vehicle_modules);
                    Debug.Log("<color=blue>Updated Module References</color>");
                }
        }
        else
            Debug.LogError("No Vehicle Masters Recieved!");

        screenManager.loadingAnimated.SetActive(false);

    }
    public bool HasModuleIdFor(string name)
    {
        bool result = false;
        result = GetServerModuleIdFor(name).Length > 0;
        return result;
    }
    public string GetServerModuleIdFor(string name)
    {
        string result = "";
        int foundIdAt = -1;
        if (name.Length > 0)
        {
            if (vehicleMasterModules.Count > 0)
                for (int i = 0; i < vehicleMasterModules.Count; i++)
                {
                    string svrModName = vehicleMasterModules[i].ModuleName.Replace(" ", "");
                    if (name.Contains(svrModName))
                    {
                        Debug.Log("Selected Id: " + vehicleMasterModules[i].ModuleID);
                        foundIdAt = i;
                        result = vehicleMasterModules[i].ModuleID;
                    }
                }
        }
        if (result.Length > 0)
            Debug.Log("Found Module Address for " + name + " = " + result);
        else
            Debug.LogError("Did not Find ModuleId for " + name);

        return result;
    }
    public void GetVehicleDetails()
    {
        //  apiManager.OnRecieveVehicleDetails += PostVehicleDetails;
        apiManager.GetVehicleDetails(ticketScreen.vinIp.text, PostVehicleDetails);
    }
    void PostLogin(string msg)
    {
        screenManager.SelectScreen(ScreenID.DASHBOARD);
        Debug.Log("Login:" + msg);
    }
    void PostVehicleDetails(string msg)
    {
        AppApiManager.VehicleDetails vd = AppApiManager.GetServerResponse(msg).response_data.vehicle_details;

        Debug.Log(">>>>>> " + vd.VIN);
        screenManager.SelectScreen(ScreenID.VEHICLE_DETAILS);
        vehicleDetailsScreen.Load(vd);
        if (vehicleDetailsScreen.GetCurrentVehicleDetails() != null)
            apiManager.SetCurrentVehicleDetails(vehicleDetailsScreen.GetCurrentVehicleDetails());
    }
    public void ShowVehicleDetails(AppApiManager.VehicleDetails details)
    {
        vehicleDetailsScreen.Load(details);
        screenManager.SelectScreen(ScreenID.VEHICLE_DETAILS);

    }
    public void LogOut()
    {
        // apiManager.Logout();
        // screenManager.SelectScreen(16);
    }
    public void UpdateProfile()
    {
        profile.email.text = apiManager.GetProfileInfo().EmailID;
        profile.name.text = apiManager.GetProfileInfo().FirstName + " "
           + apiManager.GetProfileInfo().LastName;
        profile.contact.text = apiManager.GetProfileInfo().Mobile;
    }
    public void UpdateDashboard()
    {
        DebugLog("Updating Dashboard..");
        GetTicketCounts();
    }
    public void PopulateTools()
    {
        Debug.Log("Downloading Images");
        if (!stepsExim)
            stepsExim = FindObjectOfType<StepsEximProcessor>();
        if (stepsExim)
        {
            List<string> toolNames = stepsExim.GetToolNamesForCurrentSteps();
            int totalDuplicates = 0;
            List<string> filteredToolNames = new List<string>();
            if (toolNames.Count > 0)
            {
                for (int i = 0; i < toolNames.Count; i++)
                {
                    bool found = false;
                    string currName = toolNames[i];
                    if (filteredToolNames.Count > 0)
                        for (int j = 0; j < filteredToolNames.Count; j++)
                        {
                            if (filteredToolNames[j] == currName)
                                found = true;
                        }
                    if (!found)
                        filteredToolNames.Add(currName);
                    else
                        totalDuplicates++;
                }
                if (filteredToolNames.Count > 0)
                    Debug.Log("Found Duplicates:" + totalDuplicates + ", Downloading Images" + filteredToolNames.Count);
                if (filteredToolNames.Count > 0)
                    assetLoader.DownloadImages(filteredToolNames, "", ".png");
                else
                    Debug.LogError("No Tools Found!!!!");
            }
        }
    }
    void GetTicketCounts()
    {


    }
    public void GetPausedTickets()
    {
        pausedTickets = GetTicketList("Paused");
        ticketScreen.pausedTickets.Load(GetNameListFor(pausedTickets).ToArray());
    }
    public void ResumeOpenTicket(int listId)
    {
        Debug.Log("TOTAL :" + tickets.Count);
        Debug.Log("RESUMING :" + tickets[listId].ShortDescription);
        ResumeTicket(listId);
    }

    //Veiw details of the a specific ticket
    public void ViewDetailsOpenTicket(int listId)
    {
        //listId = listId / curticket;

        Debug.LogError("  Select Ticket Type----->>>>>>>>       " + listId);
        List<AppApiManager.Ticket> curTicketList = new List<AppApiManager.Ticket>();
        switch (ticketScreen.ticketType)
        {
            case TicketScreen.TicketType.Open:
                curTicketList = vehicleTickets;
                break;
            case TicketScreen.TicketType.Escalated:
                curTicketList = newEscaletedTickets;
                break;
            case TicketScreen.TicketType.Reassigned:
                curTicketList = reassignedTickets;
                break;
            case TicketScreen.TicketType.Resolved:
                curTicketList = resolvedTickets;
                break;
            case TicketScreen.TicketType.Close:
                curTicketList = closeTickets;
                break;
            default:
                break;
        }
        Debug.Log("TOTAL :" + curTicketList.Count + "     " + curTicketList[0].TicketStatus);

        //listId = listId / 3;
        Debug.LogError(listId + " LIST ID");

        if (ticketScreen.ticketType == TicketScreen.TicketType.Open)
        {
            apiManager.currentTicket = curTicketList[listId];
            curTicket = curTicketList[listId];
            Debug.Log("RESUMING :" + curTicket.ShortDescription);
            ShowTicketDetails(curTicketList[listId], TicketScreen.TicketType.Open);
            sendDetailsToMail.SetActive(true);
        }
        else if (ticketScreen.ticketType == TicketScreen.TicketType.Escalated)
        {
            apiManager.currentTicket = curTicketList[listId];
            curTicket = curTicketList[listId];
            Debug.Log("RESUMING :" + curTicket.ShortDescription);
            // Debug.Log(curTicket.);
            ShowTicketDetails(curTicketList[listId], TicketScreen.TicketType.Escalated);
            sendDetailsToMail.SetActive(true);
        }
        else if (ticketScreen.ticketType == TicketScreen.TicketType.Reassigned)
        {
            apiManager.currentTicket = curTicketList[listId];
            curTicket = curTicketList[listId];
            Debug.Log("RESUMING :" + curTicket.ShortDescription);
            // Debug.Log(curTicket.);
            ShowTicketDetails(curTicketList[listId], TicketScreen.TicketType.Reassigned);
            sendDetailsToMail.SetActive(false);
        }
        else if (ticketScreen.ticketType == TicketScreen.TicketType.Resolved)
        {
            apiManager.currentTicket = curTicketList[listId];
            curTicket = curTicketList[listId];
            Debug.Log("RESUMING :" + curTicket.ShortDescription);
            // Debug.Log(curTicket.);
            ShowTicketDetails(curTicketList[listId], TicketScreen.TicketType.Resolved);
            sendDetailsToMail.SetActive(false);
        }
        else if (ticketScreen.ticketType == TicketScreen.TicketType.Close)
        {
            //Updated on 28th Jan 2025 by Dhiraj
            //Disable Start Button on click 
            StartButtonOfSelectedTicket.SetActive(false);
            apiManager.currentTicket = curTicketList[listId];
            curTicket = curTicketList[listId];
            Debug.Log("RESUMING :" + curTicket.ShortDescription);
            // Debug.Log(curTicket.);
            ShowTicketDetails(curTicketList[listId], TicketScreen.TicketType.Close);
            sendDetailsToMail.SetActive(false);
        }
    }
    public void OnHome()
    {
        Mixpanel.Track("Home_Clicked");
        if (!loginManager.DidShowDisclaimer())
            loginManager.ShowDisclaimerIfNotShown();
        else
        {
            if (apiManager.currentTicket.TicketID != null && apiManager.currentTicket.TicketID.Length > 0)
            {
                screenManager.SelectScreen(ScreenID.PROCESS_SELECTION);

            }
            else
            {

                screenManager.Toast("Select to continue");
                screenManager.SelectScreen(ScreenID.VIN_INPUT);
            }
        }
    }
    void DisableLoading(string msg)
    {
        apiManager.onReqError.RemoveListener(DisableLoading);
        screenManager.DebugToScreen("error: " + msg);
        screenManager.SetLoadingState(false);
    }

    public void OnTicketResume()
    {
        if (curTicket != null)
        {
            Debug.Log("Resuming Ticket for ..." + curTicket.VIN.ToString());
            if (curTicket.VIN.Length > 0)
            {
                screenManager.SetLoadingState(true);
                apiManager.onReqError.AddListener(DisableLoading);
                apiManager.GetVehicleDetails(curTicket.VIN, LoadVehicle);
                //apiManager.GetVehicleDetails(curTicket)
                // Mixpanel.Track("VIN_Details_Received");
            }
            else
                NewTicketProcess();
        }
        else
            NewTicketProcess();
        void NewTicketProcess()
        {
            Debug.Log("Opening VIN Identification Screen!");
            screenManager.SelectScreen(ScreenID.VIN_INPUT);
        }
    }
    public void LoadVehicle(string arg0)
    {
        screenManager.SetLoadingState(false);
        // Debug.Log("VN: " +);
        apiManager.onReqError.RemoveListener(DisableLoading);
        //   AppApiManager.ServerResponseVehDetails serverResp = AppApiManager.GetServerResponseVehicleDetails(arg0);
        AppApiManager.ServerResponseVehDetails serverResp = AppApiManager.GetServerResponseVehicleDetails(arg0);
        if (serverResp.status.Contains("fail"))
            screenManager.Toast(serverResp.response_data.message);
        else
        if (apiManager.HasVehicleDetails())
        {
            autoModuleSelector.LoadVehicleAndModules(apiManager.GetCurVehicleDetails());
            string var = apiManager.GetCurVehicleDetails().VariantName;
            Debug.Log("Variant for Resume Ticket:" + var);
            GetModuleLoader().SetModuleReferences(serverResp.response_data.vehicle_modules);
            screenManager.SelectScreen(ScreenID.PROCESS_SELECTION);
            if (apiManager.OnTicket())
            {
                if (GetLocalizationSupport().selectedLangauge != LocalizationSupport.Language.English)
                    screenManager.Toast("resume Ticket: " + apiManager.currentTicket.TicketID
                        + System.Environment.NewLine
                        + ", updated language to " + GetLocalizationSupport().selectedLangauge.ToString());
                else
                    screenManager.Toast("resume Ticket: " + apiManager.currentTicket.TicketID);
            }
        }
        else
            screenManager.SelectScreen(ScreenID.VIN_INPUT);
    }
    public void ShowTicketDetails(int ticketNum)
    {
        curTicket = openTickets[ticketNum];
        ticketDetails.ticketId.text = curTicket.TicketID;
        ticketDetails.issue_short.text = (curTicket.ShortDescription);
        ticketDetails.added_Time.text = curTicket.AddedDateTime;
        ticketDetails.vin.text = (curTicket.VIN);
        ticketDetails.ticket_stat.text = (curTicket.TicketStatus);
        ticketDetails.last_Updated.text = (curTicket.UpdatedDateTime);
        screenManager.SelectScreen(ScreenID.TICKET_DETAILS);
        //apiManager.GetTicketDetails(openTickets[ticketNum].TicketID, OnRecieveDetails);
    }
    public void ShowTicketDetails(AppApiManager.Ticket ticket, TicketScreen.TicketType type)
    {
        if (type == TicketScreen.TicketType.Close)
        {
            ticketDetails.vinRef.SetActive(false);
        }
        else
        {
            ticketDetails.vinRef.SetActive(true);
        }

        if (type == TicketScreen.TicketType.Open || type == TicketScreen.TicketType.Escalated)
        {
            ticketDetails.closingRemarkRef.SetActive(false);
        }
        else
        {
            ticketDetails.closingRemarkRef.SetActive(true);
        }

        ticketDetails.ticketId.text = ticket.TicketID;
        ticketDetails.issue_short.text = (ticket.ShortDescription);
        ticketDetails.added_Time.text = ticket.AddedDateTime;
        ticketDetails.vin.text = (ticket.VIN);
        ticketDetails.ticket_stat.text = (ticket.TicketStatus);
        ticketDetails.last_Updated.text = (ticket.UpdatedDateTime);
        ticketDetails.closing_Remark.text = FormatClosingRemark(ticket.ClosingRemark);
        screenManager.SelectScreen(ScreenID.TICKET_DETAILS);
        //apiManager.GetTicketDetails(openTickets[ticketNum].TicketID, OnRecieveDetails);
    }
    public static string FormatClosingRemark(string closingRemark)
    {
        // Split the remark into individual lines based on "|"
        string[] remarks = closingRemark.Split('|');

        // Get the last remark
        string lastRemark = remarks[remarks.Length - 1].Trim();

        // Initialize variable to store the formatted remark
        string formattedRemark = string.Empty;

        // Extract timestamp, username, and message using regex
        Match match = Regex.Match(lastRemark, @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) (\w+) (.+)$");

        // If regex matches, concatenate timestamp, username, and message
        if (match.Success)
        {
            string timestamp = match.Groups[1].Value;
            string username = match.Groups[2].Value;
            string message = match.Groups[3].Value;

            // Concatenate each part with spaces
            formattedRemark = $"{message}";
        }
        else
        {
            // If regex doesn't match, just use the whole remark
            formattedRemark = lastRemark;
        }

        return formattedRemark;
    }
    bool useAsCurrentTicket = false;
    public void GetTicketDetails(string ticketId, bool useAsCurr = false)
    {
        useAsCurrentTicket = useAsCurr;
        apiManager.GetTicketDetails(ticketId, OnRecieveDetails);
    }
    public void SendLogToMail(int id)
    {
        apiManager.SendLogToEmail(curTicket.TicketID, OnMailSent, id);
    }
    void OnMailSent(string msg)
    {
        screenManager.Toast("logs sent to mail!");
        Debug.Log("Mail Sent!:" + msg);
    }
    private void OnRecieveDetails(string arg0)
    {
        Debug.Log(arg0);
        AppApiManager.Ticket t = AppApiManager.GetServerResponse(arg0).response_data.ticket_details;
        if (useAsCurrentTicket)
        {
            Debug.Log("Using as current ticket");
            apiManager.currentTicket = AppApiManager.GetServerResponse(arg0).response_data.ticket_details;
        }
    }
    public void ResumeTicket(int listId)
    {
        DebugLog("Resuming Ticket " + tickets[listId].ShortDescription);
        apiManager.currentTicket = tickets[listId];
        OpenAppLink(tickets[listId].LongDescription);
    }
    public void GetTickets()
    {
        Debug.LogError("Old Method in use!");
        openTickets = GetTicketList("Open");
        ticketScreen.openTickets.Load(GetNameListFor(openTickets).ToArray());
    }
    public void LoadToTicketScreen(List<AppApiManager.Ticket> inList, ImageLabelList uiList)
    {
        //  openTickets = inList;

        if (inList.Count > 0)
        {
            //uiList.LoadWithDetails(GetNameListFor(inList).ToArray(), GetVINListFor(inList).ToArray(), GetDateTimeListFor(inList).ToArray());
            uiList.Load(GetNameListFor(inList).ToArray(), GetTicketDetailsListFor(inList).ToArray(), GetDateTimeListFor(inList).ToArray());
            ///uiList.Load(GetTicketDetailsListFor(inList).ToArray());
            //uiList.Load(GetDateTimeListFor(inList).ToArray());
        }
        else
        {
            uiList.Load(new string[] { "" });
            screenManager.Toast("No tickets available");
            //Debug.LogError(  + inList.Count + " Ticket count");
        }
    }
    public List<AppApiManager.Ticket> GetOpenTickets() => openTickets;
    public List<AppApiManager.Ticket> GetEscalatedTickets() => escalatedTickets;
    public List<AppApiManager.Ticket> GetReassignedTickets() => reassignedTickets;
    public List<AppApiManager.Ticket> GetResolvedTickets() => resolvedTickets;
    public List<AppApiManager.Ticket> GetNewEscalatedTickets() => newEscaletedTickets;

    public List<AppApiManager.Ticket> GetCloseTickets() => closeTickets;
    public void ShowOpenTickets()
    {
        ticketScreen.openTickets.rowElement.button.interactable = true;
        //ticketScreen.SelectTicketType(TicketScreen.TicketType.Open);
        //LoadToTicketScreen(openTickets, ticketScreen.openTickets);
        screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);
        //ticketScreen.openTickets.gameObject.SetActive(true);
    }

    //This function calls in Open Session tab
    public void SelectOpenTicket()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Open);
        LoadToTicketScreen(openTickets, ticketScreen.openTickets);
    }
    public void SelectEscalatedTicket()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Escalated);
        LoadToTicketScreen(escalatedTickets, ticketScreen.escalatedTickets);
    }
    public void SelectClosedTicket()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Close);
        LoadToTicketScreen(closeTickets, ticketScreen.closeTickets);
    }

    /// <summary>
    /// Dhriaj 25-6-2024
    /// </summary>

    public void ShowResolvedTickets()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Resolved);
        if (ticketScreen.resolvedTicket == null)
        {
            ticketScreen.resolvedTicket = ticketScreen.openTickets;
            //ticketScreen.openTickets.gameObject.SetActive(true);
        }
        else
        {
            ticketScreen.resolvedTicket.rowElement.button.interactable = true;
            ticketScreen.SelectTicketType(TicketScreen.TicketType.Resolved);
            LoadToTicketScreen(resolvedTickets, ticketScreen.resolvedTicket);
            screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);
        }
    }

    public void ShowReassignedTickets()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Reassigned);
        if (ticketScreen.reassignedTicket == null)
        {
            ticketScreen.reassignedTicket = ticketScreen.openTickets;
            //ticketScreen.openTickets.gameObject.SetActive(true);
        }
        else
        {
            ticketScreen.reassignedTicket.rowElement.button.interactable = true;
            ticketScreen.SelectTicketType(TicketScreen.TicketType.Reassigned);
            LoadToTicketScreen(reassignedTickets, ticketScreen.reassignedTicket);
            screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);
        }
    }

    public bool isSorted = false;
    private void Update()
    {

    }


    public void TryProcessTicketsAndShow()
    {
        if (escalatedTickets == null || escalatedTickets.Count == 0)
            return;

        ShortListOfTickets();

        Debug.Log("Shortlisting Tickets...");
        Debug.Log("Shortlisting Tickets... " + escalatedTickets.Count + " " + resolvedTickets.Count + " " + reassignedTickets.Count);

        if (ticketScreen.escalatedTickets == null)
        {
            ticketScreen.escalatedTickets = ticketScreen.openTickets;
            // ticketScreen.openTickets.gameObject.SetActive(true);
        }
        else
        {
            ticketScreen.escalatedTickets.rowElement.button.interactable = true;
            ticketScreen.SelectTicketType(TicketScreen.TicketType.Escalated);
            LoadToTicketScreen(newEscaletedTickets, ticketScreen.escalatedTickets);
            screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);
        }
    }
    public void ShortListOfTickets()
    {
        newEscaletedTickets = MergeAndSortTickets(escalatedTickets, resolvedTickets, reassignedTickets);
    }

    public List<AppApiManager.Ticket> MergeAndSortTickets(
        List<AppApiManager.Ticket> escalated,
        List<AppApiManager.Ticket> resolved,
        List<AppApiManager.Ticket> reassigned)
    {
        // Use LINQ for more concise merging and sorting
        return escalated
            .Concat(resolved)
            .Concat(reassigned)
            .OrderByDescending(ticket => ticket.UpdatedDateTime)
            .Take(10)
            .ToList();
    }
    /// <summary>
    /// END
    /// </summary>
    public void ShowEscalatedTickets()
    {
        //ShortListOfTickets();
        if (ticketScreen.escalatedTickets == null)
        {
            // ticketScreen.escalatedTickets = ticketScreen.openTickets;
            //ticketScreen.openTickets.gameObject.SetActive(true);
        }
        else
        {
            ticketScreen.escalatedTickets.rowElement.button.interactable = true;
            ticketScreen.SelectTicketType(TicketScreen.TicketType.Escalated);
            LoadToTicketScreen(newEscaletedTickets, ticketScreen.escalatedTickets);
            screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);
        }
    }
    public void ShowCloseTickets()
    {
        ticketScreen.SelectTicketType(TicketScreen.TicketType.Close);
        if (ticketScreen.closeTickets == null)
        {
            ticketScreen.closeTickets = ticketScreen.openTickets;
            //ticketScreen.openTickets.gameObject.SetActive(true);
        }
        else
        {
            //  ticketScreen.closeTickets 
            ticketScreen.closeTickets.rowElement.button.interactable = true;
            ticketScreen.SelectTicketType(TicketScreen.TicketType.Close);
            LoadToTicketScreen(closeTickets, ticketScreen.closeTickets);
            screenManager.SelectScreenIfNotSelected(ScreenID.OPEN_TICKETS);

            //    ticketScreen.openTickets.gameObject.SetActive(true);
        }
    }
    public LocalizationSupport GetLocalizationSupport() => localizationSupport;
    public void SaveOpenVehicleTickets(List<AppApiManager.Ticket> vehTics)
    {
        vehicleTickets = vehTics;
        openTickets = vehTics;
        LoadToTicketScreen(vehicleTickets, ticketScreen.openTickets);
    }
    public void SaveEscalatedVehicleTickets(List<AppApiManager.Ticket> vehTics)
    {
        escalatedTickets = vehTics;
        LoadToTicketScreen(escalatedTickets, ticketScreen.openTickets);
    }
    public void SaveClosedVehicleTickets(List<AppApiManager.Ticket> vehTics)
    {
        closeTickets = vehTics;
        LoadToTicketScreen(closeTickets, ticketScreen.openTickets);
    }

    /// <summary>
    /// Dhiraj
    /// </summary>
    /// <param name="vehTics"></param>
    public void SaveReassignedVehicleTickets(List<AppApiManager.Ticket> vehTics)
    {
        reassignedTickets = vehTics;
        LoadToTicketScreen(reassignedTickets, ticketScreen.openTickets);
    }
    public void SaveResolvedVehicleTickets(List<AppApiManager.Ticket> vehTics)
    {
        resolvedTickets = vehTics;
        LoadToTicketScreen(resolvedTickets, ticketScreen.openTickets);
    }
    // END
    public void LoadOpenTickets() => LoadToTicketScreen(vehicleTickets, ticketScreen.openTickets);
    public void LoadEscalatedTickets() => LoadToTicketScreen(escalatedTickets, ticketScreen.openTickets);
    public void LoadClosedTickets() => LoadToTicketScreen(closeTickets, ticketScreen.openTickets);
    public void LoadReassignedTickets() => LoadToTicketScreen(reassignedTickets, ticketScreen.openTickets);
    public void LoadResolvedTickets() => LoadToTicketScreen(resolvedTickets, ticketScreen.openTickets);

    public bool HasOpenTickets()
    {
        bool result = false;
        if (escalatedTickets != null)
            result = escalatedTickets.Count > 0;
        return result;
    }
    public List<AppApiManager.Ticket> GetOpenVehicleTickets() => vehicleTickets;
    public void SelectModuleWithName(string name)
    {

    }
    #endregion
    // Update is called once per frame
    #region Helper
    List<string> GetNameListFor(List<AppApiManager.Ticket> inList)
    {
        List<string> result = new List<string>();
        foreach (AppApiManager.Ticket t in inList)
        {
            // result.Add(t.ShortDescription);
            result.Add(t.TicketUID);

        }
        return result;
    }

    List<string> GetTicketDetailsListFor(List<AppApiManager.Ticket> inList)
    {
        List<string> result = new List<string>();
        foreach (AppApiManager.Ticket t in inList)
        {
            if (string.IsNullOrWhiteSpace(t.VIN))
            {
                result.Add("[" + t.TicketType + "]");
            }
            else
            {
                result.Add(t.VIN);
            }
        }
        return result;
    }
    List<string> GetDateTimeListFor(List<AppApiManager.Ticket> inList)
    {
        List<string> result = new List<string>();
        foreach (AppApiManager.Ticket t in inList)
        {
            result.Add(t.AddedDateTime);
        }
        return result;
    }

    List<AppApiManager.Ticket> GetTicketList(string ticketState)
    {
        List<AppApiManager.Ticket> tList = apiManager.TicketList();
        List<AppApiManager.Ticket> result = new List<AppApiManager.Ticket>();
        foreach (AppApiManager.Ticket t in tList)
        {
            if (t.TicketStatus.Contains(ticketState))
            {
                result.Add(t);
            }
        }
        return result;
    }
    List<string> GetTicketList(string ticketState, out List<AppApiManager.Ticket> copyToList)
    {
        tickets = apiManager.TicketList();
        copyToList = new List<AppApiManager.Ticket>();
        List<string> ticketNameList = new List<string>();
        foreach (AppApiManager.Ticket t in tickets)
        {
            if (t.TicketStatus.ToLower().Contains(ticketState))
            {
                ticketNameList.Add(t.ShortDescription);
                copyToList.Add(t);
            }
        }
        return ticketNameList;
    }
    #endregion
    void DebugLog(string msg)
    {
        if (enableDebug)
            Debug.Log(msg);
    }
}
#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(ScreenLinker))]
public class ScreenLinkerEditor : Editor
{
    bool showDebugUi;


    public override void OnInspectorGUI()
    {
        showDebugUi = EditorGUILayout.Toggle("Debug", showDebugUi);
        if (showDebugUi)
            DrawDefaultInspector();
        if (EditorApplication.isPlaying)
        {


            ScreenLinker script = (ScreenLinker)target;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
                script.SaveState();
            if (GUILayout.Button("Load"))
                script.LoadState();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Update Ticket"))
                script.UpdateTicket();
            EditorGUILayout.Space(30);
            if (GUILayout.Button("Generate ScreenLink"))
                script.link = script.GetScreenLinkString();
            script.link = EditorGUILayout.TextField(new GUIContent("App Link String"), script.link);
            if (GUILayout.Button("Open Screen Link >>"))
                script.OpenInternalLink(script.link);
            if (GUILayout.Button("Download Images"))
                script.PopulateTools();

        }
        else
            EditorGUILayout.HelpBox("Only visible in Play Mode", MessageType.Info);
    }
}
#endif
#endregion