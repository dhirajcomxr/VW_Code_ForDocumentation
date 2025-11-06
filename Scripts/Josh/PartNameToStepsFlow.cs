using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartNameToStepsFlow : MonoBehaviour
{

    [Header("TEST")]
    [SerializeField] string searchForId, resultText;
    [SerializeField] bool searchNow = false, playStep = false, import = false;
    [SerializeField] PartData result;
    [SerializeField] TextAsset partSequenceFile, assemblyStepsFile, dismantlingStepsFile;
    [Header("Settings")]
    [Space(10)]
    [SerializeField] string fileName;
    PartSequenceEximProcessor sequenceProcessor;

    [SerializeField]//   Uncomment to view List
    List<PartData> partList;

    private int totalSteps;
    private List<Dictionary<string, object>> data;
    [System.Serializable]
    public class PartData
    {
        public string name, zone, id;
        public GameObject linkedObject;
        public PartData()
        {
            this.name = "";
            this.zone = "";
            this.id = "";
        }
        public PartData(string n, string z, string i, GameObject linkedObj)
        {
            this.name = n;
            this.zone = z;
            this.id = i;
            this.linkedObject = linkedObj;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadData(fileName);
    }

    // Update is called once per frame
    void Update()
    {
        if (searchNow)
        {
            result = GetPartDataFor(searchForId);
            Debug.Log("NAME:" + result.name + " | ZONE:" + result.zone);
            searchNow = false;
        }
        if (import)
        {
            import = false;
            LoadData(fileName);
        }   
       
        if (playStep)
        {
            if (!sequenceProcessor)
                sequenceProcessor = FindObjectOfType<PartSequenceEximProcessor>();
            if (sequenceProcessor)
                sequenceProcessor.StepSequenceFromName(searchForId);
        
        }
    }
   
    /// <summary>
    /// this returns all part data if provided with the id
    /// </summary>
    /// <param name="partId"></param>
    /// <returns>Part Data with zone,id and name </returns>
    public PartData GetPartDataFor(string partId)
    {
        PartData p = new PartData();
        for (int i = 0; i < partList.Count; i++)
        {
            if (partId == partList[i].id)
                p = partList[i];
        }
        return p;
    }
    public string GetPartNameFor(string partId) => GetPartDataFor(partId).name;
    #region Load
    private void LoadData(string file)
    {
        data = null;
        // data = CSVReader.Read("DT05N");
        data = CSVReader.Read(file);
        totalSteps = data.Count;
        Debug.Log(data.Count);
        InitialiseData();
    }

    private void InitialiseData()
    {
        partList = new List<PartData>();
        for (int i = 0; i < totalSteps; i++)
        {
            string zone = data[i]["Zone"].ToString();
            if (zone.Length >= 1)
            {
                var cur = data[i];
                GameObject g = GameObject.Find(cur["Linked Object"].ToString());

                partList.Add(new PartData(cur["Part Name"].ToString(), zone, cur["Part ID"].ToString(), g));
            }
        }
    }
    #endregion
}
