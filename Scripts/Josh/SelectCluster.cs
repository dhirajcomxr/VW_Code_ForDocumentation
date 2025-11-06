using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCluster : MonoBehaviour
{
    // Start is called before the first frame update
    public List<AppApiManager.VehicleCluster> clusterlist;
    public Dropdown CL_Dropdown;
    public AppApiManager apiManager;
    [SerializeField] TechnicianLogin technicianLogin;
    //[SerializeField] AppApiManager.VehicleCluster[] clusters;
   // [SerializeField] AppApiManager.TicketDetails[] ticketDetails;
    public Text Comment;

    [SerializeField] Text toastText;
[SerializeField] GameObject toastPanel;
   // public Text kmText;
    public GameObject Submit;
    [SerializeField] ScreenLinker screenLinker;
   // [SerializeField] Text vinNum, technicianName;


    void OnEnable()
    { 
        var details  = apiManager.GetCurVehicleDetails();
       // vinNum.text = "VIN Number:" +details.VIN.ToString();
       // technicianName.text = "Technician Name: " + technicianLogin.profileName.text;
    }

    void Start()
    {
      //  clusters = apiManager.GetClusters();
  
        List<string> clusterNames = new List<string>();
        clusterNames.Add("Please Select Cluster");
        /*if (clusters.Length > 0)
        {
            for (int i = 0; i < clusters.Length; i++)
            {
                clusterNames.Add(clusters[i].ClusterName);
            }
        }*/
        CL_Dropdown.ClearOptions();
        CL_Dropdown.AddOptions(clusterNames);
    }

    /*    public void SelectClusterforTicket()
        {
            List<string> CLname = new List<string>();
            foreach(AppApiManager.VehicleCluster VC in clusterlist)
            {
                CLname.Add(VC.ClusterName);
            }
            CL_Dropdown.ClearOptions();
            CL_Dropdown.AddOptions(CLname);
        }*/
    //*  This function is used to escalate the ticket if a user doesn't provide the VIN number but for now the client requirment is to disable the ticket escalation button 
    public void save_Cluster(Text inputText)
    {
        if (!string.IsNullOrWhiteSpace(inputText.text))
        {
            screenLinker._escalationRemark = inputText.text;
            screenLinker.EscalateCurrentTicket();
            Debug.Log("Ticket escalated with comment not null");
        }
        else
        {
        ShowToast("Please add comment to proceed!");
        }
    
         
        //{
        //    if (kmText.text == "")
        //        screenManager.Toast("Please enter Kilometer");
        //    else
        //apiManager.EscalateCurrentTicket();

        //string id = "";
        //foreach (AppApiManager.VehicleCluster VC in clusters)
        //{
        // if (VC.ClusterName == CL_Dropdown.options[CL_Dropdown.value].text)
        //{
        // Debug.Log(VC.ClusterID + "  Cluster ID " + VC.ClusterName);
        // id = "'" + VC.ClusterID;
        //}
        //}
        // apiManager.GetTicketDetails();
        // apiManager.SaveTicketClusters(OnsaveCluster);
        //}
        //Debug.Log("KILO METER: " + kmText.text);
    }  
    
    void ShowToast(string message)
{
    toastText.text = message;
    toastPanel.SetActive(true);
    CancelInvoke(nameof(HideToast));
    Invoke(nameof(HideToast), 2f); // hides after 2 seconds
}

void HideToast()
{
    toastPanel.SetActive(false);
}
    

    private void OnsaveCluster(string arg)
    {
        Debug.Log("save_Cluster Cluster at End of DT" + arg);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
