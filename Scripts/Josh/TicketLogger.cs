using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicketLogger : MonoBehaviour
{
    AppApiManager apiManager;
    string ticketId = "";
    // Start is called before the first frame update
    void Start()
    {
        AppLogger.onProcessLog += SendTicketLog;
        apiManager = FindObjectOfType<AppApiManager>();
    }

    private void SendTicketLog(KeyValuePair<string,string>[] keyValues)
    {
        Debug.Log("Got Log for !"+ticketId);
        if (apiManager)
        {
            if(apiManager.HasActiveTicket())
                apiManager.SaveTicketProgress(keyValues);

        }
    }
}
