using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonInjestor : MonoBehaviour
{
    public TextAsset jsonFile;
    public jsonBlob blob;
    public string jsonConv;
    [System.Serializable]
    public class jsonBlob
    {
        public string[] headers, keys;
        public string[][] data;
       
    }
   

   
    [SerializeField]
   [System.Serializable]
    public class JData
    {
        public string[] data;
    }
    [System.Serializable]
    public class DTSteps
    {
        public string[,] step;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (jsonFile != null)
        {
            blob = new jsonBlob();
            blob.headers = getSArray(5);
            blob.keys = getSArray(5);
            var data = new string[5][];
            for (int j = 0; j < 5; j++)
            {
                var s = getSArray(3);
                data[j] = s;
                Debug.Log(data[j]);
            }
            jsonConv = JsonUtility.ToJson(blob);
        }
    }
    string[] getSArray(int l)
    {
        string[] s = new string[l];
        for (int i = 0; i < l; i++)
        {
            s[i] ="I:"+ Random.value;
        }
        return s;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
