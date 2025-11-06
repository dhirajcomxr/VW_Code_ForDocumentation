using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LineDetector : MonoBehaviour {

    public GameObject SearchGO;
    public Material wireMaterial;
    public Text text;
    public GameObject Table;
    public GameObject Add;
    public GameObject Remove;
    public GameObject ArrowDown;
    public GameObject ArrowUp;
    public ExteriorCam Ext;
    //public GameObject Reset;
    public GameObject Row;
    public GameObject connectorName;
    public GameObject ConnectorDesig;
    //public GameObject ComponentDesig;
    public GameObject Content_Parent;
    public List<GameObject> wires;
    public Connector connector;
    public float speed = 0.7f;
    public bool wireSelected = false;
    public List<GameObject> selectedPins;
    public GameObject Num;
    public GameObject cameraPivot;
    public Transform NewCamPos;
    public GameObject tempTag;
    bool hasAtleastOneYes = false;
    AllConnectors AllConn;
    Camera cam;
    GameObject[] temp, temp2;
    GameObject temp1;
    Button temp_btn;
    Animator Anim;
    Outline outline;
    bool isSmoothing;
    public bool isDown = false;
    RaycastHit globalHit;
    public float smoothStat = 0;
    GlobalActiveWires GlobalWires;
    Scene scene;
    CentralHarnessCSVMapping centralMappingObj;

    // Start is called before the first frame update
    void Start() {
        AllConn = FindObjectOfType<AllConnectors>();
        isDown = false;
        Color col = wireMaterial.color;
        col.a = 1;
        wireMaterial.color = col;
        scene = SceneManager.GetActiveScene();
        if (SceneManager.sceneCount > 1)
            scene = SceneManager.GetSceneAt(1);
        else
            scene = SceneManager.GetSceneAt(0);
        centralMappingObj = FindObjectOfType<CentralHarnessCSVMapping>();
    }

    public void DisableAllWires() {
        foreach (Wire w in connector.wires) {
            if (w.nodes != null) {
                foreach (Node n in w.nodes) {
                    if (n.model != null) {
                        n.model.SetActive(false);
                    }
                }
            }
        }
    }

    public void DisableAllOutlineEffects() {
        foreach (GameObject o in temp) {
            if (o != null) {
                //Debug.Log(o);
                outline = o.transform.GetComponent<Outline>();
                outline.enabled = false;
            }
        }
    }

    public void DisableAllLabels() {
        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
        temp1.SetActive(false);
        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
        temp1.SetActive(false);
        temp1 = GameObject.Find("Labels");
        if (temp1 != null) {
            temp1.SetActive(false);
        }
        temp1 = GameObject.Find("Labels (1)");
        if (temp1 != null) {
            temp1.SetActive(false);
        }
    }

    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void SetBoolBack() {
        isSmoothing = false;
        Ext.enabled = true;
        Ext.NewPosRot(NewCamPos.transform.position, NewCamPos.transform.rotation);
        //cameraPivot.transform.position = globalHit.transform.position;
        //Camera.main.transform.LookAt(globalHit.transform.position);
        //Debug.Log("is smoothing false");
    }

    // Update is called once per frame
    void Update() {

        if (isSmoothing == true) {
            smoothStat = smoothStat + Time.deltaTime * speed;
            if (smoothStat >= 1f) {
                //isSmoothing = false;
                SetBoolBack();
            }
            //Invoke("SetBoolBack", 0.2f);
            Ext.transform.position = Vector3.Lerp(Ext.transform.position, NewCamPos.transform.position, speed * Time.deltaTime);
            //Camera.main.transform.LookAt(globalHit.transform.position);
            Ext.transform.rotation = Quaternion.Lerp(Ext.transform.rotation, NewCamPos.transform.rotation, Time.deltaTime * speed);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        if (Input.GetMouseButtonDown(0)) {
            if (connector == null) {
                if (!IsPointerOverUIObject()) {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    //globalHit = hit;


                    if (Physics.Raycast(ray, out hit, 10000f)) {
                        if (connector != null) {
                            if (hit.transform.gameObject.name.ToLower().Contains("wire")) {
                                //Debug.Log("Wire Selected");
                                wireSelected = true;
                                int wireNum = 0, nodeNum = 0;
                                foreach (Wire w in connector.wires) {
                                    wireNum++;
                                    foreach (Node n in w.nodes) {
                                        nodeNum++;
                                        bool matchFound = (hit.transform.gameObject == n.model);
                                        n.model.SetActive(matchFound);
                                        if (matchFound) {
                                            selectedPins.Add(w.pin);
                                            GetComponent<PartHighlighter>().Highlight(w.pin);
                                            if (n.endPointPin != null) {
                                                selectedPins.Add(n.endPointPin);
                                                GetComponent<PartHighlighter>().Highlight(n.endPointPin);
                                            }
                                            text.text = "<b>Component:</b>\n" + connector.component.gameObject.name
                                                + "\n\n<b>Component Designation:</b>\n" + connector.connectorDesignation
                                                //+ "\n\n<b>Connector Part Number:</b>\n" + connector.gameObject.name
                                                + "\n\n<b>Connector Designation:</b>\n" + connector.connectorDesignation
                                                + "\n\n-----------------------------------------------------------------"
                                                + "\n\n<b>Pin Number:</b>\n" + wireNum
                                                + "\n\n<b>Color Code:</b>\n" + w.colorCode
                                                + "\n\n<b>Cross Section:</b>\n" + w.crossSection
                                                + "\n\n<b>Node Index:</b>\n" + nodeNum
                                                //+ "\n\n<b>End Connector Part Number:</b>\n" + n.endPointObj.name
                                                + "\n\n<b>End Connector Designation:</b>\n" + n.endComponentDesignation
                                                + "\n\n<b>End Connector Pin Number:</b>\n" + n.endPointPinNum
                                                //+ "\n\n<b>End Component:</b>\n" + n.endComponentObj.name
                                                + "\n\n<b>End Component Designation:</b>\n" + n.endComponentDesignation;
                                        }
                                    }
                                }
                            }
                            else {
                                if (wireSelected) {
                                    foreach (GameObject g in wires) {
                                        g.SetActive(true);
                                    }
                                    wireSelected = false;
                                    //Debug.Log("Wire Deselected");
                                    GetComponent<PartHighlighter>().RemoveAllHighlights();
                                    text.text = "<b>Component:</b>\n" + connector.component.gameObject.name
                                        + "\n\n<b>Component Designation:</b>\n" + connector.componentDesignation
                                        //+ "\n\n<b>Connector Part Number:</b>\n" + connector.gameObject.name
                                        + "\n\n<b>Connector Designation:</b>\n" + connector.connectorDesignation
                                        + "\n\n<b>Wires:</b>\n" + connector.wires.Length;
                                }
                                else {
                                    foreach (Wire w in connector.wires) {
                                        if (w.nodes.Length != 0) {
                                            foreach (Node n in w.nodes) {
                                                if (n.model != null) {
                                                    n.model.SetActive(false);

                                                }
                                            }
                                        }
                                    }
                                    Debug.Log("Connector Deselected");


                                    GlobalWires.DestroyWires();

                                    Table.SetActive(false);
                                    Add.SetActive(false);
                                    Remove.SetActive(false);
                                    //Arrow.SetActive(false);
                                    DisableAllArrows();
                                    //Reset.SetActive(false);
                                    if (temp.Length != 0) {
                                        foreach (GameObject g in temp) {
                                            //Debug.Log(temp.Length);
                                            //temp.Remove(g);
                                            DestroyImmediate(g);
                                        }
                                    }
                                    temp2 = FindObjectOfType<AllNodes>().AllNodess;
                                    foreach (GameObject go in temp2) {
                                        go.SetActive(false);
                                    }
                                    foreach (GameObject go in GameObject.FindGameObjectsWithTag("Label")) {
                                        if (go.name == "Labels" || go.name == "Labels (1)") {
                                            go.SetActive(false);
                                        }
                                    }
                                    FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().RemoveAllHighlights();
                                    Color col = wireMaterial.color;
                                    col.a = 1;
                                    wireMaterial.color = col;
                                    text.text = "Description goes here";
                                    connector = null;
                                }
                            }
                        }
                        else {
                            if (hit.transform.GetComponent<Connector>() != null) {
                                cameraPivot.transform.position = hit.transform.position;
                                //cameraPivot.transform.position = hit.transform.position - hit.transform.forward * 0.05f ;
                                NewCamPos.position = hit.transform.position - hit.transform.forward * 0.05f;
                                //Camera.main.transform.position = hit.transform.position - (hit.transform.forward * 0.02f);
                                NewCamPos.LookAt(hit.transform.position);
                                globalHit = hit;
                                Ext.distance = 0.05f;
                                isSmoothing = true;
                                smoothStat = 0;
                                Ext.enabled = false;
                                //Ext.NewPosRot(hit.transform.position - (hit.transform.forward * 0.2f), Quaternion.LookRotation(hit.transform.position - Ext.transform.position));
                                //Ext.NewPosRot(hit.transform.position, Quaternion.LookRotation(hit.transform.position-Ext.transform.position));
                                //cameraPivot.transform.LookAt(hit.transform.position);
                                SearchGO.SetActive(false);
                                Debug.Log("Connector Selected");

                                DisableAllArrows();
                                //Arrow1.SetActive(false);
                                GlobalWires = FindObjectOfType<GlobalActiveWires>();

                                //GlobalWires.DestroyWires();

                                Color col = wireMaterial.color;
                                col.a = 0.15f;
                                wireMaterial.color = col;
                                connector = hit.transform.GetComponent<Connector>();
                                //if()
                                connector.GetComponent<BoxCollider>().enabled = false;

                                if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
                                    AllConn.StartConnMat(connector);
                                }

                                int tempCount = connector.transform.GetChild(2).childCount;
                                tempTag = connector.component.transform.Find("Tag").gameObject;

                                /*text.text = "<b>Component:</b>\n" + connector.component.gameObject.name
                                + "\n\n<b>Component Designation:</b>\n" + connector.componentDesignation
                                //+ "\n\n<b>Connector Part Number:</b>\n" + connector.gameObject.name
                                + "\n\n<b>Connector Designation:</b>\n" + connector.connectorDesignation
                                + "\n\n<b>Wires:</b>\n" + connector.wires.Length;*/

                                /*foreach (Wire w in connector.wires) {
                                    foreach (Node n in w.nodes) {
                                        n.model.SetActive(true);
                                    }
                                }*/
                                int cc = 0;
                                Table.SetActive(true);
                                Add.SetActive(true);
                                Remove.SetActive(true);
                                //Arrow1.SetActive(true);
                                //Reset.SetActive(true);
                                Anim = null;
                                Anim = connector.transform.GetChild(2).GetComponent<Animator>();
                                if(Anim == null)
                                {
                                    Debug.Log("entered");
                                    Add.SetActive(false);
                                    Remove.SetActive(false);
                                }
                                Add.GetComponent<Button>().onClick.AddListener(AssignAddAnim);
                                Remove.GetComponent<Button>().onClick.AddListener(AssignRemoveAnim);
                                //Arrow.GetComponent<Button>().onClick.AddListener(FlipTheArrow);
                                //Reset.GetComponent<Button>().onClick.AddListener(ReloadLevel);
                                //Debug.Log(connector);

                                temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
                                temp1.SetActive(true);
                                //if (temp1.transform.GetChild(0).GetComponent<Canvas>())
                                {
                                    //temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                                }

                                temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
                                temp1.SetActive(true);
                                //if (temp1.transform.GetChild(0).GetComponent<Canvas>())
                                {
                                    //temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                                }

                                temp1 = connector.component.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;
                                //temp1.AddComponent<Canvas>();


                                //Vector3 screenPos = Camera.main.WorldToScreenPoint(connector.transform.position);
                                //Table.transform.position = screenPos;
                                //Table.transform.position = new Vector3(Table.transform.position.x, Table.transform.position.y + 200, Table.transform.position.z);
                                for (int i = 0; i < connector.wires.Length; i++) //calculating the number of table rows
                                {
                                    cc++;
                                }
                                temp = new GameObject[cc];

                                if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
                                    hasAtleastOneYes = false;
                                }
                                else {
                                    hasAtleastOneYes = true;
                                }
                                //Debug.Log(cc);
                                cc = 0;
                                connectorName.transform.GetComponent<Text>().text = connector.componentDesignation.ToString() + "-" + connector.connectorName.ToString();
                                ConnectorDesig.transform.GetComponent<Text>().text = connector.connectorDesignation.ToString();
                                //ComponentDesig.transform.GetComponent<Text>().text = connector.componentDesignation.ToString();
                                for (int i = 0; i < connector.wires.Length; i++) //filling table data
                                {
                                    if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
                                        hasAtleastOneYes = false;
                                    }
                                    else {
                                        hasAtleastOneYes = true;
                                    }
                                    if (connector.wires[i].nodes != null) {
                                        for (int m = 0; m < connector.wires[i].nodes.Length; m++) {
                                            if (connector.wires[i].nodes[m].toInclude) { hasAtleastOneYes = true; }
                                        }
                                        if (hasAtleastOneYes) {


                                            //Debug.Log(connector.wires[i].nodes.Length);
                                            temp[i] = Instantiate(Row, Content_Parent.transform);
                                            temp[i].transform.GetComponent<ConnectedWire>().wire = connector.wires[i];
                                            temp[i].transform.GetComponent<ConnectedWire>().conn = connector;
                                            connector.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(i).GetComponent<ConnectedWire>().wire = connector.wires[i];
                                            connector.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(i).GetComponent<ConnectedWire>().conn = connector;




                                            temp[i].transform.GetChild(0).gameObject.GetComponent<Text>().text = connector.wires[i].wireNumber.ToString();
                                            temp[i].transform.GetChild(1).gameObject.GetComponent<Text>().text = connector.wires[i].crossSection.ToString();
                                            //tempTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].crossSection.ToString();
                                            temp[i].transform.GetChild(2).gameObject.GetComponent<Text>().text = connector.wires[i].colorCode.ToString();

                                            // tempTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].colorCode.ToString();
                                            if (connector.wires[i].nodes.Length != 0) {
                                                temp[i].transform.GetChild(3).gameObject.GetComponent<Text>().text = connector.wires[i].nodes[0].details;
                                                //  tempTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].nodes[0].details;
                                            }
                                        }
                                        else {
                                            //temp[i] = Instantiate(Row, Content_Parent.transform);
                                            //temp[i].transform.GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();
                                            //temp[i].transform.GetComponent<Button>().enabled = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            else {
                Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit1 = new RaycastHit();

                if (Physics.Raycast(ray1, out hit1, 10000f)) {
                    //Debug.Log(hit1.collider.gameObject.name);
                    if (hit1.collider.gameObject.tag == "Pin") {
                        hit1.collider.gameObject.GetComponent<ConnectedWire>().ShowWire();
                    }
                }
            }
        }
    }

    private void AssignRemoveAnim() {
        Anim.Play("RemConnector");
        NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.1f + new Vector3(0.15f, 0.15f, -0.15f);
        //NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.15f;
        NewCamPos.LookAt(Anim.transform.position);
        isSmoothing = true;
        //Debug.Log("StartedRemove For Connector" + " " + connector);
    }

    private void AssignAddAnim() {
        Anim.Play("AddConnector");
        NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.1f + new Vector3(0.15f, 0.15f, -0.15f);
        //NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.15f;
        NewCamPos.LookAt(Anim.transform.position);
        isSmoothing = true;
        //Debug.Log("StartedAdd For Connector" + " " + connector);
    }

    public void SelectConnector(GameObject TargetConn) {

        SearchGO.SetActive(false);
        Connector tempConn = TargetConn.GetComponent<Connector>();
        cameraPivot.transform.position = TargetConn.transform.position;


        //cameraPivot.transform.position = cameraPivot.transform.position - transform.forward * 0.05f ;


        NewCamPos.position = tempConn.localPivot.transform.position - tempConn.localPivot.transform.up * 0.1f;

        //Camera.main.transform.position = hit.transform.position - (hit.transform.forward * 0.02f);
        NewCamPos.LookAt(TargetConn.transform.position);

        //Debug.Log(Vector3.Distance(cameraPivot.transform.position, NewCamPos.transform.position));
        //globalHit = hit;
        Ext.distance = 0.05f;
        isSmoothing = true;
        smoothStat = 0;
        Ext.enabled = false;
        //Ext.NewPosRot(hit.transform.position - (hit.transform.forward * 0.2f), Quaternion.LookRotation(hit.transform.position - Ext.transform.position));
        //Ext.NewPosRot(hit.transform.position, Quaternion.LookRotation(hit.transform.position-Ext.transform.position));
        //cameraPivot.transform.LookAt(hit.transform.position);
        //Debug.Log("Connector Selected");

        DisableAllArrows();
        //Arrow1.SetActive(false);
        GlobalWires = FindObjectOfType<GlobalActiveWires>();

        //GlobalWires.DestroyWires();

        Color col = wireMaterial.color;
        col.a = 0.15f;
        wireMaterial.color = col;
        connector = TargetConn.transform.GetComponent<Connector>();

        connector.GetComponent<BoxCollider>().enabled = false;

        if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
            //Debug.Log("Changing start mat");
            AllConn.StartConnMat(connector);
        }

        int tempCount = connector.transform.GetChild(2).childCount;
        //Debug.Log(connector.connectorDesignation);
        tempTag = connector.component.transform.Find("Tag").gameObject;

        /*text.text = "<b>Component:</b>\n" + connector.component.gameObject.name
        + "\n\n<b>Component Designation:</b>\n" + connector.componentDesignation
        //+ "\n\n<b>Connector Part Number:</b>\n" + connector.gameObject.name
        + "\n\n<b>Connector Designation:</b>\n" + connector.connectorDesignation
        + "\n\n<b>Wires:</b>\n" + connector.wires.Length;*/

        /*foreach (Wire w in connector.wires) {
            foreach (Node n in w.nodes) {
                n.model.SetActive(true);
            }
        }*/
        //int cc = 0;
        Table.SetActive(true);
        Add.SetActive(true);
        Remove.SetActive(true);
        //Arrow1.SetActive(true);
        //Reset.SetActive(true);
        Anim = null;
        Anim = connector.transform.GetChild(2).GetComponent<Animator>();

        if (Anim == null)
        {
            Debug.Log("entered");
            Add.SetActive(false);
            Remove.SetActive(false);
        }
        Add.GetComponent<Button>().onClick.AddListener(AssignAddAnim);
        Remove.GetComponent<Button>().onClick.AddListener(AssignRemoveAnim);
        //Arrow.GetComponent<Button>().onClick.AddListener(FlipTheArrow);
        //Reset.GetComponent<Button>().onClick.AddListener(ReloadLevel);
        //Debug.Log(connector);

        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
        temp1.SetActive(true);
        //if (temp1.transform.childCount == 1 && temp1.transform.GetChild(0).GetComponent<Canvas>()) 
        {
            //temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        }

        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
        temp1.SetActive(true);
        //if (temp1.transform.childCount == 1 && temp1.transform.GetChild(0).GetComponent<Canvas>()) 
        {
            //temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        }

        temp1 = connector.component.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;
        //temp1.AddComponent<Canvas>();

        //Vector3 screenPos = Camera.main.WorldToScreenPoint(connector.transform.position);
        //Table.transform.position = screenPos;
        //Table.transform.position = new Vector3(Table.transform.position.x, Table.transform.position.y + 200, Table.transform.position.z);

        temp = new GameObject[connector.wires.Length];

        connectorName.transform.GetComponent<Text>().text = connector.componentDesignation.ToString() + "-" + connector.connectorName.ToString();
        ConnectorDesig.transform.GetComponent<Text>().text = connector.connectorDesignation.ToString();
        //ComponentDesig.transform.GetComponent<Text>().text = connector.componentDesignation.ToString();
        for (int i = 0; i < connector.wires.Length; i++) //filling table data
        {
            if (connector.wires[i].nodes != null) {
                if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
                    hasAtleastOneYes = false;
                }
                else {
                    hasAtleastOneYes = true;
                }

                for (int m = 0; m < connector.wires[i].nodes.Length; m++) {
                    if (connector.wires[i].nodes[m].toInclude) { hasAtleastOneYes = true; }
                }
                if (hasAtleastOneYes) {
                    //Debug.Log(connector.wires[i].nodes.Length);
                    temp[i] = Instantiate(Row, Content_Parent.transform);
                    temp[i].transform.GetComponent<ConnectedWire>().wire = connector.wires[i];
                    temp[i].transform.GetComponent<ConnectedWire>().conn = connector;

                    if (connector.transform.GetChild(2).GetChild(0).GetChild(0).childCount > 1) {
                        connector.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(i).GetComponent<ConnectedWire>().wire = connector.wires[i];
                        connector.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(i).GetComponent<ConnectedWire>().conn = connector;
                    }

                    temp[i].transform.GetChild(0).gameObject.GetComponent<Text>().text = connector.wires[i].wireNumber.ToString();
                    temp[i].transform.GetChild(1).gameObject.GetComponent<Text>().text = connector.wires[i].crossSection.ToString();
                    //tempTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].crossSection.ToString();
                    temp[i].transform.GetChild(2).gameObject.GetComponent<Text>().text = connector.wires[i].colorCode.ToString();
                    //Debug.Log(connector.connectorDesignation);

                    // tempTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].colorCode.ToString();
                    if (connector.wires[i].nodes.Length != 0) {
                        temp[i].transform.GetChild(3).gameObject.GetComponent<Text>().text = connector.wires[i].nodes[0].details;
                        //  tempTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = connector.wires[i].nodes[0].details;
                    }
                }
                else {
                    //temp[i] = Instantiate(Row, Content_Parent.transform);
                    // temp[i].transform.GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();
                    // temp[i].transform.GetComponent<Button>().enabled = false;
                }
            }
        }

    }


    public void DeselectConnector() {
        SearchGO.SetActive(true);
        if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
            SearchGO.GetComponent<SearchBar>().enabled = true;
        }


        connector.GetComponent<BoxCollider>().enabled = true;
        {
            GameObject[] tempArr;
            tempTag.SetActive(false);
            tempArr = GameObject.FindGameObjectsWithTag("Tag");
            foreach (GameObject g in tempArr) {
                g.SetActive(false);
            }
            tempArr = GameObject.FindGameObjectsWithTag("Label");
            foreach (GameObject g in tempArr) {
                g.SetActive(false);
            }
            foreach (Wire w in connector.wires) {
                if (w.nodes != null) {
                    if (w.nodes.Length != 0) {
                        foreach (Node n in w.nodes) {
                            if (n.model != null) {
                                n.model.SetActive(false);
                            }
                        }
                    }
                }
            }
            Debug.Log("Connector Deselected");

            AllConn.RestoreMaterials();

            //foreach (GameObject g in GlobalWires.AllWires)
            {
                //Destroy(g);
                GlobalWires.DestroyWires();
            }
            Table.SetActive(false);
            Add.SetActive(false);
            Remove.SetActive(false);
            //Arrow.SetActive(false);
            DisableAllArrows();
            //Reset.SetActive(false);
            if (temp.Length != 0) {
                foreach (GameObject g in temp) {
                    DestroyImmediate(g);
                }
            }
            temp2 = FindObjectOfType<AllNodes>().AllNodess;
            foreach (GameObject go in temp2) {
                if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
                    if (centralMappingObj.Csv_name != "Mobile Telephone Systems" || (go.name != "N-369" && go.name != "N-B478")) {
                        go.SetActive(false);
                    }
                }

            }
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Label")) {
                if (go.name == "Labels" || go.name == "Labels (1)") {
                    go.SetActive(false);
                }
            }
            FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().RemoveAllHighlights();
            Color col = wireMaterial.color;
            col.a = 1;
            wireMaterial.color = col;
            text.text = "Description goes here";
            connector = null;
        }
        if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
            AllConn.ChangeTOBlack();
            centralMappingObj.ResetConnMat();
        }

    }



    public void FlipTheArrow() {
        if (isDown) {
            //Debug.Log("Entered down");
            FindObjectOfType<ITweenMover>().SwipeDown();
            isDown = false;
            ArrowDown.SetActive(false);
            ArrowUp.SetActive(true);
            //Arrow.transform.rotation = new Quaternion(Arrow.transform.rotation.x, Arrow.transform.rotation.y, -90, 1);
        }
        else {
            //Debug.Log("Entered up");
            FindObjectOfType<ITweenMover>().SwipeUp();
            isDown = true;
            ArrowDown.SetActive(true);
            ArrowUp.SetActive(false);
            //Arrow.transform.rotation = new Quaternion(Arrow.transform.rotation.x, Arrow.transform.rotation.y,90, 1);
        }
    }

    public void DisableAllArrows() {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Arrow")) {
            if (go.name == "arrows") {
                //Debug.Log("Arrows disabled");
                go.SetActive(false);
            }

            //GameObject.Find("arrows").SetActive(false);
        }
    }

    public void ReloadLevel() {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
    }
}
