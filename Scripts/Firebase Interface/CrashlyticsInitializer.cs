using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class CrashlyticsInitializer : MonoBehaviour
{

    public static CrashlyticsInitializer instance;

    public bool isInitialized;

    void Awake()
    {
        if (instance == null)
            instance = this;

        isInitialized = false;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase Dependency Status:" + dependencyStatus.ToString());
                // Set a flag here to indicate that your project is ready to use Firebase.
                isInitialized = true;
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    public void TestCrash()
    {
        FindObjectOfType<UIScreenManager>().Toast("Crash Test!");
        throw new System.Exception("(ignore) this is a test crash");
    }
}