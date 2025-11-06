using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ObjectReferenceMapper : MonoBehaviour
{
  //  public Hashtable oReferences;
    Dictionary<int, GameObject> oReferences;
    private static ObjectReferenceMapper instance = null;
    public static ObjectReferenceMapper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectReferenceMapper>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = "ObjectReferenceMapper";
                    instance = go.AddComponent<ObjectReferenceMapper>();
                    //  DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    public void Init()
    {
        ORMID[] allids=null;
        
        if (oReferences != null)
            oReferences.Clear();
            oReferences = new Dictionary<int, GameObject>();
        allids = FindObjectsOfType<ORMID>(true);
        //  Debug.Log("Total: " + allids.Length);
        for (int i = 0; i < allids.Length; i++)
        {
            oReferences.Add(allids[i].id, allids[i].gameObject);
            //     ormids.Add(allids[i]);
        }
    }
    public GameObject GetGameObjectFromId(int id)
    {
        ORMID[] allids;
        if (oReferences == null)
            oReferences = new Dictionary<int, GameObject>();
        
        if (oReferences.Count < 1)
        {
            allids = FindObjectsOfType<ORMID>(true);
          //  Debug.Log("Total: " + allids.Length);
            for (int i = 0; i < allids.Length; i++)
            {
                oReferences.Add(allids[i].id, allids[i].gameObject);
           //     ormids.Add(allids[i]);
            }
      //      Debug.Log("Total ORMIDs: " + oReferences.Count);
        }
        GameObject resultGo = null;
        if(!oReferences.TryGetValue(id,out resultGo))
        {
            Debug.Log("ID: " + id + " not found!");
        }
        return resultGo;
    }
    public GameObject[] GetGameObjectsFromIds(int[] ids)
    {
        List<GameObject> result= new List<GameObject>();
        if(ids!=null)
        for (int i = 0; i < ids.Length; i++)
        {
            result.Add(GetGameObjectFromId(ids[i]));
        }
        return result.ToArray();
    }
    // Start is called before the first frame update
    public static int GenerateId(GameObject g)
    {
        ORMID g_orm = g.GetComponent<ORMID>();
        if (g_orm == null)
            g_orm = g.AddComponent<ORMID>();
        g_orm.id = g.GetInstanceID();
        return g_orm.id;
    }
    public static int[] GenerateId(GameObject[] gArray)
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < gArray.Length; i++)
        {
            if(gArray[i]!=null)
            ids.Add(GenerateId(gArray[i]));
        }
        return ids.ToArray();
    }
   
}
