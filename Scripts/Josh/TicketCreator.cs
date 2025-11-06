using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicketCreator : MonoBehaviour
{
    [SerializeField] AppApiManager apiManager;
    [SerializeField] ScreenLinker linker;
    [SerializeField] ImageLabelList clusterList;
    [SerializeField] TechnicianLogin Logine;
    public Text kilometer;
    // Start is called before the first frame update
    public List<int> selectedClusters;
  [SerializeField]  AppApiManager.VehicleCluster[] clusters;
    private void Reset()
    {
        apiManager = FindObjectOfType<AppApiManager>();
    }


    //cluster select and Short description option disable feedback from VW 
    public void CreateTicket(InputField issueInput)
    {
        Debug.Log("Creating Ticket");

        /*if (selectedClusters.Count < 1)
        {
            linker.GetScreenManager().Toast("Please Select Module to Create Ticket!");
            return;
        }
        if (issueInput.text.Length > 2 && kilometer.text != "")
            CreateTicket(issueInput.text);
        else
            linker.GetScreenManager().Toast("Please input kilometer and issue to Create Ticket!");
        */

        if (kilometer.text != "")
            CreateTicket(kilometer.text);
        else
            linker.GetScreenManager().Toast("Please input kilometer to Create Ticket!");
    }

    //
    public void CreateTicket(string issueInput)
    {
        if (issueInput.Length > 0)
        {
            apiManager.CreateTicket("Option Disable by default",issueInput,OnCreateTicket);
            apiManager.OnLogToScreen += linker.GetScreenManager().DebugToScreen;
        }
    }

    //On ticket created sucessfull
    private void OnCreateTicket(string arg0)
    {
        Debug.Log("ON CREATE TICKET: " + arg0);
        AppApiManager.ServerResponse resp = AppApiManager.GetServerResponse(arg0);
        if (resp.status.Contains("success"))
        {
            string id = AppApiManager.GetServerResponse(arg0).response_data.ticket_id;
            if (id.Length > 0)
            {
                linker.GetScreenManager().Toast("Your Ticket has been created!" + resp.message);
                Mixpanel.Track("New_Ticket_Created");
                apiManager.currentTicket.TicketID = id;
                linker.GetScreenManager().SelectScreen(ScreenLinker.ScreenID.PROCESS_SELECTION);
                SendClusterInfo();
                linker.GetTicketDetails(id, true);
            }
            else
                linker.GetScreenManager().Toast("Error Creating Ticket!: " + resp.message);
        }
        else
            linker.GetScreenManager().Toast("Error Creating Ticket!" + resp.message);

        Debug.Log("Populate Dropdown");
        Logine.GetOpenTickets();
    }
   public void ResetClusters()
    {
        selectedClusters = new List<int>();
        for (int i = 0; i < clusterList.transform.childCount; i++)
        {
            ImageLabelListElement el = clusterList.transform.GetChild(i).GetComponent<ImageLabelListElement>();
            if (el)
            {
                UnityEngine.UI.Button btn = el.button;
                if (btn != null)
                {
                    btn.image.color = btn.colors.normalColor;
                    el.label.color = btn.colors.highlightedColor;
                }
            }
        }
    }
    public void SelectCluster(int listId)
    {
        Debug.Log("Selected Cluster "+listId);
        if (clusterList != null)
        {
            ImageLabelListElement el = clusterList.transform.GetChild(listId).GetComponent<ImageLabelListElement>();
            if (el)
            {
                UnityEngine.UI.Button btn = el.button;
                if (btn != null)
                {

                    if (selectedClusters == null)
                        selectedClusters = new List<int>();

                    bool found = false;
                    for (int i = 0; i < selectedClusters.Count; i++)
                    {
                        if (listId == selectedClusters[i])
                        {
                            btn.image.color = btn.colors.normalColor;
                            el.label.color = btn.colors.highlightedColor;
                            selectedClusters.RemoveAt(i);
                           
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        selectedClusters.Add(listId);
                        btn.image.color = btn.colors.highlightedColor;
                        el.label.color = btn.colors.normalColor;
                    }

                }
            }
            else
                Debug.LogError("No Row element found!");
        }
    }
    public void LoadClusterUI()
    {
        if (clusterList)
            LoadClustersToList(clusterList);
    }
    public void LoadClustersToList(ImageLabelList list)
    {
        if (list != null)
        {
            clusterList = list;
             clusters = apiManager.GetClusters();
            List<string> clusterNames = new List<string>();
            if (clusters.Length > 0)
            {
                for (int i = 0; i < clusters.Length; i++)
                {
                    clusterNames.Add(clusters[i].ClusterName);
                }
            }
            clusterList.Load(clusterNames.ToArray());
        }
    }

    //Options are disable due VW feedback
    public void SendClusterInfo()
    {
        string ids = "Option Disable by default";
        string SelClusName = "";

        List<Value> vals = new List<Value>();
      
        for (int i = 0; i < selectedClusters.Count; i++)
        {
            if (i > 0)
                ids += ",";
            ids += "'" + clusters[selectedClusters[i]].ClusterID + "'";
            SelClusName += " " + clusters[selectedClusters[i]].ClusterName + ",";
            vals.Add(new Value(clusters[selectedClusters[i]].ClusterName));
        }

        Debug.Log("<color=red>[DBG]</color>" + ids);
      //  Mixpanel.Track("Cluster_Selected: " + SelClusName);
      
        Mixpanel.Track("Cluster_Selected","clusters",new Value(vals));

        
        apiManager.SaveTicketClusters(ids,OnClusterSave);
    }

    private void OnClusterSave(string arg0)
    {
        Debug.Log("Saved Clusters: " + arg0);
    }
}
