using mixpanel;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class PermissionsRationaleDialog : MonoBehaviour
{
    const int kDialogWidth = 300;
    const int kDialogHeight = 100;
    private bool windowOpen = true;

    
    //void DoMyWindow(int windowID)
    void DoMyWindow(int windowID)
    {
        /* UIScreenManager screenManager = FindObjectOfType<UIScreenManager>();
         //screenManager.GetDialog().Show("Permission Request", "Allow Interactive Diagnostic to use Camera?", "Yes",()=>{ 
         screenManager.GetDialog().Show("Permission Request", "Allow Interactive Diagnostics to use Camera?",new System.Collections.Generic.List<UIScreenManager.NamedActions>()
         {
             new UIScreenManager.NamedActions("No",()=>screenManager.GetDialog().Close()),
             new UIScreenManager.NamedActions("Yes",()=>{
             Mixpanel.Track("Camera_Permission_Granted");
 #if PLATFORM_ANDROID
             Permission.RequestUserPermission(Permission.Camera);
 #endif
             windowOpen = false;
         })
        });*/

        // GUI.Label(new Rect(10, 20, kDialogWidth - 20, kDialogHeight - 50), "Allow Interactive Diagnostics to use Camera?");
        //GUI.Button(new Rect(10, kDialogHeight - 30, 100, 20), "No");
       // GUI.Label(new Rect(10, 20, 500 - 20, 200 - 50), "Allow Interactive Diagnostics to use Camera?");
       // GUI.Button(new Rect(10, 200 - 60, 200, 50), "No");
        //if (GUI.Button(new Rect(kDialogWidth - 110, kDialogHeight - 30, 100, 20), "Yes"))
       // if (GUI.Button(new Rect(500 - 210, 200 - 60, 200, 50), "Yes"))
        //{
#if PLATFORM_ANDROID
            Permission.RequestUserPermission(Permission.Camera);
#endif
            windowOpen = false;
        //}
    }

    void OnGUI()
    {
        Debug.Log("Camera_Permission_Granted  " + windowOpen);
        if (windowOpen)
        {

           // Rect rect = new Rect((Screen.width / 2) - (500 / 2), (Screen.height / 2) - (200 / 2), 500, 200);
           // GUI.ModalWindow(0, rect, DoMyWindow, "Permissions Request Dialog");
            DoMyWindow(2);
        }
    }
}