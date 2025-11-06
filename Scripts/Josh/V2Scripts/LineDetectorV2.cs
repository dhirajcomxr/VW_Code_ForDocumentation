using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LineDetectorV2 : MonoBehaviour {

    public GameObject SearchGO;
    public Material wireMaterial;
    public Text text;
    public GameObject Table;
    public GameObject Add;
    public GameObject Remove;
    public GameObject ArrowDown;    // this is to collapse the table
    public GameObject ArrowUp;      // this is to expand the table
    public ExteriorCam Ext;
    public InputField searchLbl;
    public GameObject Row;
    public GameObject connectorName;
    public GameObject ConnectorDesig;
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
    AllConnectorsSlavia AllConn;
    Camera cam;
    public GameObject[] temp2;
    public GameObject temp1;
    Button temp_btn;
    Animator Anim;
    Outline outline;
    bool isSmoothing;
    public bool isDown = false;
    RaycastHit globalHit;
    public float smoothStat = 0;
    Scene scene;
    CentralHarnessCSVMapping centralMappingObj;
    public Material pinMat;
    public Material harnessMat;
    public Material arrowMat;

    // Start is called before the first frame update
    void Start() {
        
        AllConn = FindObjectOfType<AllConnectorsSlavia>();
        isDown = false;
        Color col = wireMaterial.color;
        col.a = 1;
        wireMaterial.color = col;
        scene = SceneManager.GetActiveScene();
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
    }

    // Update is called once per frame
    void Update() {
        if (isSmoothing == true) {
            smoothStat = smoothStat + Time.deltaTime * speed;
            if (smoothStat >= 1f) {
                //isSmoothing = false;
                SetBoolBack();
            }
            Ext.transform.position = Vector3.Lerp(Ext.transform.position, NewCamPos.transform.position, speed * Time.deltaTime);
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
                    if (Physics.Raycast(ray, out hit, 10000f)) {
                        if (connector != null) {
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
                            GetComponent<GlobalActiveWiresV2>().DestroyWires();
                            Table.SetActive(false);
                            Add.SetActive(false);
                            Remove.SetActive(false);
                            //Arrow.SetActive(false);
                            DisableAllArrows();
                            //Reset.SetActive(false);

                            temp2 = FindObjectOfType<AllNodesV2>().nodes;
                            foreach (GameObject go in temp2) {
                                go.SetActive(false);
                            }
                            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Label")) {
                                if (go.name == "Labels" || go.name == "Labels (1)") {
                                    go.SetActive(false);
                                }
                            }
                            FindObjectOfType<LineDetectorV2>().GetComponent<PartHighlighter>().RemoveAllHighlights();
                            Color col = wireMaterial.color;
                            col.a = 1;
                            wireMaterial.color = col;
                            text.text = "Description goes here";
                            connector = null;
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

                                //GlobalWires.DestroyWires();

                                Color col = wireMaterial.color;
                                col.a = 0.15f;
                                wireMaterial.color = col;
                                connector = hit.transform.GetComponent<Connector>();

                                FindObjectOfType<CentralHarnessMapper>().PopulateRows(connector.gameObject);
                                //if()
                                connector.GetComponent<BoxCollider>().enabled = false;

                                if (scene.name == "Central Wiring Harness (Arpit's final)" && scene.name == "Slavia Central Wiring Harness") {
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
                                Anim = connector.transform.GetChild(2).GetComponent<Animator>();
                                Add.GetComponent<Button>().onClick.AddListener(AssignAddAnim);
                                Remove.GetComponent<Button>().onClick.AddListener(AssignRemoveAnim);
                                //Arrow.GetComponent<Button>().onClick.AddListener(FlipTheArrow);
                                //Reset.GetComponent<Button>().onClick.AddListener(ReloadLevel);
                                //Debug.Log(connector);

                                temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
                                temp1.SetActive(true);
                                //if (temp1.transform.GetChild(0).GetComponent<Canvas>()) {
                                //    temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                                //}
                                temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
                                temp1.SetActive(true);
                                //if (temp1.transform.GetChild(0).GetComponent<Canvas>()) {
                                //    temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                                //}
                                temp1 = connector.component.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;
                                //temp1.AddComponent<Canvas>();
                                //Vector3 screenPos = Camera.main.WorldToScreenPoint(connector.transform.position);
                                //Table.transform.position = screenPos;
                                //Table.transform.position = new Vector3(Table.transform.position.x, Table.transform.position.y + 200, Table.transform.position.z);
                                for (int i = 0; i < connector.wires.Length; i++) //calculating the number of table rows
                                {
                                    cc++;
                                }

                                if (scene.name == "Central Wiring Harness (Arpit's final)" && scene.name == "Slavia Central Wiring Harness") {
                                    hasAtleastOneYes = false;
                                }
                                else {
                                    hasAtleastOneYes = true;
                                }
                                //Debug.Log(cc);
                                cc = 0;
                                connectorName.transform.GetComponent<Text>().text = connector.componentDesignation.ToString() + "-" + connector.componentName.ToString();
                                ConnectorDesig.transform.GetComponent<Text>().text = connector.connectorDesignation.ToString();
                                //ComponentDesig.transform.GetComponent<Text>().text = connector.componentDesignation.ToString();

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
                        if(pinMat != null) {
                            RemoveHighlightPin();
                        }
                        pinMat = hit1.collider.gameObject.GetComponent<MeshRenderer>().material;
                        HighlightPin();
                        Debug.Log(hit1.collider.gameObject.name);
                        GetComponent<CentralHarnessMapper>().ResetTags();
                        DestroyExistingWires();
                        GetComponent<CentralHarnessMapper>().IdentifyRows(connector.gameObject, hit1.collider.transform.GetSiblingIndex() + 1);
                        //hit1.collider.gameObject.GetComponent<SlaviaConnectedWire>().ShowWire();
                    }
                }
            }
        }
    }

    private void AssignRemoveAnim() {
        Anim.Play("RemConnector");
        NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.1f + new Vector3(0.15f, 0.15f, -0.15f);
        NewCamPos.LookAt(Anim.transform.position);
        isSmoothing = true;
    }

    private void AssignAddAnim() {
        Anim.Play("AddConnector");
        NewCamPos.position = Anim.transform.position + Anim.transform.forward * 0.1f + new Vector3(0.15f, 0.15f, -0.15f);
        NewCamPos.LookAt(Anim.transform.position);
        isSmoothing = true;
    }

    public void SelectConnector(GameObject TargetConn) {
        // disable search
        SearchGO.SetActive(false);
        harnessMat.color = new Color(harnessMat.color.r, harnessMat.color.g, harnessMat.color.b, 0.1490196f);

        // get camera angle
        Connector tempConn = TargetConn.GetComponent<Connector>();
        cameraPivot.transform.position = TargetConn.transform.position;
        NewCamPos.position = tempConn.localPivot.transform.position - tempConn.localPivot.transform.up * 0.1f;
        Debug.Log(NewCamPos.position);
        NewCamPos.LookAt(TargetConn.transform.position);
        Ext.distance = 0.05f;
        isSmoothing = true;
        smoothStat = 0;
        Ext.enabled = false;

        // disable arrows
        DisableAllArrows();

        // make harness transparent
        Color col = wireMaterial.color;
        col.a = 0.15f;
        wireMaterial.color = col;

        // get the connector and disable box collider
        connector = TargetConn.transform.GetComponent<Connector>();
        connector.GetComponent<BoxCollider>().enabled = false;

        // find tag and assign
        tempTag = connector.component.transform.Find("Tag").gameObject;

        // enable UI elements
        Table.SetActive(true);
        if(connector.transform.GetChild(2).GetComponent<Animator>() != null) {
            Add.SetActive(true);
            Remove.SetActive(true);
        }     
        

        // connector animation
        Anim = connector.transform.GetChild(2).GetComponent<Animator>();
        Add.GetComponent<Button>().onClick.AddListener(AssignAddAnim);
        Remove.GetComponent<Button>().onClick.AddListener(AssignRemoveAnim);

        // enable front facing labels
        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
        temp1.SetActive(true);
        if (temp1.transform.childCount == 1 && temp1.transform.GetChild(0).GetComponent<Canvas>()) {
            temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        }

        // enable back facing labels
        temp1 = connector.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
        temp1.SetActive(true);
        if (temp1.transform.childCount == 1 && temp1.transform.GetChild(0).GetComponent<Canvas>()) {
            temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        }

        // pins
        temp1 = connector.component.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;

        // update the table title
        connectorName.transform.GetComponent<Text>().text = connector.componentDesignation.ToString() + "-" + connector.componentName.ToString();
        ConnectorDesig.transform.GetComponent<Text>().text = connector.connectorDesignation.ToString();

        // create wires
        if (TargetConn != null) {
            Debug.Log(TargetConn.name);
            FindObjectOfType<CentralHarnessMapper>().PopulateRows(TargetConn);
        }
        else {
            Debug.LogError("Target connector cannot be null");
        }
    }

    public void DeselectConnector() {
        
        StartCoroutine(Deselect());
    }

    IEnumerator Deselect() {
        if(pinMat != null) {
            RemoveHighlightPin();
        }
        harnessMat.color = new Color(harnessMat.color.r, harnessMat.color.g, harnessMat.color.b, 1f);
        GetComponent<CentralHarnessMapper>().CleanUp();

        // searchbar
        searchLbl.text = "";
        SearchGO.SetActive(true);
        SearchGO.GetComponent<SearchBar>().enabled = true;
        GetComponent<CentralHarnessMapper>().ActivateConnectors(true, true);

        yield return new WaitForEndOfFrame();

        // disable tags
        GetComponent<CentralHarnessMapper>().ResetTags();
        tempTag.SetActive(false);
        GameObject[] tempArr;
        tempArr = GameObject.FindGameObjectsWithTag("Tag");
        foreach (GameObject g in tempArr) {
            g.SetActive(false);
        }

        // disable labels
        foreach (GameObject g in GetComponent<CentralHarnessMapper>().allConnectors) {
            foreach (TMP_Text t in g.GetComponentsInChildren<TMP_Text>()) {
                if (t.transform.parent.name.StartsWith("Label")) {
                    t.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        // destroy wires
        GetComponent<GlobalActiveWiresV2>().DestroyWires();

        // reset UI
        Table.SetActive(false);
        Add.SetActive(false);
        Remove.SetActive(false);

        // disable arrows
        DisableAllArrows();

        // disable nodes
        temp2 = GetComponent<AllNodesV2>().nodes;
        DisableNodes(temp2);

        // disable labels
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Label")) {
            if (go.name == "Labels" || go.name == "Labels (1)") {
                go.SetActive(false);
            }
        }

        //FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().RemoveAllHighlights();
        connector = null;
    }
    public void DisableNodes(GameObject[] nodes) {
        // disable nodes
        foreach (GameObject go in nodes) {
            go.SetActive(false);
        }
    }

    // this is to expand/collapse the UI table
    public void FlipTheArrow() {

        if (isDown) {
            FindObjectOfType<ITweenMover>().SwipeDown();
            isDown = false;
            ArrowDown.SetActive(false);
            ArrowUp.SetActive(true);
        }
        else {
            FindObjectOfType<ITweenMover>().SwipeUp();
            isDown = true;
            ArrowDown.SetActive(true);
            ArrowUp.SetActive(false);
        }
    }
    public void DisableAllArrows() {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Arrow")) {
            if (go.name == "arrows") {
                go.SetActive(false);
            }
        }
    }
    public void HighlightPin() {
        pinMat.SetColor("_EmissionColor", new Color(0.6981132f, 0.1191923f, 0.2207547f));
    }

    public void RemoveHighlightPin() {
        if(pinMat != null)
        pinMat.SetColor("_EmissionColor", Color.black);
    }

    public void ReloadLevel() {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
    }
    public void DestroyExistingWires() {
        GetComponent<GlobalActiveWiresV2>().DestroyWires();
        GetComponent<GlobalActiveWiresV2>().AllWires = new List<GameObject>();
    }
}
