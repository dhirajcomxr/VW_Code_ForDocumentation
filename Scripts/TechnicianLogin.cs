using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using mixpanel;
public class TechnicianLogin : MonoBehaviour
{
    [SerializeField] IPAddressRetriever addressRetriever;
    public Text brandName;
    public Button ogButton;
    [SerializeField] GameObject errorMsgPanel, idPanel;
    [SerializeField] Text deviceIdText;
    public GameObject profileContainer;
    public Profile[] profiles;
    public GameObject No_Technician;
    int selectedProfile = -1;
    [System.Serializable]
    public class Profile
    {
        public Button button;
        public Image image;
        public string imageURL;
        public Text nameT;

        public string fLName, userId, Email;
        public Profile(string uName, string uId, string uEmail, string Url)
        {
            this.fLName = uName;
            this.userId = uId;
            this.Email = uEmail;
            this.imageURL = Url;
            if (nameT)
                this.nameT.text = uName;
        }
    }
    public Button[] profileButtons;
    public GameObject[] enableOnSelection;
    public GameObject[] disableOnSelection;
    public Image LoginImage;
    public Text loginName;
    public InputField passwordIp;
    public Text profileName;
    public Image LoginProfileImage;
    [SerializeField]
    public AppApiManager.VehicleMasters masters;
    [SerializeField] AppApiManager apiManager;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] ScreenLinker linker;
    [SerializeField] bool testNow = false;
    [SerializeField] string testJson = "";
    bool shownDisclaimer = false;
    bool shownTutorial = false;
    [SerializeField] bool shownOpenTickets = false;
    bool recievedVehicleMasters = false;
    public Sprite Image;
    // public Scrollbar ticketScrollbar;
    AppApiManager.VehicleCluster[] Cluster;
    AppApiManager.Technician[] technicians;



    /* public List<Texture2D> Profile_Picture = new List<Texture2D>();
     public List<string> Profile_Pic_Urls = new List<string>();*/
    // Start is called before the first frame update
    /* public void DownloadIm()
      {
          Debug.Log("  Profile Picture   " + Profile_Picture.Count);
          for (int i = 0; i < profileContainer.transform.childCount; i++)
          {
              Debug.Log("  profile Container  " + profileContainer.transform.GetChild(i).name);
              Sprite Image = Sprite.Create(Profile_Picture[i], new Rect(0, 0, Profile_Picture[i].width, Profile_Picture[i].height), new Vector2(0, 0));
              profileContainer.transform.GetChild(i).GetComponent<Image>().sprite = Image;
          }
      }*/

    private void Start()
    {
        for (int i = 0; i < profiles.Length; i++)
        {
            StartCoroutine(DownloaderProfilePic(profiles[i].imageURL, profiles[i].userId));
        }

        Debug.Log("Unique ID   ==  " + apiManager.GetUniqueId());   //Get a mac address of the device

        if (profiles.Length == 0)
        {
            No_Technician.SetActive(true);
            Debug.Log(" No profile available ");
        }
        else
        {
            No_Technician.SetActive(false);
            Debug.Log("Technician Profile Available");
        }
        if (testNow)
        {
            testNow = false;
            OnVehicleMasters(testJson);
        }
    }

    public void ShowLoginOptions(int b)
    {
        selectedProfile = b;
        for (int i = 0; i < profileButtons.Length; i++)
        {
            if (i == b)
            {
                profileButtons[i].Select();
                Text profileName = profileButtons[i].GetComponentInChildren<Text>();
                Image SelectedImage = profileButtons[i].GetComponent<Image>();
                if (profileName)
                {
                    Debug.Log("Selected: " + profileName.text.ToString());
                    Mixpanel.Track("User_Selected ", "username", profileName.text.ToString());
                    loginName.text = profileName.text.ToString();
                    LoginImage.sprite = SelectedImage.sprite;
                }
            }
            profileButtons[i].gameObject.SetActive(i == b);
        }
        SetState(enableOnSelection, true);
        SetState(disableOnSelection, false);

    }

    public void ForgotPass()
    {
        apiManager.OnForgot += ApiManager_Forgot;
        // Debug.Log("Profile Email..............."+profiles[selectedProfile].Email);
        apiManager.ForgotPassword(profiles[selectedProfile].userId, profiles[selectedProfile].Email);
    }

    //call the ApiManager_OnLogin function using binding (apiManager.OnLogin += ApiManager_OnLogin;)
    public void Login()
    {
        apiManager.OnLogin += ApiManager_OnLogin;
        apiManager.Login(profiles[selectedProfile].userId, passwordIp.text);
    }

    string bName;
    private int aa;

    public void ShowBrandName()
    {
        bName = apiManager.serverData.brand.ToString();
        // Debug.LogError("BNM" + bName);
        brandName.text = bName;
    }

    public void Logout()
    {
        //apiManager.OnLogout += ApiManager_OnLogout;
        //apiManager.Logout();
        Debug.Log("LOGOUT......");
    }
    private void OnEnable()
    {
        if (selectedProfile >= 0)
        {
            screenManager.SelectScreen(ScreenLinker.ScreenID.PROFILE);
            profileName.text = profiles[selectedProfile].fLName;
        }
    }

    private void ApiManager_Forgot(string text)
    {
        apiManager.OnForgot -= ApiManager_Forgot;
        screenManager.Toast("Password Reset Link Share To Your Email" + text);

    }
    private void ApiManager_OnLogout(string text)
    {
        // apiManager.OnLogout -= ApiManager_OnLogout;
        // screenManager.Toast("User Logout" + text);
        Debug.Log("LOGOUT......");
        SceneManager.LoadScene(0);
    }

    private void ApiManager_OnLogin(string text)
    {
        apiManager.OnLogin -= ApiManager_OnLogin;
        Debug.Log("On Login Response: " + text);
        // Mixpanel.Track("Login_Attempted", Application.version);
        Mixpanel.Track("Login_Attempted", "app_verion", Application.version);


        if (text.ToLower().Contains("success"))
        {
            Mixpanel.Track("Success");
            Mixpanel.Identify(profiles[selectedProfile].Email);

            Profile curProf = profiles[selectedProfile];

            Mixpanel.People.Email = curProf.Email;
            Mixpanel.People.Name = curProf.fLName;

            screenManager.DebugToScreen("Logged in:" + profiles[selectedProfile].fLName);
            //    screenManager.SelectScreen(ScreenLinker.ScreenID.ELSA_DISCLAIMER);
            screenManager.SelectScreen(ScreenLinker.ScreenID.PROFILE);
            profileName.text = profiles[selectedProfile].fLName;
            LoginProfileImage.sprite = profiles[selectedProfile].image.sprite;
            //apiManager.GetOpenTickets(OnOpenTickets);                                
            apiManager.GetEscalatedTickets(OnEscalatedTickets);
            apiManager.GetReassignedTickets(OnReassignedTickets);
            apiManager.GetResolvedTickets(OnResolvedTickets);

            linker.ShortListOfTickets();
            Debug.Log("Fetching All Tickets");
            //linker.ShowEscalatedTickets();
        }
        else
        {
            Mixpanel.Track("Failed");
            screenManager.Toast(text);
            if (text.ToLower().Contains("fail"))
            {
                Debug.Log("failed");
            }
        }
    }



    public bool DidShowDisclaimer() => shownDisclaimer;

    //
    public void ShowDisclaimerIfNotShown()
    {
        if (!shownDisclaimer)
        {
            shownDisclaimer = true;
            screenManager.SelectScreen(ScreenLinker.ScreenID.ELSA_DISCLAIMER);
        }
        else
        {
            if (!shownTutorial)
            {
                shownTutorial = true;
                screenManager.SelectScreen(ScreenLinker.ScreenID.TUTORIAL_BEGIN);
            }
            else
            {
                //if (!shownOpenTickets)
                //{
                if (linker.HasOpenTickets())
                {
                    //ShowOpenTickets();                    
                    linker.ShortListOfTickets();
                    PopulateAllTicketInEscalate();
                    Mixpanel.Track("Open_Ticket_Loaded", "tickets", linker.GetOpenTickets().Count);
                    shownOpenTickets = true;
                }
                else
                    screenManager.SelectScreen(ScreenLinker.ScreenID.VIN_INPUT);
                //}
            }
            //else
            //    if (HasCurrentOpenTicket())
            //    linker.OnTicketResume();
            //  screenManager.SelectScreen(ScreenLinker.ScreenID.PROCESS_SELECTION);
            //else
            //    screenManager.SelectScreen(ScreenLinker.ScreenID.VIN_INPUT);
        }
    }

    //Get available vehicle detials from the server
    public void GetVehicleMasters()
    {
        if (!recievedVehicleMasters)
            apiManager.GetVehicleMasters(OnVehicleMasters);
        apiManager.GetVehicleMastersForTorque(OnVehicleMasterWithModulesForTorque);
    }

    void OnVehicleMasterWithModulesForTorque(string arg0)
    {

        //linker.GetScreenManager().SetLoadingState(false);
        ManualSelector.mastersForTorque = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        //linker.UpdateVehicleMasters(masters);
    }

    //Get open ticket from the server
    public void GetOpenTickets()
    {
        Debug.Log("Getting Open Tickets");
        /*      if (shownOpenTickets)
              {*/
        Debug.Log("Get Open Tickets!");
        apiManager.GetOpenTickets(OnOpenTickets);
        /*   }  */

    }

    //Show open tickect on the screen 
    public void PopulateOpenTickets()
    {

        //Debug.Log("PopulateOpenTickets........");
        if (linker.GetOpenTickets() != null)
        {
            if (linker.GetOpenTickets().Count > 0)
            {
                linker.ShowOpenTickets();
            }
            else
            {
                GetOpenTickets();
            }

        }
        else
            GetOpenTickets();
    }

    //Show escalated tickect on the screen 
    public void PopulateEscalatedTickets()
    {

        if (linker.GetEscalatedTickets() != null)
        {
            if (linker.GetEscalatedTickets().Count > 0)
            {
                Debug.LogError("Get Escalated ....." + linker.GetEscalatedTickets().Count);
                //linker.ShowEscalatedTickets();
            }
            else
                GetEscalatedTickets();
        }
        else
            GetEscalatedTickets();
    }

    //Show close tickect on the screen 
    public void PopulateCloseTickets()
    {

        Debug.Log("PopulateCloseTickets........");
        if (linker.GetCloseTickets() != null)
        {
            if (linker.GetCloseTickets().Count > 0)
            {
                linker.ShowCloseTickets();
            }
            else
            {
                GetCloseTicket();
            }

        }
        else
            GetCloseTicket();
    }



    /// <summary>
    /// Dhiraj 25-4-2024 Get and Show Resolved and Reassigned Ticket to application
    /// </summary>

    public void PopulateAllTicketInEscalate()
    {
        if (linker.GetNewEscalatedTickets().Count > 0)
        {
            Debug.LogError("Get Escalated ....." + linker.GetNewEscalatedTickets().Count);
            linker.isSorted = false;
            linker.TryProcessTicketsAndShow();
            //screenManager.SelectScreenIfNotSelected(ScreenLinker.ScreenID.OPEN_TICKETS);
        }
        else
        {
            GetEscalatedTickets();
            GetReassignedTickets();
            GetResolvedTickets();

            // Add this line after all ticket fetching is guaranteed to be complete
            linker.TryProcessTicketsAndShow();
        }

    }

    //Show reassigned tickect on the screen 
    public void PopulateReassignedTickets()
    {

        if (linker.GetReassignedTickets() != null)
        {
            if (linker.GetReassignedTickets().Count > 0)
            {
                Debug.LogError("Get Escalated ....." + linker.GetReassignedTickets().Count);
                //linker.ShowReassignedTickets();
            }
            else
                GetReassignedTickets();
        }
        else
            GetReassignedTickets();
    }

    //Show resolved tickect on the screen 
    public void PopulateResolvedTickets()
    {
        Debug.Log("PopulateCloseTickets........");
        if (linker.GetResolvedTickets() != null)
        {
            if (linker.GetResolvedTickets().Count > 0)
            {
                //linker.ShowResolvedTickets();
            }
            else
            {
                GetResolvedTickets();
            }

        }
        else
            GetResolvedTickets();
    }



    //Get reassigned ticket from the server
    public void GetReassignedTickets()
    {
        Debug.Log("Getting Reassigned Tickets");
        if (shownOpenTickets)
        {
            Debug.Log("Get Reassigned Tickets!");
            apiManager.GetReassignedTickets(OnReassignedTickets);
            screenManager.SetLoadingState(true, () => screenManager.SetLoadingState(false));
        }

    }
    //Get resolved ticket from the server
    public void GetResolvedTickets()
    {
        Debug.Log("Getting Resolved Tickets");
        if (shownOpenTickets)
        {
            Debug.Log("Get Resolved Tickets!");
            apiManager.GetResolvedTickets(OnResolvedTickets);
            screenManager.SetLoadingState(true, () => screenManager.SetLoadingState(false));
        }

    }


    //Get close ticket from the server
    public void GetCloseTicket()
    {
        if (shownOpenTickets)
        {
            Debug.Log("Get Closed Tickets!");
            apiManager.GetClosedTickets(OnCloseTickets);
            screenManager.SetLoadingState(true, () => screenManager.SetLoadingState(false));
        }

    }

    //Get escalated ticket from the server
    public void GetEscalatedTickets()
    {
        Debug.Log("Getting Escalated Tickets");
        if (shownOpenTickets)
        {
            Debug.Log("Get Escalated Tickets!");
            apiManager.GetEscalatedTickets(OnEscalatedTickets);
            screenManager.SetLoadingState(true, () => screenManager.SetLoadingState(false));
        }

    }

    //After Login
    public void OnProfileContinue()
    {
        Debug.Log("Continue on profile");
        GetVehicleMasters();  //
        if (!shownOpenTickets)
            apiManager.GetOpenTickets(OnOpenTickets);  //Fetch Open tickets thorugh the API 
        //       if (screenManager.GetCurrentScreenIndex() != ScreenLinker.ScreenID.OPEN_TICKETS)
        if (!shownDisclaimer || !shownTutorial || !shownOpenTickets)
            ShowDisclaimerIfNotShown();
        else
                   if (HasCurrentOpenTicket())
            linker.OnTicketResume();
        //    screenManager.SelectScreen(ScreenLinker.ScreenID.TUTORIAL_BEGIN);
    }

    //Get vehicle details and show it on different UI
    private void OnVehicleMasters(string arg0)
    {
        Debug.Log("<color=blue>[VEH-MSTRS]</color> " + arg0, gameObject);
        masters = AppApiManager.GetServerResponseVehicleMasters(arg0).response_data;
        Cluster = masters.clusters;
        linker.UpdateVehicleMasters(masters);

        if (masters != null)
        {
            if (masters.clusters != null)
                if (masters.clusters.Length > 0)
                {
                    recievedVehicleMasters = true;
                    apiManager.SetClusters(masters.clusters);
                    linker.LoadClusterForTicket();
                }
            if (masters.variant_types != null)
            {
                // Debug.LogError( " MODELS " +masters.models);
                Debug.Log("linker.GetManualSelector().SetVariantTypes(masters.variant_types);");
                linker.GetManualSelector().SetVariantTypes(masters.variant_types);
                // linker.GetManualSelector().SetModuleGroups(masters.models);

                linker.GetManualSelector().SetModel(new List<string>(masters.models));
            }
        }
    }


    public bool HasCurrentOpenTicket()
    {
        return linker.IsCurrentTicketOpen();
    }

    //Get the response sucessfully from the server for open ticket
    void OnOpenTickets(string response)
    {
        Debug.Log("   Open ticket    " + response);
        AppApiManager.ServerResponse serverResp = AppApiManager.GetServerResponse(response);
        List<AppApiManager.Ticket> tickets = serverResp.response_data.tickets;
        List<AppApiManager.Ticket> vehicleTickets = new List<AppApiManager.Ticket>();
        for (int i = 0; i < tickets.Count; i++)
        {
            if (tickets[i].TicketType.ToLower().Contains("vehicle"))
            {
                Debug.Log(" Contains Vehicle");
            }
            vehicleTickets.Add(tickets[i]);
            //Debug.LogError(tickets[i].ShortDescription);
        }

        Debug.Log("<color=blue>Open Ticket:</color>" + tickets.Count);
        if (vehicleTickets.Count > 0)
        {
            linker.SaveOpenVehicleTickets(vehicleTickets);

            linker.GetScreenManager().DebugToScreen("Loaded Open Tickets");
            //if (screenManager.GetCurrentScreenIndex() != ScreenLinker.ScreenID.OPEN_TICKETS)
            //    ShowDisclaimerIfNotShown();
            //    screenManager.SelectScreen(ScreenLinker.ScreenID.OPEN_TICKETS);
            //screenManager.GetDialog().Show("Open Tickets", "You have " + tickets.Count + " pending tickets",
            //"Open Tickets >", linker.ShowOpenTickets);
        }
    }

    //Get the response sucessfully from the server for escalated ticket
    void OnEscalatedTickets(string response)
    {

        AppApiManager.ServerResponse serverResp = AppApiManager.GetServerResponse(response);
        List<AppApiManager.Ticket> tickets = serverResp.response_data.tickets;
        List<AppApiManager.Ticket> vehEscltdTickets = new List<AppApiManager.Ticket>();
        for (int i = 0; i < tickets.Count; i++)
        {
            //if (tickets[i].TicketType.ToLower().Contains("vehicle"))
            vehEscltdTickets.Add(tickets[i]);
        }

        Debug.Log("<color=blue>Escalated Ticket:</color>" + tickets.Count + " TICKETS: " + response);
        if (vehEscltdTickets.Count > 0)
        {
            linker.SaveEscalatedVehicleTickets(vehEscltdTickets);
            //linker.ShowEscalatedTickets();
            linker.GetScreenManager().DebugToScreen("Loaded Escalated Tickets");
        }
    }



    /// <summary>
    //Get the response sucessfully from the server for reassigned ticket
    void OnReassignedTickets(string response)
    {
        if (screenManager.IsLoading())
            screenManager.SetLoadingState(false);
        AppApiManager.ServerResponse serverResp = AppApiManager.GetServerResponse(response);
        List<AppApiManager.Ticket> tickets = serverResp.response_data.tickets;
        List<AppApiManager.Ticket> vehEscltdTickets = new List<AppApiManager.Ticket>();
        for (int i = 0; i < tickets.Count; i++)
        {
            //if (tickets[i].TicketType.ToLower().Contains("vehicle"))
            vehEscltdTickets.Add(tickets[i]);
        }

        Debug.Log("<color=blue>Escalated Ticket:</color>" + tickets.Count + " TICKETS: " + response);
        if (vehEscltdTickets.Count > 0)
        {
            linker.SaveReassignedVehicleTickets(vehEscltdTickets);
            //linker.ShowReassignedTickets();
            linker.GetScreenManager().DebugToScreen("Loaded Reassigned Tickets");
        }
    }
    /// </summary>
    /// <param name="response"></param>
    /// 
    /// <summary>
    //Get the response sucessfully from the server for resolved ticket
    void OnResolvedTickets(string response)
    {
        if (screenManager.IsLoading())
            screenManager.SetLoadingState(false);
        AppApiManager.ServerResponse serverResp = AppApiManager.GetServerResponse(response);
        List<AppApiManager.Ticket> tickets = serverResp.response_data.tickets;
        List<AppApiManager.Ticket> vehEscltdTickets = new List<AppApiManager.Ticket>();
        for (int i = 0; i < tickets.Count; i++)
        {
            //if (tickets[i].TicketType.ToLower().Contains("vehicle"))
            vehEscltdTickets.Add(tickets[i]);
        }

        Debug.Log("<color=blue>Escalated Ticket:</color>" + tickets.Count + " TICKETS: " + response);
        if (vehEscltdTickets.Count > 0)
        {
            linker.SaveResolvedVehicleTickets(vehEscltdTickets);
            // linker.ShowResolvedTickets();
            linker.GetScreenManager().DebugToScreen("Loaded Resolved Tickets");
        }        
    }
    /// </summary>
    /// <param name="response"></param>


    //Get the response sucessfully from the server for close ticket
    void OnCloseTickets(string response)
    {
        if (screenManager.IsLoading())
            screenManager.SetLoadingState(false);
        Debug.Log("   Close ticked    " + response);
        AppApiManager.ServerResponse serverResp = AppApiManager.GetServerResponse(response);
        List<AppApiManager.Ticket> tickets = serverResp.response_data.tickets;
        List<AppApiManager.Ticket> vehicleTickets = new List<AppApiManager.Ticket>();
        for (int i = 0; i < tickets.Count; i++)
        {
            /*if (tickets[i].TicketType.ToLower().Contains("vehicle"))
            {
                Debug.Log(" Contains Vehicle");
             }*/
            vehicleTickets.Add(tickets[i]);
        }
        Debug.Log("<color=blue>Closed Ticket:</color>" + tickets.Count + " TICKETS: " + response);
        if (vehicleTickets.Count > 0)
        {
            Debug.Log("Vehicle Ticket ------->>>>>>>>>>      " + vehicleTickets.ToString());
            linker.SaveClosedVehicleTickets(vehicleTickets);
            linker.ShowCloseTickets();
            linker.GetScreenManager().DebugToScreen("Loaded Closed Tickets");
        }
    }
    public void ShowOpenTickets()
    {
        if (!shownDisclaimer)
        {
            shownDisclaimer = true;
            screenManager.SelectScreen(ScreenLinker.ScreenID.ELSA_DISCLAIMER);
        }
        else
            screenManager.SelectScreenIfNotSelected(ScreenLinker.ScreenID.OPEN_TICKETS);
    }
    void SetState(GameObject[] gs, bool state)
    {
        for (int i = 0; i < gs.Length; i++)
        {
            gs[i].SetActive(state);
        }
    }

    //Show available technician on the login page
    public void InitProfile()
    {
        //Mixpanel.Reset();
        Mixpanel.Track("User_Login_Initiated");
        Debug.Log("Initialised Profiles");

        screenManager.DebugToScreen("logging in Device...");
        //  apiManager.OnRecieveTechnicians += LoadProfiles;
        //   apiManager.GetTechnicians();
        apiManager.GetTechnicians(OnRecieveTechniciansInfo);

    }

    public void RegistrationCom()
    {
        Debug.LogError("Device_Registration_Completed");
    }

    //Get the data(response) from the API for Technician
    void OnRecieveTechniciansInfo(string resp)
    {
        Debug.Log("<color=blue>[SVR-RESP]</color>" + resp);
        AppApiManager.ServerResponse serverResponse = AppApiManager.GetServerResponse(resp);

        if (serverResponse.status.Contains("fail"))
        {
            if (errorMsgPanel.activeInHierarchy)
                errorMsgPanel.SetActive(false);

            idPanel.SetActive(true);
            deviceIdText.text = apiManager.GetUniqueId();
            Debug.Log(apiManager.GetUniqueId());
            Debug.Log(serverResponse.response_data.message);
            Debug.LogError("Device_Not_Registered");
        }
        else
        {
            Mixpanel.Track("Device_Already_Registered");
            LoadProfiles(serverResponse.response_data);
        }

    }

    //Show technician on login page
    public void LoadProfiles(AppApiManager.ServerData data)
    {
        aa++;
        Debug.Log("Loaded Profiles: " + data.ToString());
        apiManager.OnRecieveTechnicians -= LoadProfiles;
        technicians = data.technicians;
        if (data.technicians != null)
        {
            screenManager.DebugToScreen("Device login successful");
        }
        List<Profile> newProfiles = new List<Profile>();
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < profileContainer.transform.childCount; i++)
        {
            Destroy(profileContainer.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < technicians.Length; i++)
        {
            string techName = technicians[i].FirstName + " " + technicians[i].LastName;
            string userId = technicians[i].UserID;
            string Email_ID = technicians[i].Email;
            string Profile_Image_URL = technicians[i].ProfilePicture;
            // Profile_Pic_Urls.Add(technicians[i].ProfilePicture);
            // StartCoroutine(DownloaderProfilePic(Profile_Image_URL));

            Button b = Instantiate(ogButton, profileContainer.transform);
            Profile p = new Profile(techName, userId, Email_ID, Profile_Image_URL);
            p.button = b;
            int sel = i;
            b.onClick.AddListener(delegate
            {
                ShowLoginOptions(sel);
            });
            b.name = techName;
            p.image = b.GetComponent<Image>();
            p.nameT = b.GetComponentInChildren<Text>();
            p.nameT.text = p.fLName;
            newProfiles.Add(p);
            buttons.Add(b);
        }
        profiles = newProfiles.ToArray();
        profileButtons = buttons.ToArray();
        screenManager.SelectScreen(GetComponent<UIScreen>().index);
        /* if (profiles == null)
         {
             No_Technician.SetActive(true);
             Debug.Log(" No profile available ");
         }
         else
         {
             No_Technician.SetActive(false);
             Debug.Log("Technician Profile Available");
         }*/

    }
    IEnumerator DownloaderProfilePic(string Url, string ID)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(Url);
        yield return www.SendWebRequest();
        Texture2D myTexture = (Texture2D)((DownloadHandlerTexture)www.downloadHandler).texture;
        Sprite Imp1 = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0, 0));
        foreach (Profile P in profiles)
        {
            if (P.userId == ID)
            {
                Debug.Log("Profile  NAme " + P.nameT.text);
                P.image.sprite = Imp1;
            }
        }

        Mixpanel.Track("IPAddress", "ip_address", addressRetriever.ipAddress);
        //Imp.sprite = Imp1;
        //   Profile_Picture.Add(myTexture);
    }

    public void ViewAllProfiles()
    {
        for (int i = 0; i < profileButtons.Length; i++)
        {
            profileButtons[i].gameObject.SetActive(true);
        }
        SetState(enableOnSelection, false);
        SetState(disableOnSelection, true);
    }

    // Update is called once per frame
    void Update()
    {

        /* if (profiles.Length == 0)
         {
             No_Technician.SetActive(true);
             Debug.Log(" No profile available ");
         }
         else
         {
             No_Technician.SetActive(false);
             Debug.Log("Technician Profile Available");
         }
         if (testNow)
         {
             testNow = false;
             OnVehicleMasters(testJson);
         }*/
    }
}
