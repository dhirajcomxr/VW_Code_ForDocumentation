using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ServiceablePartInfo {
    public string id;
    public string name;
    public string group;
}
public class Tag {
    public static string serviceable = "Serviceable";
}
public class ExplodedView : MonoBehaviour {
 
    public GameObject label;
    public GameObject resetOneLevelUi;
    [Space(10)]
    public ExteriorCam cam;
    public GameObject cameraPivot;
    public GameObject obj;
  
    public string partId = "";
   [SerializeField] public PartIdToStepFunction idToStep;
    [SerializeField] public PartHighlighter highlighter;
    public string fileName = "";
    public TextAsset partListFile;
    public bool destroyDisabledMeshes = false;
    public bool addColliders = false;

    int currentZoneIndex = 0;

    GameObject selectedObject;
    float temp = 0f, tapStart = 0f;

  [SerializeField]  int explodeLevel = 0;
  [SerializeField]  bool isExploding = false, isResetting = false;
    Bounds lastBound;
    bool loaded = false;
    // Start is called before the first frame update
    void OnEnable() {
        foreach (MeshRenderer mesh in obj.GetComponentsInChildren<MeshRenderer>(true)) {
            // remove any unused meshes
            if (destroyDisabledMeshes) {
                if (!mesh.gameObject.activeSelf) {
                    Destroy(mesh.gameObject);
                    continue;
                }
            }
            // add mesh colliders to all meshes
            if (addColliders) {
                if (mesh.gameObject.GetComponent<Collider>() == null) {
                    mesh.gameObject.AddComponent<MeshCollider>();
                }
            }
        }
        //ReadPartNames(partListFile);
        //SetCenterPivot(gameObject);
    }
    public void Load()
    {
        if(partListFile)
        ReadPartNames(partListFile);
        SetCenterPivot(gameObject);
    }
    void OnDisable() => StopAllCoroutines();
    Vector3 buttonDwnPos;
    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            tapStart = Time.time;
            buttonDwnPos = Input.mousePosition;
          
        }
        if (Input.GetMouseButtonUp(0)) {
            // only process the tap if it is shorter than threshold
            if ((Time.time - tapStart) < 0.9f && !isExploding && !isResetting) {
                Ray ray = Camera.main.ScreenPointToRay(buttonDwnPos);
             //   Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    if (hit.collider.gameObject.layer == 6) {
                        // tapped on the model
                        //Debug.Log("Hit");
                        StartCoroutine(SelectMesh(hit.collider.gameObject));
                    }
                    else {
                        //Debug.Log("Tapped Outside");
                        //   StartCoroutine(TappedOutside());
                       
                    }
                }
                else {
                    //Debug.Log("Tapped Outside");
                    //  StartCoroutine(TappedOutside());
                    if (isResetting || isExploding)
                        isResetting = isExploding = false;
                    if (explodeLevel >1)
                    {
                        Debug.Log("IS AT "+explodeLevel);
                        StartCoroutine(TappedOutside());
                    }
                }
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------
    
    IEnumerator SelectMesh(GameObject hit) {
        isExploding = true;
        UpdateResetAndExplodeUiOptions(true);
        if (explodeLevel == 0) {
            // Locate the zone
            GameObject parent = hit;
            int failSafeCounter = 0;
            while (parent.transform.parent.gameObject != obj) {
                failSafeCounter++;
                if (failSafeCounter > 10) {
                    Debug.LogError("Force Break");
                    break;
                }
                parent = parent.transform.parent.gameObject;
            }

            // locate the index of that zone
            currentZoneIndex = parent.transform.GetSiblingIndex();
            GameObject currentZone = obj.transform.GetChild(currentZoneIndex).gameObject;

            // set the explode level to 1
            explodeLevel = 1;
            SetCenterPivot(currentZone);
            // disable all other zones
            for (int i = 0; i < obj.transform.childCount; i++) {
                obj.transform.GetChild(i).gameObject.SetActive(i == currentZoneIndex);
            }
            //cameraPivot.transform.position = GetPartCenter(currentZone);
            currentZone.GetComponent<Animator>().SetTrigger("Explode");

            yield return new WaitForSeconds(0.1f);
            //  iTween.MoveTo(cameraPivot, GetPartCenter(currentZone), 0.5f);
            SetCenterPivot(currentZone);
        }
        else if (explodeLevel == 1) {
            // highlight the selected part
            if (hit.gameObject.tag != Tag.serviceable) {
                // check if the parent object is serviceable
                GameObject parent = hit.transform.parent.gameObject;
                int counter = 0;
                while (parent.tag != Tag.serviceable) {
                    counter++;
                    if (counter > 10) {
                        Debug.LogError("Force Break");
                        break;
                    }
                    if (parent.transform.parent == null) {
                        break;
                    }
                    parent = parent.transform.parent.gameObject;
                }
                hit = parent;
            }
            selectedObject = hit;
            //  hit.AddComponent<Highlighter>();
            idToStep.SearchForPart(selectedObject);
            explodeLevel = 2;
            SetLabel(GetObjectName(hit.gameObject.name));
            //label.GetComponent<Text>().text = GetObjectName(hit.gameObject.name);
            //label.SetActive(true);
            if (idToStep != null)
                idToStep.SearchPartId(hit.gameObject.name);
        }
        isExploding = false;
    }
    GameObject target;
    void SetCenterPivot(GameObject currentZone)
    {
      
        Vector3 partCenter = GetPartCenter(currentZone);
        Vector3 pivotPos = cameraPivot.transform.position;
        pivotPos.y = partCenter.y;
      
        cameraPivot.transform.position = partCenter;
        SetPivotOnly(partCenter);
    }
    void SetPivotOnly(Vector3 center,bool test=true)
    {
        if (!target)
            target = new GameObject("Camera Pivot Runtime");
        Vector3 offset = Vector3.zero;
        if(test)
           offset= Vector3.up * (lastBound.max.y + lastBound.min.y) * 0.5f;
        target.transform.position = center + offset;
        cam.target = target.transform;
        
        cam.CallMovement();
    }
    string GetObjectName(string id) {
        foreach (ServiceablePartInfo part in serviceablePartInfo) {
            if (part.id == id) {
                return part.name.ToUpper();
            }
        }
        return id;
    }
    
    void SetLabel(string labelText)
    {
        if (label)
        {
            label.GetComponent<Text>().text = labelText;
            label.transform.parent.gameObject.SetActive(labelText.Length > 0);
        }
    }
    void UpdateResetAndExplodeUiOptions(bool state)
    {
        resetOneLevelUi.SetActive(true);
    }

    public void ResetOneLevel() => StartCoroutine(TappedOutside());
    public void ResetFull()
    {
        if (gameObject.activeSelf)
        {
            isResetting = isExploding = false;
            StartCoroutine(TappedOutside());
            StartCoroutine(TappedOutside());
            StartCoroutine(TappedOutside());
        }
    }
    IEnumerator TappedOutside() {
        isResetting = true;
        //Debug.Log("Tapped Outside");
        UpdateResetAndExplodeUiOptions(false);
        // label.SetActive(false);
        SetLabel("");
        if (selectedObject != null) {
            Debug.Log("Found:" + selectedObject.name, selectedObject);
            if(selectedObject.GetComponent<Highlighter>())
            selectedObject.GetComponent<Highlighter>().enabled = false;
            if (selectedObject.GetComponent<iTween>())
                Destroy(selectedObject.GetComponent<iTween>());
            selectedObject = null;
            idToStep.SearchForPart(selectedObject);
        }

        if (explodeLevel == 1) {
            // reset the explode level
            explodeLevel = 0;

            GameObject currentZone = obj.transform.GetChild(currentZoneIndex).gameObject;
            currentZone.GetComponent<Animator>().SetTrigger("Reset");

            yield return new WaitForSeconds(0.5f);

            // enable all objects
            for (int i = 0; i < obj.transform.childCount; i++) {
                obj.transform.GetChild(i).gameObject.SetActive(true);
            }
            //   iTween.MoveTo(cameraPivot, GetPartCenter(obj), 0.5f);
            cameraPivot.transform.position = GetPartCenter(obj);
            SetPivotOnly(transform.position,false);
        }
        else if (explodeLevel == 2) {
            explodeLevel = 1;
        }
      
        isResetting = false;
        UpdateResetAndExplodeUiOptions(explodeLevel > 0);
    }

    [ContextMenu("Export Serviceable Parts to CSV")]
    void ExportToCSV() {
        List<string> lines = new List<string>();
        for (int i = 0; i < obj.transform.childCount; i++) {
            lines.Add("\n\n" + obj.transform.GetChild(i).gameObject.name + " ---------------");
            for (int j = 0; j < obj.transform.GetChild(i).childCount; j++) {
                lines.Add(obj.transform.GetChild(i).GetChild(j).gameObject.name);
            }
        }
        string path = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + fileName + ".csv";
        Debug.Log(path);
        File.WriteAllLines(path, lines.ToArray());
    }

    //------------------------------------------------------------------------------------------------------------

    // calculate center of the part
    Vector3 GetPartCenter(GameObject part) {
        Bounds b = new Bounds();
        foreach (MeshRenderer mr in part.GetComponentsInChildren<MeshRenderer>(true)) { b.Encapsulate(mr.bounds); }
        lastBound = b;
        return b.center;
    }

    /*
    // calculate the explode position
    Vector3 CalcExplodePosition(GameObject part, GameObject root, bool calcFromCenter) {
        Vector3 partPosition = part.transform.position;
        Vector3 rootPosition = root.transform.position;
        if (calcFromCenter) { partPosition = GetPartCenter(part); rootPosition = GetPartCenter(root); }
        Vector3 explodePos = partPosition + ((partPosition - rootPosition) * 2f);
        return explodePos;
    }
    */

    //------------------------------------------------------------------------------------------------------------

    List<ServiceablePartInfo> serviceablePartInfo;
    //void ReadPartNames()
    //{
    //    TextAsset dataFile = Resources.Load(fileName) as TextAsset;
    //    ReadPartNames(dataFile);
    //}
      public  void ReadPartNames(TextAsset data) {
    
        serviceablePartInfo = new List<ServiceablePartInfo>();
        //string filePath = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + fileName + ".csv";

        string[] lines = Regex.Split(data.text, System.Environment.NewLine);
        foreach (string s in lines) {
            if (s.Contains(",")) {
                string[] split = Regex.Split(s, ",");
                // filter out empty lines
                if (split[0] != "") {
                    ServiceablePartInfo part = new ServiceablePartInfo();
                    part.group = split[0];
                    part.id = split[1];
                    part.name = split[2];

                    serviceablePartInfo.Add(part);
                }
            }
        }
    }
}