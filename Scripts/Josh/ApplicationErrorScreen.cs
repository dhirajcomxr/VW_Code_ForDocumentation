using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class ApplicationErrorScreen : MonoBehaviour
{
   [SerializeField] AppApiManager apiManager;
    [SerializeField] UIScreenManager screenManager;
    [SerializeField] UIScreen screen;
    [SerializeField] InputField issueIp;
    [SerializeField] public Texture2D errorScreenshot;
    [SerializeField] GameObject SubmitButton;
    [SerializeField] GameObject showSreenshot;
    // Start is called before the first frame update
    private void Reset()
    {
        screen = GetComponent<UIScreen>();
        screenManager = FindObjectOfType<UIScreenManager>();
        apiManager = FindObjectOfType<AppApiManager>();
    }
    public void OnError()
    {
      //  issueIp.Select();
        issueIp.text = "";
        Debug.Log("Captured Screenshot!"); 
        StartCoroutine(CaptureScreenShotAsTxture());
        //errorScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
        screenManager.SelectScreen(screen.index);
       
        // StartCoroutine(CaptureScreen(1f));
    }


    IEnumerator CaptureScreenShotAsTxture()
    {
        yield return new WaitForEndOfFrame();
        errorScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Texture2D tex = new Texture2D(errorScreenshot.width, errorScreenshot.height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, errorScreenshot.width,errorScreenshot.height), 0, 0);
        tex.Apply();
        Debug.Log(tex + " texture........");
        showSreenshot.GetComponent<RawImage>().texture = tex;
        yield return null;
    }


    /*  private IEnumerator CaptureScreen(float waitTime)
      {

          yield return new WaitForSeconds(waitTime);

          print("WaitAndPrint " + Time.time);

      }*/
    public Text selectType;

    public void getSelectTypeValue()
    {
        Debug.LogError(selectType.text);
    }

/// Dhiraj 6-6-2024 Screen Linket reference to execute email method
    [SerializeField] ScreenLinker screenLinker;
/// 

    public void CreatManTechTicket()
    {
      //  apiManager.CreateManTechTicket(issueIp.text, errorScreenshot.EncodeToJPG(), "technical", OnCreateManTechTicket);
    }

    //Create IT/System Ticket
    public void SubmitError()
    {
        if (issueIp.text != null)
        {
           // Debug.LogError(AppApiManager.UserDataKV.ticket_type);
            Debug.Log("  Error ScreenShot    " + errorScreenshot);
            Mixpanel.Track("Application_Feedback_Submitted");
            //Debug.LogError(selectType.text);
            /// Dhiraj | 4-6-2024 || Create reassign ticket condition
            if (selectType.text == "System")
            {
                apiManager.CreateSystemTicket(issueIp.text, errorScreenshot.EncodeToJPG(), selectType.text, OnCreateSystemTicket);
                
                Debug.Log("System Ticket got Created");
            }
            else
            {
                apiManager.CreateITTicket(issueIp.text, errorScreenshot.EncodeToJPG(), selectType.text, OnCreateITTicket);
                
                Debug.Log("IT Ticket got Created");
            }
            ///
            screenManager.Toast("Sending...");
        
        }
    }
    [System.Serializable]
   public class SysRespData 
    {
        public string ticket_id, message;
}
    [System.Serializable]
   public class SysResp : AppApiManager.Response
    {
        public SysRespData response_data;
    }
    private void OnCreateSystemTicket(string arg0)
    {     
        SysResp curSysTicket = JsonUtility.FromJson<SysResp>(arg0);
        Debug.Log("Created System Ticket with ID:" + curSysTicket.response_data.ticket_id);
        screenManager.Back();
        screenManager.Toast("Report sent");
       // apiManager.EscalateTicket(curSysTicket.response_data.ticket_id, OnEscalateSysTicket);
        
        /// Dhiraj 6-6-2024 Get the newly created ticket details and store it to API Manager script aslo send the same to EMAIL by using Screen linker
        GetTicketDetails(arg0);
        screenLinker.EscalateCurrentTicket();
        ///
        //  Debug.Log("System Report Sent!: " + arg0);
       
    }

    private void OnEscalateSysTicket(string arg0)
    {
        Debug.Log("Escalated System Ticket: " + arg0);
    }

    /// <summary>
    /// Dhiraj 
    /// 4-6-3034
    /// On It Ticket creation
    /// </summary>
    /// <param name="arg0"></param>
    /// Start
    private void OnCreateITTicket(string arg0)
    {
        Debug.Log("Dhiraj : data" + arg0);
        SysResp curSysTicket = JsonUtility.FromJson<SysResp>(arg0);
        Debug.Log("Created IT Ticket with ID:" + curSysTicket.response_data.ticket_id);
        screenManager.Back();
        screenManager.Toast("Report sent");                
        apiManager.ReassignTicket(curSysTicket.response_data.ticket_id, OnReassignITTicket);
        GetTicketDetails(arg0);
        screenLinker.ReassignCurrentITTicket();
        //  Debug.Log("System Report Sent!: " + arg0);
    }


    private void OnReassignITTicket(string arg0)
    {
        Debug.Log("Escalated IT Ticket: " + arg0);
    }

    public void GetTicketDetails(string arg0)
    {
        AppApiManager.ServerResponse resp = AppApiManager.GetServerResponse(arg0);
        Debug.Log("Dhiraj : data" + resp);
        if (resp.status.Contains("success"))
        {
            string id = AppApiManager.GetServerResponse(arg0).response_data.ticket_id;
            if (id.Length > 0)
            {
                apiManager.currentTicket.TicketID = id;
                screenLinker.GetTicketDetails(id, true);
            }
            else
                screenLinker.GetScreenManager().Toast("Error Creating Ticket!: " + resp.message);
        }

    }


    /// END

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (issueIp.text == "")
        {
            SubmitButton.SetActive(false);

        }else if(issueIp.text != "")
        {
            SubmitButton.SetActive(true);
        }
      
    }
}
