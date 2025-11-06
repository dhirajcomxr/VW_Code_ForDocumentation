using mixpanel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class IpApiData
{
    public string ip;
    public string country_name;
    public string city;
    public string country_calling_code;

    public static IpApiData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<IpApiData>(jsonString);
    }
}

public class IPAddressRetriever : MonoBehaviour
{
    private string currentCountry;
    private string currentCity;
    private string countryCode;
    public string ipAddress;
    public bool enableAppLock = true;
    [SerializeField] List<string> saveCSVdata = new List<string>();
    bool isFound;
    
    private void Start()
    {
        //Debug.Log("App Version ------> " + Application.version);
        StartCoroutine(SetCountry());
    }

    //Information of the Device 
    IEnumerator SetCountry()
    {
        string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
        string uri = $"https://ipapi.co/{ip}/json/";
        Debug.Log("COUNTRY IP" + ip);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            IpApiData ipApiData = IpApiData.CreateFromJSON(webRequest.downloadHandler.text);
            Debug.Log("data" + webRequest.downloadHandler.text);

            ipAddress = ipApiData.ip;
            currentCountry = ipApiData.country_name;
            currentCity = ipApiData.city;
            countryCode = ipApiData.country_calling_code;
            Mixpanel.Track("CityName", "city_name", currentCity);
            Mixpanel.Track("IPAddress", "ip_address", ipAddress);
            StartCoroutine(AccessCityName());
        }
    }

    //Checking the Device is accessible on a particular location
    IEnumerator AccessCityName()
    {
        yield return new WaitForSeconds(1f);
        string url = $""; //csv file location
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading CSV file: " + webRequest.error);
            }
            else
            {
                string[] fields;
                string csvText = webRequest.downloadHandler.text; //Downloading csv data
                string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None); // Splitting every new line
                foreach (string line in lines)
                {
                    fields = line.Split(','); // Splitting each location on a single line seperated by comma(,)
                    for (int i = 0; i < fields.Length; i++)
                    {                 
                        saveCSVdata.Add(fields[i]); //Storing each location in the List
                    }
                }
                foreach (string loc in saveCSVdata)
                {
                    if(loc == "All Location")
                    {
                        isFound = true;
                        Debug.Log("Allow User all over the location");
                        StartCoroutine(CheckVersion());
                        break;
                    }
                    else if(loc == "India")
                    {
                        if (currentCountry == "India")
                        {
                            Debug.Log("By Country name Allow User in " + currentCountry);
                            StartCoroutine(CheckVersion());
                        }
                        else
                            QuitApplication();
                    }
                    else if (currentCity.Contains(loc))
                    {
                        isFound = true;
                        Debug.Log("Allow User By City name" + currentCity + " " + loc);
                        StartCoroutine(CheckVersion());
                        break;
                    }
                }
                if (!isFound)
                    QuitApplication();
            }
        }
    }

    IEnumerator CheckVersion()
    {
        WWWForm form = new WWWForm();
        form.AddField("connectivity", "vwversion");
        form.AddField("appVersion", Application.version.ToString());

        UnityWebRequest w = UnityWebRequest.Post("" + "", form);
        yield return w.SendWebRequest();
        if (w.error == null)
        {
            string itemData = w.downloadHandler.text;
            if (itemData.Contains("success"))
            {
            }
            else
            {
                QuitApplication();
            }
        }
        else
            Debug.Log("Error during upload: " + w.error);
    }

    void QuitApplication()
    {
        if (enableAppLock)
        {
            Application.Quit();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }
    }

    public void ScriptDisabler()
    {
        this.enabled = false;
    }
    
}
