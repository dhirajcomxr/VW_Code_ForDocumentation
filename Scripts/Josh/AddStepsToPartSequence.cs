using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class AddStepsToPartSequence : MonoBehaviour
{

    [Header("TEST")]
    [SerializeField] string searchForId, resultText;
    [SerializeField] bool getParent = false, searchNow = false, populateLinkedObjs = false, export = false, import = false;
    [SerializeField] PartData result;
    [SerializeField] Transform partReferenceParent;
    [SerializeField] TextAsset partSequenceFile, assemblyStepsFile, dismantlingStepsFile;
    [Header("Settings")]
    [Space(10)]
    [SerializeField] string fileName;

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
        if (getParent)
        {
            GameObject r = GetCorrectParent(searchForId);
            if (r)
                Debug.Log("Found:" + r.name, r);
            else
                Debug.Log(searchForId + " not found!");
            getParent = false;
        }
        if (export)
        {
            ExportData(fileName);
            export = false;
        }
        if (populateLinkedObjs)
        {
            populateLinkedObjs = false;
            PopulateLinkedObjects();
            ExportData(fileName);

        }
    }
    void PopulateLinkedObjects()
    {
        for (int i = 0; i < partList.Count; i++)
        {
            PartData p = partList[i];
            GameObject g = GetCorrectParent(p.id);
            if (g)
                p.linkedObject = g;
            else
                Debug.Log(i + ": Did Not Find for " + p.id);
            partList[i] = p;
        }
    }
    GameObject SimpleParentFinder(string name)
    {
        GameObject g = GameObject.Find(name);
        GameObject result = null;
        if (g)
            result = g.transform.parent.gameObject;
        return result;
    }
    GameObject GetCorrectParent(string name)
    {
        GameObject g = GameObject.Find(name);
        Transform root= null;
        GameObject cp = null;
        if (g)
            root = g.transform.root;
        if (root!=null)
        {   
            Transform lastParent = g.transform.parent;
            while (lastParent.parent != root)
            {
                lastParent = lastParent.parent;
              
            }
            if (lastParent.parent == root)
                cp = lastParent.gameObject;

        }
        return cp;
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
    private void ExportData(string fName)
    {
        List<string> expRows = new List<string>();

        expRows.Add("Sr.No.,Zone,Part Name,Part Id,Linked Object,");
        for (int i = 0; i < partList.Count; i++)
        {
            string row = (i + 1) + ",";
            PartData part = partList[i];

            row += part.zone + ",";
            row += part.name + ",";
            row += part.id + ",";
            row += part.linkedObject ? part.linkedObject.name + "," : ",";

            expRows.Add(row);
        }
        string dStepPath = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "Dump" + System.IO.Path.DirectorySeparatorChar + fName + ".csv";
        Debug.Log(+expRows.Count + " rows Saved " + fileName + " to " + dStepPath.ToString());
        System.IO.File.WriteAllLines(dStepPath, expRows.ToArray());

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
