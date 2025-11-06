using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class PartListTable : MonoBehaviour
{
    
  
    [SerializeField] string searchForId,resultText;
   
    [Header("Settings")]
   
    [Space(10)]
    [SerializeField] PartData result;
    [SerializeField] bool getParent = false, searchNow = false, populateLinkedObjs = false, export = false, import = false;

    //  [SerializeField] Transform partReferenceParent;
    // [SerializeField] string fileName;
    [SerializeField] Transform prefabRoot;
    [SerializeField] string exportFileName;
    [SerializeField] TextAsset fileAsset;
     [SerializeField]//   Uncomment to view List
    List<PartData> partList;
    
    private int totalSteps;
    private List<Dictionary<string, object>> data;
    [System.Serializable]
    public class PartData
    {
        public string name, zone, id,linkedObjectName;
        public GameObject linkedObject;
        public bool isOneTime = false;
        public PartData()
        {
            this.name = "";
            this.zone = "";
            this.id = "";
        }
       public PartData(string n,string z,string i,GameObject linkedObj,string linkedObjName)
        {
            this.name = n;
            this.zone = z;
            this.id = i;
            this.linkedObject = linkedObj;
            this.linkedObjectName = linkedObjName;
        }
        public PartData(string n, string z, string i, GameObject linkedObj, string linkedObjName,bool oneTime)
        {
            this.name = n;
            this.zone = z;
            this.id = i;
            this.linkedObject = linkedObj;
            this.linkedObjectName = linkedObjName;
            this.isOneTime = oneTime;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
   //     LoadData(fileName);
    }

    // Update is called once per frame
    void Update()
    {
        if(searchNow)
        {
            searchNow = false;
            result = GetPartDataFor(searchForId);
            Debug.Log("NAME:" + result.name + " | ZONE:" + result.zone);
          
        }
        if(import)
        {
            import = false;
            LoadData(fileAsset);
        }
        if (getParent)
        {
            GameObject r = SimpleParentFinder(searchForId);
            Debug.Log("Found",r);
            getParent = false;
        }
        if (export)
        {
            ExportData(exportFileName);
            export = false;
        }
        if (populateLinkedObjs)
        {
            populateLinkedObjs = false;
            PopulateLinkedObjects();
            ExportData(exportFileName);

        }
    }
  public  void PopulateLinkedObjects()
    {
        for (int i = 0; i < partList.Count; i++)
        {
            PartData p = partList[i];
            GameObject g = GetParentGameObjectFromChildName(p.id);
            if (g)
                p.linkedObject = g;
            else
            {
                
                Debug.Log(i + ": Did Not Find for " + p.id);
            }
            partList[i] = p;
        }
    }
    GameObject SimpleParentFinder(string name)
    {
        GameObject g = GameObject.Find(name);
        GameObject result = null;
        if (g)
         result=   g.transform.parent.gameObject ;
        return result ;
    }
    GameObject GetParentGameObjectFromChildName(string name)
    {
        GameObject child = GameObject.Find(name);
        GameObject parentGO = null;
        string resultName = "";
        if (child == null)
            Debug.Log("[" + name + "]" + " not found");
        else
        {
            Transform parent = child.transform.parent;
             resultName = "";
            Transform root = child.transform.root;
            resultName += root.name;
            Debug.Log("TOTAL:" + root.childCount);
            List<string> subParents = new List<string>();
            for (int i = 0; i < root.childCount; i++)
            {
                //    subParents.Add(root.GetChild(i).name);
                if (parent == root.GetChild(i))
                    resultName += "/" + parent.name;
                else
                {
                    Transform t = root.GetChild(i);
                    if (t.childCount > 0)
                    {
                        Transform r = t.Find(name);
                    }
                }
            }
          parentGO=  parent.gameObject;
        }
        string FindMatchInParent(Transform par,string childName)
        {
            string result = "";
            Transform parT = par.Find(childName);
            if (parT != null)
                result = parT.name;
            return result;
        }
        
        resultText = resultName;
        return parentGO;
    }
    GameObject GetParentFromChild(Transform root,string name)
    {
        GameObject g = GameObject.Find(name);
        Transform result = g.transform;
        if (result == null)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform t = root.GetChild(i);
                if (t.childCount > 0)
                    t.Find(name);
            }
        }
        else
        {

            for (int i = 0; i < root.childCount; i++)
            {
                ;
            }
        }
            return result.gameObject;
    }
    /// <summary>
    /// this returns all part data if provided with the id
    /// </summary>
    /// <param name="partId"></param>
    /// <returns>Part Data with zone,id and name </returns>
    public PartData GetPartDataFor(string partId)
    {
        PartData p = null;
        for (int i = 0; i < partList.Count; i++)
        {
            if (partId == partList[i].id)
                p = partList[i];
        }
        if(p==null)
        Debug.Log("No Part Data found for " + partId);
        return p;    
    }
    public string GetLinkedObjectNameFor(string pardId)
    {
        string linkedObjName = GetPartDataFor(pardId).linkedObjectName??"";
        if (linkedObjName.Length > 0)
            resultText = linkedObjName;
        else
            Debug.Log("No Part Data found for " + pardId);
        return linkedObjName;
    }
    public string GetPartNameFor(string partId)
    {
         string pName = GetPartDataFor(partId).name;
        if (pName.Length > 0)
        {
            resultText = pName;
            Debug.Log("Part Name for " + partId + " is " + pName);
        }
        else
            Debug.Log(partId + " not found in Part List");
        return pName;
    }

    public List<PartData> GetList() => partList;
    public List<PartData> GetResultsFor(string search)
    {
       return partList.FindAll(
         delegate (PartData pt)
         {
             return pt.name.Replace(" ", "").ToUpper().Contains(search.Replace(" ", "").ToUpper());
         }
         );
    }
    public void Load() => LoadData(fileAsset);
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
    public void LoadData(TextAsset file)
    {
        data = null;     
        // data = CSVReader.Read("DT05N");
        Debug.Log("Loading partlist from " + file.name);
        data = CSVSimple.Read(file);
        totalSteps = data.Count;
        Debug.Log(data.Count);
        fileAsset = file;
        InitialiseData();
       
    }
    public void Export() => ExportData(exportFileName);
    private void ExportData(string fName)
    {
        List<string> expRows= new List<string>();
       
        expRows.Add("Sr.No.,Zone,Part Name,Part Id,Linked Object,");
        for (int i = 0; i < partList.Count; i++)
        {
            string row = (i+1)+",";
            PartData part = partList[i];

            row += part.zone + ",";
            row += part.name + ",";
            row += part.id + ",";
            row += part.linkedObject? part.linkedObject.name + ",": ",";

            expRows.Add(row);
        }
        string dStepPath = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "Dump" +System.IO.Path.DirectorySeparatorChar + fName + ".csv";
        Debug.Log(+expRows.Count+" rows Saved " + exportFileName + " to " + dStepPath.ToString());
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
                if (!cur.ContainsKey("Part Name"))
                    Debug.Log("Part Name does not exist");
                if (!(cur.ContainsKey("Part Id")))
                    Debug.Log("Part Id does not exist");
                string linkedObjName = "";
                GameObject g = null;
                if (data[i].ContainsKey("Linked Object"))
                {
                    linkedObjName = cur["Linked Object"].ToString();
                     g = GameObject.Find(linkedObjName);
                }
                bool oneTime = false;
                if (cur.ContainsKey("OneTimeUse"))
                {
                    oneTime = cur["OneTimeUse"].ToString() == "" ? false : true;
                }
                partList.Add(new PartData(cur["Part Name"].ToString(), zone, cur["Part Id"].ToString(), g,linkedObjName,oneTime));
            }
        }  
    }
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(PartListTable))]
public class PartListTableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PartListTable script = (PartListTable)target;
        string msg = "";
      
            EditorGUILayout.HelpBox(msg, MessageType.Info, true);
       
            EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import", GUILayout.MinHeight(25)))
                script.Load();

        if (GUILayout.Button("Export", GUILayout.MinHeight(25)))
            script.Export();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Populate Linked Objects", GUILayout.MinHeight(25)))
            script.PopulateLinkedObjects();

        EditorGUILayout.EndVertical();
        
        DrawDefaultInspector();
    }
}
#endif

