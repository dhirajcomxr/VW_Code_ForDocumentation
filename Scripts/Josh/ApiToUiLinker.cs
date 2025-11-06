using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ApiToUiLinker : MonoBehaviour
{
   [SerializeField] AppApiManager apiManager;
   [SerializeField] UIScreenManager screenManager;
    [Header("UI References")]
    [SerializeField] int totalTickets = -1, totalPausedTickets=-1;
    [SerializeField] ProfileScreen profile;
    [SerializeField] TicketScreen ticketScreen;
    [Header("Debug")]
    [SerializeField] List<AppApiManager.Ticket> tickets;

    [System.Serializable]
    struct ProfileScreen
    {
       public Text name, email, location,contact;
    }
    [System.Serializable]
    struct TicketScreen
    {
       public Text pausedTicketsCount, openTicketsCount;
        public ImageLabelList pausedTickets,openTickets;

    }
    
    private void OnApplicationQuit()
    {
        Debug.Log("Saving");
        apiManager.SaveData();
    }
    // Start is called before the first frame update
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
            Login();
   
    }
    void UpdateProfile()
    {
        profile.email.text = apiManager.GetProfileInfo().EmailID;
        profile.name.text = apiManager.GetProfileInfo().FirstName +" "
           + apiManager.GetProfileInfo().LastName;
        profile.contact.text = apiManager.GetProfileInfo().Mobile;
    }
    void Login()
    {
        Debug.Log("Login in to continue");
    }
    void GetTicketCounts()
    {
        apiManager.GetTickets();
        GetPausedTickets();
        GetTickets();
        ticketScreen.openTicketsCount.text =
            "" + ticketScreen.openTickets.GetCount();
     ticketScreen.pausedTicketsCount.text=
           ""+ ticketScreen.pausedTickets.GetCount();

    }
    public void GetPausedTickets()
    {
        ticketScreen.pausedTickets.Load(GetTicketList("pause").ToArray());
    }
  public void GetTickets()
    {
        ticketScreen.openTickets.Load(GetTicketList("open").ToArray());
    }
    // Update is called once per frame
    #region Helper
    List<string> GetTicketList(string ticketState)
    {
        tickets = apiManager.TicketList();
        List<string> ticketNameList = new List<string>();
        foreach (AppApiManager.Ticket t in tickets)
        {
            if (t.TicketStatus.ToLower().Contains(ticketState))
                ticketNameList.Add(t.ShortDescription);
        }
        return ticketNameList;
    }
    #endregion
}
