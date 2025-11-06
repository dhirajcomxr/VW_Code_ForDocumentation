using UnityEngine;
using mixpanel;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
public class RequestPermissions : MonoBehaviour
{
    GameObject dialog = null;
    private void OnEnable()
    {
        Mixpanel.Track("App Start!");
        Debug.Log("APP Start");
    }
    //Camera Permission
    void Start()
        {

#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                dialog = new GameObject();
            }
       
#endif
        }

        void OnGUI()
        {
#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // The user denied permission to use the microphone.
                // Display a message explaining why you need it with Yes/No buttons.
                // If the user says yes then present the request again
                // Display a dialog here.
                dialog.AddComponent<PermissionsRationaleDialog>();
                return;
            }
            else if (dialog != null)
            {
                Destroy(dialog);
            }
#endif

            // Now you can do things with the microphone
        }

    private void OnApplicationQuit()
    {
        Mixpanel.Track("Application_Quit");
        Mixpanel.Reset();
        //Mixpanel.Reset();
    }

}
