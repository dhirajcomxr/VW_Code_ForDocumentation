using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FetchURL : MonoBehaviour
{

    [System.Serializable]
    public class YourDataClass
    {
        public string ApiUrl;
        public YourVehicleDataClass VehicleData;
    }

    [System.Serializable]
    public class YourVehicleDataClass
    {
        // Define the structure of your vehicle data class
    }

    [ContextMenu("GetData")]
    public void FetctData(string url)
    {
        StartCoroutine(Fetch(url));
    }
    IEnumerator Fetch(string url)
    {
        string phpScriptUrl = url;
         UnityWebRequest www = UnityWebRequest.Get(phpScriptUrl);

        yield return www;

        if (www.error != null)
        {
            Debug.LogError("WWW error: " + www.error);
        }
        else
        {
            // Parse the JSON response
            YourDataClass data = JsonUtility.FromJson<YourDataClass>(www.downloadHandler.text);
            Debug.LogError("API Url 1: " + data);
            // Access the API URL and do something with it
            string apiUrl = data.ApiUrl;
            Debug.LogError("API URL: " + apiUrl);

            // Access the vehicle data if needed
            YourVehicleDataClass vehicleData = data.VehicleData;
            if (vehicleData != null)
            {
                // Do something with the vehicle data
            }
        }
    }  
}
