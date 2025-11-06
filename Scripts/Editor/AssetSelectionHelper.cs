using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class SelectObjectsWithMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [MenuItem("Assets/Select GameObjects with This Material")]
    private static void SelectMaterialUsages(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
        {
            if (selected.GetType() == typeof(Material))
            {
                Material mat = selected as Material;
                FindMaterialReferences(mat);
            }

        }
    }
    [MenuItem("Assets/Select ALL GameObjects in this Scene with This Material")]
    private static void SelectMaterialUsages2(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
        {
            if (selected.GetType() == typeof(Material))
            {
                Material mat = selected as Material;
                FindMaterialReferences(mat,true);
            }

        }
    }
    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Select GameObjects with This Material", true)]
    private static bool NewMenuOptionValidation()
    {
        // This returns true when the selected object is a Texture2D (the menu item will be disabled otherwise).
        return Selection.activeObject.GetType() == typeof(Material);
    }
   
    [MenuItem("CONTEXT/Component/Select GameObjects Using this Component")]
 private static void FindReferences(MenuCommand data)
 {
        string ue = "UnityEngine", valveVr = "Valve.VR.InteractionSystem";
        Object context = data.context;
     if (context)
     {
         var comp = context as Component;
            string compName = comp.GetType().ToString();
            if (comp)
            {
                if (compName.Contains(ue))
                    compName = compName.Remove(0, ue.Length+1);
                else if(compName.Contains(valveVr))
                    compName = compName.Remove(0, valveVr.Length+1);
                Debug.Log("Component is " + compName);
                SetSearchFilter(compName, 0);
                
            }
             
     }
 }
 
 [MenuItem("Assets/Select GameObjects with usage")]
 private static void FindReferencesToAsset(MenuCommand data)
 {
     var selected = Selection.activeObject;
     if (selected) {
            if (selected.GetType() == typeof(Material))
            {
                Material mat = selected as Material;
                FindMaterialReferences(mat);
            }
            else
                FindReferencesTo(selected);
        }
    }
    private static void FindMaterialReferences(Material m)
        => FindMaterialReferences(m, false);
    private static void FindMaterialReferences(Material m,bool includeInactive)
    {
        var referencedBy = new List<Object>();
        var allObjects = Object.FindObjectsOfType<GameObject>(includeInactive);
        for (int j = 0; j < allObjects.Length; j++)
        {
            if (allObjects[j].GetComponent<Renderer>()) { 
            Renderer selectedRenderer = allObjects[j].GetComponent<Renderer>();
            if(selectedRenderer.sharedMaterial== m)
            {
                Debug.Log("This Material named"+m.name+" is used by "+ selectedRenderer.name);
                referencedBy.Add(selectedRenderer.gameObject);
            }
            }
        }
        if (referencedBy.Count > 0)
            Selection.objects = referencedBy.ToArray();
        else Debug.Log("no references in scene");
    }
    class CommonMat
    {
        public Renderer rend;
        public int matId;
        public CommonMat(Renderer r,int id)
        {
            this.rend = r;
            this.matId = id;
        }
    }
    //List<CommonMat> allCommonMats;
    //private static void FindMaterialReferences2(Material m, bool includeInactive)
    //{
    //    var referencedBy = new List<Object>();
       
    //    var allObjects = Object.FindObjectsOfType<Renderer>(includeInactive);
    //    for (int j = 0; j < allObjects.Length; j++)
    //    {
    //        if (allObjects[j]!=null)
    //        {
    //            Renderer selectedRenderer = allObjects[j];
    //            int totalMatInSelRend = selectedRenderer.materials.Length;
    //            for (int i = 0; i < totalMatInSelRend; i++)
    //            {
    //                if (selectedRenderer.materials[i]==m)
    //                {
                      
    //                    Debug.Log("This Material named" + m.name + " is used by " + selectedRenderer.name);
    //                    referencedBy.Add(selectedRenderer.gameObject);
    //                }
    //            }
              
    //            if (selectedRenderer.sharedMaterial == m)
    //            {
    //                Debug.Log("This Material named" + m.name + " is used by " + selectedRenderer.name);
    //                referencedBy.Add(selectedRenderer.gameObject);
    //            }
    //        }
    //    }
    //    if (referencedBy.Count > 0)
    //        Selection.objects = referencedBy.ToArray();
    //    else Debug.Log("no references in scene");
    //}
    private static void FindReferencesTo(Object to)
 {
     
     var referencedBy = new List<Object>();
     var allObjects = Object.FindObjectsOfType<GameObject>();
     for (int j = 0; j < allObjects.Length; j++)
     {
         var go = allObjects[j];
 
           
         if (PrefabUtility.GetPrefabAssetType(go) == PrefabUtility.GetPrefabAssetType(go))
         {
             if (PrefabUtility.GetCorrespondingObjectFromSource(go) == to)
             {
                 Debug.Log(string.Format("referenced by {0}, {1}", go.name, go.GetType()), go);
                 referencedBy.Add(go);
             }
         }
 
         var components = go.GetComponents<Component>();
         for (int i = 0; i < components.Length; i++)
         {
             var c = components[i];
             if (!c) continue;
 
             var so = new SerializedObject(c);
             var sp = so.GetIterator();
 
             while (sp.NextVisible(true))
                 if (sp.propertyType == SerializedPropertyType.ObjectReference)
                 {
                     if (sp.objectReferenceValue == to)
                     {
                         Debug.Log(string.Format("referenced by {0}, {1}", c.name, c.GetType()), c);
                         referencedBy.Add(c.gameObject);
                     }
                 }
         }
     }
 
     if (referencedBy.Count > 0)
         Selection.objects = referencedBy.ToArray();
     else Debug.Log("no references in scene");
 }
    [MenuItem("Assets/Select GameObjects with usage", true)]
    private static bool NewMenuOptionValidationAssetSelector()
    {
        // This returns true when the selected object is a Texture2D (the menu item will be disabled otherwise).
        return Selection.activeObject.GetType() != typeof(Material);
    }

    public const int FILTERMODE_ALL = 0;
    public const int FILTERMODE_NAME = 1;
    public const int FILTERMODE_TYPE = 2;

    public static void SetSearchFilter(string filter, int filterMode)
    {
        SearchableEditorWindow hierarchy = null;
        SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));

        foreach (SearchableEditorWindow window in windows)
        {

            if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
            {

                hierarchy = window;
                break;
            }
        }

        if (hierarchy == null)
            return;

        MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
        object[] parameters = new object[] { filter, filterMode, false, false };

        setSearchType.Invoke(hierarchy, parameters);
    }
}
