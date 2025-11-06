using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterUI : MonoBehaviour
{
    public GameObject Splash;
    public GameObject Login;
    public GameObject CreateAccount;
    public GameObject Dashboard;
    public GameObject Resume;
    public GameObject OpenTicket;
    public GameObject ResolvedTicket; 
    public GameObject ResolvedTicket1; 
    public GameObject Explore;
    public GameObject Setting;
    public GameObject NewTicket1;
    public GameObject NewTicket2;
    public GameObject NewTicket3;
    public GameObject RnR;
    //public GameObject RnR_PS;
    public GameObject Dropdown;
    public GameObject Diagnostic;
    public GameObject UploadComplaint;
    public GameObject ProblemSolved;
    public GameObject Success;
    public GameObject Error;
    public GameObject Upload;
    public GameObject RnRView;
    public GameObject ExploreZones;
    public GameObject OilSump1;
    public GameObject OilSump2;
    public GameObject Steps;
    public GameObject EngineRnR;
    public GameObject Wiring;
    public GameObject Request;
    public GameObject Chat;
    public GameObject Text;
    public GameObject Left;
    public GameObject Top;
    public Button custom;
    
    private Text temp;
    private string temp1;
    private bool a;

    void Awake()
    {
        temp =  Text.GetComponent<Text>();
    }

    void Start()
    {
        Invoke("LoginFromSplash", 2);
    }

    public static void SetTransparency(Image p_image, float p_transparency)
    {
        if (p_image != null)
        {
            UnityEngine.Color __alpha = p_image.color;
            __alpha.a = p_transparency;
            p_image.color = __alpha;
        }
    }

    public void LoginFromSplash()
    {
        Splash.SetActive(false);
        Login.SetActive(true);
    }

    public void DontHaveAccount()
    {
        Login.SetActive(false);
        CreateAccount.SetActive(true);
    }

    public void LoginFromLogin()
    {
        Login.SetActive(false);
        Dashboard.SetActive(true);
        Debug.Log("Login");
    }

    public void BackFromCreateAccount()
    {
        CreateAccount.SetActive(false);
        Login.SetActive(true);
    }

    public void ContinueFromCreateAccount()
    {
        CreateAccount.SetActive(false);
        Dashboard.SetActive(true);
    }

    public void ExploreFromDashboard()
    {
        Dashboard.SetActive(false);
        Explore.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExplore);
        temp.text = "Explore";
        
    }

    public void BackFromExplore()
    {
        Dashboard.SetActive(true);
        Explore.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void SettingFromDashboard()
    {
        Dashboard.SetActive(false);
        Setting.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSetting);
        temp.text = "Setting";
        
    }

    public void BackFromSetting()
    {
        Dashboard.SetActive(true);
        Setting.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void DashboardFromPanel()
    {
        Dashboard.SetActive(true);
        Explore.SetActive(false);
        Setting.SetActive(false);
        //profile.SetActive(false);
        NewTicket1.SetActive(false);
        NewTicket2.SetActive(false);
        NewTicket3.SetActive(false);
        Resume.SetActive(false);
        OpenTicket.SetActive(false);
        ResolvedTicket.SetActive(false);
        RnR.SetActive(false);
        Wiring.SetActive(false);
        Request.SetActive(false);
        Dropdown.SetActive(false);
        Diagnostic.SetActive(false);
        ProblemSolved.SetActive(false);
        Success.SetActive(false);
        Error.SetActive(false);
        Upload.SetActive(false);
        RnRView.SetActive(false);
        ExploreZones.SetActive(false);
        Steps.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void SettingFromPanel()
    {
        Setting.SetActive(true);
        Explore.SetActive(false);
        /*profile.SetActive(false);
        NewTicket1.SetActive(false);
        NewTicket2.SetActive(false);
        NewTicket3.SetActive(false);
        Resume.SetActive(false);
        OpenTicket.SetActive(false);
        ResolvedTicket.SetActive(false);
        RnR.SetActive(false);
        Dropdown.SetActive(false);
        Diagnostic.SetActive(false);
        ProblemSolved.SetActive(false);
        Success.SetActive(false);
        Error.SetActive(false);
        Upload.SetActive(false);
        RnRView.SetActive(false);
        ExploreZones.SetActive(false);
        Steps.SetActive(false); */
        //Left.SetActive(true);
        //Top.SetActive(true);
        //Text.SetActive(true);
        //temp1 = custom.onClick.GetPersistentMethodName(0);
        //Debug.Log(temp1);
        //custom.onClick.RemoveAllListeners();
        temp.text = "Setting";
        custom.onClick.AddListener(DeactivateSetting);
    }

    public void ExploreFromPanel()
    {
        Explore.SetActive(true);
        Setting.SetActive(false);
        /*profile.SetActive(false);
        NewTicket1.SetActive(false);
        NewTicket2.SetActive(false);
        NewTicket3.SetActive(false);
        Resume.SetActive(false);
        OpenTicket.SetActive(false);
        ResolvedTicket.SetActive(false);
        RnR.SetActive(false);
        Dropdown.SetActive(false);
        Diagnostic.SetActive(false);
        ProblemSolved.SetActive(false);
        Success.SetActive(false);
        Error.SetActive(false);
        Upload.SetActive(false);
        RnRView.SetActive(false);
        ExploreZones.SetActive(false);
        Steps.SetActive(false); */
        custom.onClick.AddListener(DeactivateExplore);
        temp.text = "Explore";
    }

    public void DeactivateSetting()
    {
        Setting.SetActive(false);
        //custom.onClick.AddListener(temp1);
    }

    public void DeactivateExplore()
    {
        Explore.SetActive(false);
    }

    public void WiringFromExplore()
    {
        Wiring.SetActive(true);
        Explore.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromWiring);
        temp.text = "Wiring Harness Screen";
    }

    public void BackFromWiring()
    {
        Wiring.SetActive(false);
        Explore.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExplore);
        temp.text = "Explore";
    }

    public void RequestHelp()
    {
        Request.SetActive(true);
        Explore.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRequestHelp);
        temp.text = "Request Help";
    }

    public void BackFromRequestHelp()
    {
        Request.SetActive(false);
        Explore.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExplore);
        temp.text = "Explore";
    }

    public void ChatFromRequest()
    {
        Chat.SetActive(true);
        Request.SetActive(false);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromChat);
        temp.text = "Chat";
    }

    public void BackFromChat()
    {
        Chat.SetActive(false);
        Request.SetActive(true);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRequestHelp);
        temp.text = "Request Help";
    }

    public void RaiseIssue()
    {
        Error.SetActive(true);
        Request.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromError);
        temp.text = "Error";
    }

    public void NewTicket1Pressed()
    {
        Dashboard.SetActive(false);
        NewTicket1.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket1);
        temp.text = "New Ticket 1";
        
    }

    public void BackFromNewTicket1()
    {
        Dashboard.SetActive(true);
        NewTicket1.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void ResumePressed()
    {
        Dashboard.SetActive(false);
        Resume.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResume);
        temp.text = "Resume";
        
    }

    public void BackFromResume()
    {
        Dashboard.SetActive(true);
        Resume.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void ResumeTicketPressed()
    {
        EngineRnR.SetActive(true);
        Resume.SetActive(false);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResumeTicketPressed);
        temp.text = "Engine R&R - Oil Filter (DS)";
    }

    public void BackFromResumeTicketPressed()
    {
        EngineRnR.SetActive(false);
        Resume.SetActive(true);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResume);
        temp.text = "Resume";
    }

    public void OpenTicketPressed()
    {
        Dashboard.SetActive(false);
        OpenTicket.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOpenTicket);
        temp.text = "Open Ticket";
        
    }

    public void BackFromOpenTicket()
    {
        Dashboard.SetActive(true);
        OpenTicket.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void ResolvedTicketPressed()
    {
        Dashboard.SetActive(false);
        ResolvedTicket.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResolvedTicket);
        temp.text = "Resolved Ticket";
        
    }

    public void BackFromResolvedTicket()
    {
        Dashboard.SetActive(true);
        ResolvedTicket.SetActive(false);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
    }

    public void TicketFromResolvedTicketPressed()
    {
        ResolvedTicket.SetActive(false);
        ResolvedTicket1.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResolvedTicket1);
    }

    public void BackFromResolvedTicket1()
    {
        ResolvedTicket1.SetActive(false);
        ResolvedTicket.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromResolvedTicket);
    }

    public void StartFromNewTicket1Pressed()
    {
        NewTicket1.SetActive(false);
        NewTicket2.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket2);
        temp.text = "New Ticket 2";

    }

    void BackFromNewTicket2()
    {
        NewTicket1.SetActive(true);
        NewTicket2.SetActive(false);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket1);
        temp.text = "New Ticket 1";
    }

    public void StartFromNewTicket2Pressed()
    {
        NewTicket2.SetActive(false);
        NewTicket3.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket3);
        temp.text = "New Ticket 3";
    }

    public void BackFromNewTicket3()
    {
        NewTicket2.SetActive(true);
        NewTicket3.SetActive(false);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket2);
        temp.text = "New Ticket 2";
    }
    
    public void RnRPressed()
    {
        a = true;
        NewTicket3.SetActive(false);
        RnR.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnR);
        temp.text = "R&R";
    }

    public void BackFromRnR()
    {
        NewTicket3.SetActive(true);
        RnR.SetActive(false);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket3);
        temp.text = "New Ticket 3";
    }

    public void PictorialSearchPressed()
    {
        a = false;
        b = true;
        NewTicket3.SetActive(false);
        RnR.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnR_PS);
        temp.text = "R&R (PS)";
    }

    public void BackFromRnR_PS()
    {
        NewTicket3.SetActive(true);
        RnR.SetActive(false);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket3);
        temp.text = "New Ticket 3";
    }

    public void DropdownPressed()
    {
        Dropdown.SetActive(true);
        NewTicket3.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromDropdownPressed);
        temp.text = "Dropdown Search";
    }

    public void BackFromDropdownPressed()
    {
        NewTicket3.SetActive(true);
        Dropdown.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromNewTicket3);
        temp.text = "New Ticket 3";
        
    }

    public void StartFromDropdownPressed()
    {
        Dropdown.SetActive(false);
        Diagnostic.SetActive(true);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromDiagnostic);
        temp.text = "Complaint — Engine RPM not raising (DTMOT0001-004)";

    }

    public void BackFromDiagnostic()
    {
        Dropdown.SetActive(true);
        Diagnostic.SetActive(false);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromDropdownPressed);
        temp.text = "Dropdown Search";
    }

    public void YesFromDiagnostic()
    {
        Diagnostic.SetActive(false);
        UploadComplaint.SetActive(true);
        //Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromUploadComplaint);
        //temp.text = "Close Ticket";
    }

    public void ContinueFromUploadComplaint()
    {
        UploadComplaint.SetActive(false);
        ProblemSolved.SetActive(true);
        Left.SetActive(true);
        temp.text = "Close Ticket";
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromProblemSolved);
    }

    public void BackFromProblemSolved()
    {
        UploadComplaint.SetActive(true);
        ProblemSolved.SetActive(false);
        Left.SetActive(false);
        temp.text = "Complaint — Engine RPM not raising (DTMOT0001-004)";
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromUploadComplaint);
    }

    public void BackFromUploadComplaint()
    {
        Diagnostic.SetActive(true);
        UploadComplaint.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromDiagnostic);
    }

    public void NoFromDiagnostic()
    {
        Diagnostic.SetActive(false);
        Steps.SetActive(true);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps_DS);
        temp.text = "Steps (DS)";
    }

    public void BackFromSteps_DS()
    {
        Diagnostic.SetActive(true);
        Steps.SetActive(false);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromDiagnostic);
        temp.text = "Complaint — Engine RPM not raising (DTMOT0001-004)";
    }

    public void StartFromSteps_DS()
    {
        EngineRnR.SetActive(true);
        Steps.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromEngineRnR_DS);
        Left.SetActive(false);
        temp.text = "Engine R&R - Oil Filter (DS)";
    }

    public void BackFromEngineRnR_DS()
    {
        EngineRnR.SetActive(false);
        Steps.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps_DS);
        Left.SetActive(true);
        temp.text = "Steps (DS)";
    }

    public void YesFromProblemSolved()
    {
        Success.SetActive(true);
        ProblemSolved.SetActive(false);
        Top.SetActive(false);
        Left.SetActive(false);
        Text.SetActive(false);
        custom.onClick.RemoveAllListeners();
    }

    public void NoFromProblemSolved()
    {
        ProblemSolved.SetActive(false);
        Error.SetActive(true);
        //Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromError);
        temp.text = "Error";
    }

    public void BackFromError()
    {
        Error.SetActive(false);
        ProblemSolved.SetActive(true);
        //Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromProblemSolved);
        temp.text = "Close Ticket";
    }

    public void SubmitFromError()
    {
        Error.SetActive(false);
        Upload.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromUpload);
        temp.text = "Upload";
    }

    public void BackFromUpload()
    {
        Error.SetActive(true);
        Upload.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromError);
        temp.text = "Error";
    }

    public void UploadFromUploadPressed()
    {
        Upload.SetActive(false);
        Dashboard.SetActive(true);
        Left.SetActive(false);
        Top.SetActive(false);
        Text.SetActive(false);
        custom.onClick.RemoveAllListeners();
    }

    public void CloseTicketFromSuccess()
    {
        Success.SetActive(false);
        Dashboard.SetActive(true);
        Left.SetActive(false);
        //Top.SetActive(false);
        //Text.SetActive(false);
        custom.onClick.RemoveAllListeners();
    }

    public void ExploreZonesFromExplorePressed()
    {
        ExploreZones.SetActive(true);
        Explore.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExploreZones1);
        temp.text = "Explore Zones";
    }

    public void BackFromExploreZones1()
    {
        ExploreZones.SetActive(false);
        Explore.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(DeactivateExplore);
        temp.text = "Explore";
    }

    public void ExplodedViewFromExplore()
    {
        a = true;
        ExploreZones.SetActive(true);
        Explore.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExplodedView);
        temp.text = "Explore Zones";
    }

    public void BackFromExplodedView()
    {
        ExploreZones.SetActive(false);
        Explore.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(DeactivateExplore);
        temp.text = "Explore";
    }

    public void RnRFromExplore()
    {
        a = true;
        RnR.SetActive(true);
        Explore.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnR);
        temp.text = "R&R";
    }

    public void EnginePressed()
    {
        if (a)
        {
            RnRView.SetActive(true);
            RnR.SetActive(false);
            custom.onClick.RemoveAllListeners();
            custom.onClick.AddListener(BackFromRnRView);
            temp.text = "R&R View";
        }

        else
        {
            ExploreZones.SetActive(true);
            RnR.SetActive(false);
            custom.onClick.RemoveAllListeners();
            custom.onClick.AddListener(BackFromExploreZones);
            temp.text = "Explore Zones";
        }
        
    }

    public void EnginePressed_PS()
    {
        ExploreZones.SetActive(true);
        RnR.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExploreZones);
        temp.text = "Explore Zones";
    }

    public void ZoneFromExploreZonesPressed()
    {
        ExploreZones.SetActive(false);
        OilSump1.SetActive(true);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump1);
        temp.text = "OIL SUMP";
    }

    public void BackFromOilSump1()
    {
        ExploreZones.SetActive(true);
        OilSump1.SetActive(false);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromExploreZones);
        temp.text = "Explore Zones";
    }


    public void BackFromOilSump2()
    {
        OilSump1.SetActive(true);
        OilSump2.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump1);
        temp.text = "OIL SUMP";
    }

    public void ViewFromOilSump1Pressed()
    {
        OilSump1.SetActive(false);
        OilSump2.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump2);
    }

    private bool b;

    public void YesFromOilSump2()
    {
        if(b)
        {
        OilSump2.SetActive(false);
        Steps.SetActive(true);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps_PS);
        temp.text = "Steps (PS)";
        }

        else 
        {
            RnR.SetActive(true);
            OilSump2.SetActive(false);
            Left.SetActive(true);
            custom.onClick.RemoveAllListeners();
            custom.onClick.AddListener(BackFromRnRExploded);
            temp.text = "R&R";
        }
        
    }

    public void BackFromRnRExploded()
    {
        RnR.SetActive(false);
        OilSump2.SetActive(true);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump2);
        temp.text = "Oil SUMP";
    }

    public void BackFromSteps_PS()
    {
        OilSump1.SetActive(true);
        Steps.SetActive(false);
        Left.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump1);
        temp.text = "Oil SUMP";
    }

    public void StartFromSteps_PS()
    {
        Steps.SetActive(false);
        EngineRnR.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromEngineRnR_PS);
        Left.SetActive(false);
        temp.text = "Engine R&R - Oil Filter (PS)";

    }

    public void BackFromEngineRnR_PS()
    {
        Steps.SetActive(true);
        EngineRnR.SetActive(false);
        Left.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps_PS);
        temp.text = "Steps (PS)";
    }

    public void CancelFromOilSump2()
    {
        OilSump1.SetActive(true);
        OilSump2.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromOilSump1);
    }
    
    public void BackFromExploreZones()
    {
        RnR.SetActive(true);
        ExploreZones.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnR_PS);
        temp.text = "R&R (PS)";
    }
    public void BackFromRnRView()
    {
        RnR.SetActive(true);
        RnRView.SetActive(false);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnR);
        temp.text = "R&R";
    }

    public void StartFromRnRView()
    {
        Steps.SetActive(true);
        RnRView.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps);
        temp.text = "Steps";
    }

    public void BackFromSteps()
    {
        Steps.SetActive(false);
        RnRView.SetActive(true);
        Left.SetActive(true);
        Top.SetActive(true);
        Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromRnRView);
        temp.text = "R&R View";
    }

    public void StartFromSteps()
    {
        Steps.SetActive(false);
        EngineRnR.SetActive(true);
        Left.SetActive(false);
        //Top.SetActive(false);
        //Text.SetActive(false);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromEngineRnR);
        temp.text = "Engine R&R - Oil Filter";
    }

    public void BackFromEngineRnR()
    {
        Steps.SetActive(true);
        EngineRnR.SetActive(false);
        Left.SetActive(true);
        //Top.SetActive(true);
        //Text.SetActive(true);
        custom.onClick.RemoveAllListeners();
        custom.onClick.AddListener(BackFromSteps);
        temp.text = "Steps";
    }






    
}
