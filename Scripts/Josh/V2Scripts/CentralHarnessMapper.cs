using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[System.Serializable]
public class ColumnData {
    public int index = 0;
    public List<GameObject> nodes;
    public List<ConnectorList> connectors;
}

[System.Serializable]
public class ConnectorList {
    public string connDesig;
    public int pin;
    public string compName;
    public string compDesig;
    public GameObject obj;
}

public class CentralHarnessMapper : MonoBehaviour {

    #region Public Vars
    public enum type { connector, node };
    public string cfdName;
    public string variant;
    public GameObject loadingSplash;
    public GameObject pinText;

    public Material active_mat, inactive_mat, defaultWireMat;

    [Header("Row wise Connector and Node details")]
    public List<int> connIndexes;
    public List<ColumnData> allConnectorCols;
    public List<int> nodeIndexes;
    public List<ColumnData> allNodeCols;
    public int detailsIndex;
    public int variantIndex;

    [Header("Unique Connector and Node details")]
    public List<GameObject> allConnectors;

    [Header("UI Elements")]
    public GameObject row;
    public GameObject rowContainer;
    public GameObject invalidPinText;
    private GameObject textObj;

    [Header("Climatic and Climatronic GameObjects")]
    public GameObject climatronic;
    public GameObject climatronicNodes;
    public GameObject climatic;
    public GameObject climaticNodes;

    [SerializeField]
    public List<string> uniquewireList = new List<string>();
    private bool wireAlreadyCreated;
    private List<string> pinList;

    [Header("Temp Vars")]
    public List<GameObject> pinLabels;

    [Space(10)]
    public bool verbose = false;

    #endregion

    #region Private Vars
    AutoCompleteComboBox srch;

    string[] lines;
    string h_connDesig = "Connector Designation";
    string h_node = "Node";
    string nodePrefix = "N-";

    #endregion

    public GameObject t2dh;

    // Start is called before the first frame update
    void Start() {

        if (cfdName.Contains("1LCSV V2") || cfdName.Contains("1.5LCSV V2"))
        {
            Debug.Log("Loading engine harness......");
        }
        else
        {
            cfdName = PlayerPrefs.GetString("cfd");
        }
        
        Debug.Log("Data = CFD Name - "+ cfdName);
        if(cfdName == "Slavia Climatronic KC0 V2")
        {
            climatronic.SetActive(true);
            climatronicNodes.SetActive(true);
        }
        else if (cfdName == "Slavia Climatic (9AB) V2")
        {
            climatic.SetActive(true);
            climaticNodes.SetActive(true);
        }

        if (cfdName == "Climatronic V2")
        {
            Debug.Log("Data =  CFD Name -Climatronic found");
            climatronic.SetActive(true);
            climatronicNodes.SetActive(true);
        }
        else if (cfdName == "Climatic (9AB) V2")
        {
            climatic.SetActive(true);
            climaticNodes.SetActive(true);
        }
        Resources.UnloadUnusedAssets();
        GC.Collect();
        ReadCFD();
    }

    // Update is called once per frame
    void Update() {
    }

    void ReadCFD() {
        nodeIndexes = new List<int>();
        connIndexes = new List<int>();
        pinLabels = new List<GameObject>();

        // read CFD data
        TextAsset data = Resources.Load<TextAsset>(cfdName);
        lines = data.text.Split('\n');

        if (PlayerPrefs.HasKey("variant")) {
            variant = PlayerPrefs.GetString("variant");  //Check the variant
        }
        // extract headers
        string allHeaders = lines[0];
        string[] header = allHeaders.Split(',');

        // find columns which are either nodes or connectors
        for (int i = 0; i < header.Length; i++) {
            if (header[i].ToLower().Contains(h_connDesig.ToLower())) { connIndexes.Add(i); }
            else if (header[i].ToLower().Contains(h_node.ToLower())) { nodeIndexes.Add(i); }
            else if (header[i].ToLower().Contains("details")) { detailsIndex = i; }
            else if(header[i].ToLower().Contains(variant.ToLower())) { variantIndex = i; }
            else if (verbose) { Debug.Log("Match not found: " + header[i].ToString()); }
        }

        // create separate lists for all connector and node columns
        allConnectorCols = new List<ColumnData>(new ColumnData[connIndexes.Count]);
        allNodeCols = new List<ColumnData>(new ColumnData[nodeIndexes.Count]);

        // resize the list to include data for all rows and assign the indexes so they can be compared at a later stage
        for (int i = 0; i < allConnectorCols.Count; i++) {
            allConnectorCols[i] = new ColumnData();
            allConnectorCols[i].index = connIndexes[i];
            allConnectorCols[i].connectors = new List<ConnectorList>(new ConnectorList[lines.Length - 1]);
        }
        for (int i = 0; i < allNodeCols.Count; i++) {
            allNodeCols[i] = new ColumnData();
            allNodeCols[i].index = nodeIndexes[i];
            allNodeCols[i].nodes = new List<GameObject>(new GameObject[lines.Length - 1]);
        }

        // skip the 1st line and start populating the connector and node info
        for (int i = 1; i < lines.Length; i++) {
            string row = lines[i];
            string[] cell = row.Split(',');
            if(cell[variantIndex].Contains("Yes")) {
              //  Debug.Log(cell[variantIndex]);
                for (int j = 0; j < cell.Length; j++) {

                    // if column has NODE data then add to list
                    if (nodeIndexes.Contains(j)) {
                        AddToLocalList(i, j, allNodeCols, cell[j], type.node);
                    }
                    // if column has CONNECTOR data then add to list
                    if (connIndexes.Contains(j)) {
                        AddToLocalList(i, j, allConnectorCols, cell[j], type.connector, cell[j + 3], cell[j + 4], cell[j + 1]);
                    }

                }
            }
            
        }

        // generate lists containing unique connectors and nodes
        allConnectors = new List<GameObject>();
        foreach (ColumnData allCols in allConnectorCols) {
            foreach (ConnectorList c in allCols.connectors) {
                if (c != null) {
                    if (c.obj != null) {
                        allConnectors.Add(c.obj);
                    }
                }
            }
        }
        // remove duplicates
        allConnectors = allConnectors.Distinct().ToList<GameObject>();
        // remove empty
        allConnectors = allConnectors.Where(item => item != null).ToList();
        // set materials for all connectors

        PopulateSearch();
        ActivateConnectors(true, true);
    }

    void AddToLocalList(int r, int c, List<ColumnData> cds, string connDesig, type type, string compDesig = "", string compName = "", string pin = "") {
        bool found = false;
        if (connDesig == "" && compDesig == "") {
            return;
        }
        foreach (ColumnData cd in cds) {
            // identify the correct column
            if (cd.index == c) {
                if (type == type.connector) {
                    // Find and assign the object. Select Component designation if Connector designation is empty
                    GameObject g = GameObject.Find(connDesig != "" ? connDesig : compDesig);
                    found = true;
                    // fill the data in the list
                    ConnectorList cData = new ConnectorList();
                    cData.obj = g;
                    cData.connDesig = connDesig;
                    cData.compName = compName;
                    cData.compDesig = compDesig;
                    if (pin != "") {
                        try {
                            cData.pin = int.Parse(pin);
                        }
                        catch (Exception e) { Debug.LogError("Invalid Pin Number @ " + c + "," + r); }
                    }
                    else {
                        cData.pin = -1; // because 0 is for ground points/lugs
                    }
                    cd.connectors[r - 1] = cData;

                    // fill the data for connector
                    try {
                        g.GetComponent<Connector>().connectorDesignation = connDesig;
                        g.GetComponent<Connector>().componentName = compName;
                        g.GetComponent<Connector>().componentDesignation = compDesig;
                    }
                    catch (Exception e) {
                        Debug.LogError("Info not applied for: " + connDesig + ". ==> " + e.ToString()
                            + "\nRow : " + r
                            + "\nCol : " + c
                            + "\nConnDesig: " + connDesig
                            + "\nCompDesig: " + compDesig
                            + "\nCompName : " + compName);
                    }
                }
                else if (type == type.node) {
                    GameObject g = GameObject.Find(connDesig);
                    cd.nodes[r - 1] = g;
                }
            }
        }
        if (!found && verbose) {
            Debug.Log("<color=yellow>Object Not Found: " + connDesig + "</color>");
        }
    }

    void PopulateSearch() {
        // maintain 2 lists and merge them later because we want the connector designations on top when searching
        List<string> connList = new List<string>();
        List<string> compNameList = new List<string>();
        foreach (GameObject g in allConnectors) {
            Connector c = g.GetComponent<Connector>();
            string connDesig = "";
            if (c!=null)
            {
                connDesig = c.connectorDesignation != "" ? c.connectorDesignation : c.componentDesignation;
                connList.Add(connDesig);
                if (c.componentName != "")
                {
                    compNameList.Add(c.componentName + " (" + connDesig + ")");
                }
                else
                {
                    Debug.LogError("Missing Component Name for: " + c.gameObject.name);
                }
            }
           
        }

        connList.Sort();
        compNameList.Sort();

        // add component names to the connector list
        foreach (string s in compNameList) {
            connList.Add(s);
        }

        srch = FindObjectOfType<AutoCompleteComboBox>();
        srch.SelectableConns = allConnectors;
        srch.AvailableOptions = connList;
        srch.RebuildPanel();
    }

    public void ActivateConnectors(bool setActiveMat, bool enableColliders) {
        foreach (GameObject g in allConnectors) {

            Renderer[] rend = g.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rend) {
                if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                    r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)" || r.material.name == "Font Material" || r.material.name == "Node_Mat" || r.material.name == "arrow (Instance)") {
                    continue;
                }
                if(r.materials.Length > 1) {
                    Material[] mats = {active_mat, active_mat, active_mat, active_mat };
                    r.materials = mats;
                }
                else
                r.material = setActiveMat ? active_mat : inactive_mat;

                g.GetComponent<Collider>().enabled = enableColliders;
            }
        }
    }

    public void PopulateRows(GameObject conn) {
        // clear existing rows if any
        while (rowContainer.transform.childCount > 0) {
            DestroyImmediate(rowContainer.transform.GetChild(0).gameObject);
        }

        string desig = conn.GetComponent<Connector>().connectorDesignation;

        if(desig == "") {
            desig = conn.GetComponent<Connector>().componentDesignation;    //assign component designation if connector designation is blank(for fuses)
        }
        List<int> pins = new List<int>();

        // identify available pins
        // check every column
        foreach (ColumnData colData in allConnectorCols) {
            // and every row in that column
            for (int i = 0; i < colData.connectors.Count; i++) {
                if (colData.connectors[i] != null) {
                    // create a row only if designation matches
                    if (colData.connectors[i].connDesig == desig || colData.connectors[i].compDesig == desig) {
                        // store row content in an array
                        string[] rowdata = lines[i + 1].Split(',');
                        int pinNo = 0;

                        if (colData.connectors[i].connDesig == desig) {
                            // check if it is a connector
                            pinNo = colData.connectors[i].pin;
                        }
                        else if (colData.connectors[i].compDesig == desig) {
                            // or a lug or a ground/earth point
                            pinNo = 0;
                        }
                        if (!pins.Contains(pinNo)) {
                            pins.Add(pinNo);
                            if (rowContainer.transform.Find(pinNo + "") == null) {
                                GameObject g = Instantiate(row);

                                // get the data
                                string crossSec = rowdata[colData.index < 5 ? (colData.index + 6) : (colData.index - 1)];
                                string colorCode = rowdata[colData.index < 5 ? (colData.index + 5) : (colData.index - 2)];
                                if (crossSec == "") { Debug.LogError("CrossSection cannot be empty for: " + conn.name); }
                                if (colorCode == "") { Debug.LogError("ColorCode cannot be empty for: " + conn.name); }

                                // assign the data
                                g.transform.GetChild(1).GetComponent<Text>().text = crossSec;
                                g.transform.GetChild(2).GetComponent<Text>().text = colorCode;
                                g.transform.GetChild(3).GetComponent<Text>().text = rowdata[detailsIndex];
                                g.transform.GetChild(0).GetComponent<Text>().text = (pinNo < 0) ? "-" : pinNo + "";
                                g.transform.SetParent(rowContainer.transform);
                                g.transform.localScale = new Vector3(1, 1, 1);
                                g.name = pinNo + "";
                                g.GetComponent<RowInfoV2>().rows.Add(i + 1);
                                g.GetComponent<RowInfoV2>().pin = pinNo;
                            }
                            else {
                                rowContainer.transform.Find(pinNo + "").GetComponent<RowInfoV2>().rows.Add(i + 1);
                            }
                        }
                        else {
                            if (verbose) {
                                Debug.Log("Repeated entry for Pin: " + pinNo);
                            }
                        }
                    }
                }
            }
        }

        // distinct and sort
        pins = pins.Distinct().ToList<int>();
        pins.Sort();
        for (int i = 0; i < pins.Count; i++) {
            rowContainer.transform.Find(pins[i] + "").SetSiblingIndex(i);
        }
    }

    public void IdentifyRows(GameObject conn, int Pin) {
        CleanUp();
        uniquewireList = new List<string>();
        pinList = new List<string>();
        List<int> validRows = new List<int>();
        // check all rows
        for (int i = 0; i < lines.Length; i++) {
            string[] cells = lines[i].Split(',');
            // check all cells within a row
            for (int j = 0; j < cells.Length; j++) {
                if (cells[j] == conn.name) {
                    // check whether the given cell belongs to the connector designation column
                    bool isConn = false;
                    foreach (int ind in connIndexes) {
                        if (ind == j) { isConn = true; }
                    }

                    // get the column index for the pin number
                    int pinIndex = 0;
                    if (isConn) {
                        pinIndex = j + 1;
                        // read the pin number and check if it's a match
                        int pinNo;
                        bool isValidPin = int.TryParse(cells[pinIndex], out pinNo);
                        if (isValidPin) {
                            if (pinNo == Pin) {
                                validRows.Add(i);
                            }
                        }
                    }
                    else {
                        // probably a fuse or a lug/ground point
                        validRows.Add(i);
                    }
                }
            }
        }
        // Load all applicable rows 
        foreach (int i in validRows) {
            LoadRow(i);
            if (verbose) { Debug.Log("Loading Row " + i); }
        }
        if (validRows.Count == 0) {
            invalidPinText.SetActive(true);
            Invoke("DisableErrorText", 3f);
            FindObjectOfType<LineDetectorV2>().RemoveHighlightPin();
            Debug.LogError("No data for: " + conn.name + ". Pin: " + Pin);
        }

        //Generate pin number labels if the pins are greater than 20
        if(conn.transform.GetChild(2).GetChild(0).GetChild(0).childCount > 20) {
            Destroy(textObj);
            GameObject obj;
             GameObject pinPlane = conn.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(Pin - 1).gameObject;
             obj = Instantiate(pinText, pinPlane.transform.position, pinPlane.transform.rotation);
            obj.transform.GetChild(0).GetComponent<TextMeshPro>().text = Pin.ToString();
            textObj = obj;
        } else {
            Debug.Log("Did not create Text");
        }

    }

    public void LoadRow(int r) {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log(lines[r]);
        string[] cells = lines[r].Split(',');
        GameObject a = null, b = null;
        string p1 = "", p2 = ""; // store the pin numbers
        string matName = "";
        string crossSec = "";

        for (int i = 0; i < cells.Length; i++) {
            bool isConnector = false;
            bool isNode = false;

            for (int j = 0; j < allConnectorCols.Count; j++) {
                if (allConnectorCols[j].index == i) { isConnector = true; }
            }
            for (int j = 0; j < allNodeCols.Count; j++) {
                if (allNodeCols[j].index == i) { isNode = true; }
            }

            if (isConnector || isNode) {
                if (a == null) {
                    // assign data if it is a node or a connector
                    if (isConnector || isNode) {
                        string objName = cells[i];
                        // if connector designation is empty assign component designation
                        if (cells[i] == "") {
                            objName = cells[i + 3];
                        }
                        if (objName != "") {
                            a = FindObj(objName, isNode ? type.node : type.connector);
                            p1 = isConnector ? cells[i + 1] : "";

                            if (a == null) { Debug.LogError(objName + " not found ==> " + i); }
                        }
                    }
                }
                else if (b == null) {
                    // assign data if it is a node or a connector
                    if (isConnector || isNode) {
                        string objName = cells[i];
                        // if connector designation is empty assign component designation
                        if (isConnector && cells[i] == "") {
                            objName = cells[i + 3];
                        }
                        if (objName != "") {
                            b = FindObj(objName, isNode ? type.node : type.connector);
                            p2 = isConnector ? cells[i + 1] : "";

                            if (b == null) { Debug.LogError(objName + " not found ==> " + i); }

                            // fetch material and cross section info
                            crossSec = cells[i - 1];
                            matName = cells[i - 2].Trim();

                            // pass the data for wire creation
                            ParseWireInfo(a, p1, b, p2, matName, crossSec, true, cells[detailsIndex]);


                            // show arrows
                            ShowArrows(a, cells[detailsIndex]);
                            ShowArrows(b, cells[detailsIndex]);
                        }
                    }
                }
                else {
                    // swap out the vars
                    a = b;
                    p1 = p2;
                    // assign data if it is a node or a connector
                    if (isConnector || isNode) {
                        string objName = cells[i];
                        // if connector designation is empty assign component designation
                        if (isConnector && cells[i] == "") {
                            objName = cells[i + 3];
                        }
                        if (objName != "") {
                            b = FindObj(objName, isNode ? type.node : type.connector);
                            p2 = isConnector ? cells[i + 1] : "";

                            if (b == null) { Debug.LogError(objName + " not found ==> " + i); }

                            // fetch material info
                            if (cells[i - 2] != "") {
                                crossSec = cells[i - 1];
                                matName = cells[i - 2].Trim();
                            }

                            // pass the data for wire creation
                            ParseWireInfo(a, p1, b, p2, matName, crossSec, false, cells[detailsIndex]);

                            // show arrows
                            ShowArrows(a, cells[detailsIndex]);
                            ShowArrows(b, cells[detailsIndex]);
                        }
                    }
                }
            }
        }
    }

    GameObject FindObj(string s, type t) {
        GameObject obj = gameObject;
        if (t == type.node) {
            s = nodePrefix + s;
            Debug.Log("Looking for Node: " + s);
            foreach (GameObject g in gameObject.GetComponent<AllNodesV2>().nodes) {
                if (g.name.ToLower() == s.ToLower()) { obj = g; }
            }

            if (obj == gameObject) { return null; }
            else { return obj; }
        }
        else {
            Debug.Log("Looking for Connector: " + s);
            obj = GameObject.Find(s);
            return obj;
        }
    }

    void CheckForDuplicateWires(string from, string fromPin, string to, string toPin) {
        wireAlreadyCreated = false;
        for (int i = 0; i < uniquewireList.Count; i++) {
            if (uniquewireList[i].ToLower() == ("From " + from + " pin " + fromPin + " to " + to + " pin " + toPin).ToLower()) {
                wireAlreadyCreated = true;
            }
        }
    }

    public void ParseWireInfo(GameObject from, string fromPin, GameObject to, string toPin, string wireColor, string crossSec, bool showBothTags, string details) {

        
        

        // find and assign correct material
        wireColor = wireColor.Replace("/", "-");
        Material mat = defaultWireMat;
        foreach (Material m in GetComponent<AllMatsV2>().mats) {
            if (m.name.ToLower() == wireColor.ToLower()) { mat = m; }
        }

        if (verbose) { Debug.Log(from.name + " --> " + to.name); }

        // enable connector/node
        from.SetActive(true);
        to.SetActive(true);

        // get the path point (gizmo) connected to the connector/node
        GameObject startPt = from.GetComponent<GizmoMapper>().connectedGizmo;
        GameObject endPt = to.GetComponent<GizmoMapper>().connectedGizmo;


        // for the FROM connector
        try {
            GameObject pins = from.transform.GetChild(2).transform.GetChild(0).transform.Find("Pins").gameObject;
            GameObject frontLabels = from.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
            GameObject backlabels = from.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;

            // enable the Labels
            frontLabels.SetActive(true);
            backlabels.SetActive(true);
            int pin = int.Parse(fromPin);

            // turn wires to the correct pins if data is present
            if (pin <= 16) {
                if (backlabels.transform.GetChild(pin - 1)) {
                    if (backlabels.transform.GetChild(pin - 1).GetComponent<GizmoMapper>()) {
                        startPt = backlabels.transform.GetChild(pin - 1).GetComponent<GizmoMapper>().connectedGizmo;
                    }
                }
            }
            // show pin number if connector has more than 20 pins
            if (pin > 0 && pins.transform.childCount > 20 && frontLabels.transform.childCount > 0) {
                GameObject pinLabel = Instantiate(frontLabels.transform.GetChild(0).gameObject);
                pinLabel.transform.SetParent(frontLabels.transform);
                pinLabel.transform.position = pins.transform.GetChild(pin - 1).transform.position;
                pinLabel.GetComponent<TMP_Text>().text = fromPin;
            }
        }
        catch (Exception e) { if (verbose) { Debug.LogError("Error Turning Wire to correct Pin for: " + from.name + ". Err=" + e.ToString()); } }

        // for the TO connector
        try {
            GameObject pins = to.transform.GetChild(2).transform.GetChild(0).transform.Find("Pins").gameObject;
            GameObject frontLabels = to.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
            GameObject backlabels = to.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;

            // enable the Labels
            frontLabels.SetActive(true);
            backlabels.SetActive(true);
            int pin = int.Parse(toPin);

            // turn wires to the correct pins if data is present
            if (pin <= 16) {
                if (backlabels.transform.GetChild(pin - 1)) {
                    if (backlabels.transform.GetChild(pin - 1).GetComponent<GizmoMapper>()) {
                        endPt = backlabels.transform.GetChild(pin - 1).GetComponent<GizmoMapper>().connectedGizmo;
                    }
                }
            }
            // show pin number if connector has more than 20 pins
            if (pin > 0 && pins.transform.childCount > 20 && frontLabels.transform.childCount > 0) {
                GameObject pinLabel = Instantiate(frontLabels.transform.GetChild(0).gameObject);
                pinLabel.transform.SetParent(frontLabels.transform);
                pinLabel.transform.position = pins.transform.GetChild(pin - 1).transform.position;
                pinLabel.GetComponent<TMP_Text>().text = toPin;
            }
        }
        catch (Exception e) { if (verbose) { Debug.LogError("Error Turning Wire to correct Pin for: " + to.name + ". Err=" + e.ToString()); } }

        CheckForDuplicateWires(from.name, fromPin, to.name, toPin);

        // finally create the wire
        if (!wireAlreadyCreated) {
            GetComponent<PathFinderV2>().CreateWire(startPt, endPt, 2, mat);
            uniquewireList.Add("From " + from.name + " pin " + fromPin + " to " + to.name + " pin " + toPin);
        }
        

        // avoid repeated entry in tags when creating wires between 1-2, 2-3, 3-4 and so on...
        if (showBothTags) {
            // enable Tags for FROM connector
            if (from.GetComponentInChildren<LookAtForTags>(true) != null) {

                ShowTags(from, crossSec, wireColor, details, from.GetComponent<Connector>().connectorDesignation, fromPin, from.GetComponent<Connector>().componentDesignation);
            }
            else {
                if (!from.name.StartsWith("N-") && verbose) { Debug.LogError("Tag not found for: " + from.name); }
            }
        }

        // enable Tags for TO connector
        if (to.GetComponentInChildren<LookAtForTags>(true) != null) {
            
            ShowTags(to, crossSec, wireColor, details, to.GetComponent<Connector>().connectorDesignation, toPin, to.GetComponent<Connector>().componentDesignation);
        }
        else {
            if (!to.name.StartsWith("N-") && verbose) { Debug.LogError("Tag not found for: " + to.name); }
        }

    }

    void ShowTags(GameObject conn, string cross, string wirecolor, string details, string connDesig, string pin, string compDesig) {
        if (cross == "") { cross = "-"; }
        if (wirecolor == "") { wirecolor = "-"; }
        if (details == "") { details = "-"; }
        if (connDesig == "") { connDesig = "-"; }
        if (pin == "") { pin = "-"; }
        if (compDesig == "") { compDesig = "-"; }

        // get the Tag
        Transform t = conn.GetComponentInChildren<LookAtForTags>(true).transform;

        // append the data incase the data is already present
        // this is for connectors which have wires mapped to multiple pins
        if (t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text != "-" && t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text != "") {
            if (cross != t.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text) {
                cross = t.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text + " and " + cross;
            } else {
                cross = t.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text;
            }

            if (wirecolor != t.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text) {
                wirecolor = t.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text + " and " + wirecolor;
            } else {
                wirecolor = t.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text;
            }
            if (pin != t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text && !t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text.StartsWith(pin + " and ")) {

                pin = t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text + " and " + pin;

            } else {
                pin = t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text;
            }
        }

        // update the tag
        t.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = cross;        // cross
        t.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wirecolor;    // color
        t.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = details;      // details
        t.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = connDesig;    // conn desig
        t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = pin;          // pin
        t.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = compDesig;    // comp desig
        t.gameObject.SetActive(true);
    }

    void ShowArrows(GameObject conn, string details) {
        if (conn == null && verbose) {
            Debug.LogError("Error showing arrows: Connector cannot be null");
        }

        // identify the direction of the arrow and enable it accordingly
        bool towardsConn = details == "G"
                        || details == "Gnd"
                        || details == "-ve"
                        || details == "+5V"
                        || details == "+12V - ckt. 87"
                        || details == "+12V-ckt .87";

        Transform arrows = conn.transform.Find("arrows");
        if (arrows != null) {
            arrows.gameObject.SetActive(true);
            arrows.GetChild(0).gameObject.SetActive(!towardsConn);
            arrows.GetChild(1).gameObject.SetActive(towardsConn);
        }
        else {
            if (verbose) { Debug.LogError("Arrows not found for:" + conn.name); }
        }
    }

    /*void  DisableLoadingSpash() {
        loadingSplash.SetActive(false);
    }*/
    void DisableErrorText() {
        invalidPinText.SetActive(false);
    }

    public void ResetTags() {

        foreach(GameObject g in allConnectors) {
            Transform t = g.GetComponentInChildren<LookAtForTags>(true).transform;
            if (t.gameObject.activeInHierarchy) {
                t.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "-";        // cross
                t.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "-";    // color
                t.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "-";      // details
                t.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "-";    // conn desig
                t.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "-";          // pin
                t.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = "-";
                t.gameObject.SetActive(false);
            }
        }
    }

    public void CleanUp() {
        // clear out any pin labels created at rumtime
        for (int i = 0; i < pinLabels.Count; i++) {
            DestroyImmediate(pinLabels[i].gameObject);
        }
        pinLabels = new List<GameObject>();

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    

}

/*
    Pending
        - Dropdown loading
	    - Wire turning
    - Multi wire testing
		- Tags
        - Arrows    
    - Duplicate removal
    - Highlighting
        - Raycasting
    - Cleaning up
        - Colliders and position for all SC fuses to be fixed so we can raycast it. Similar tok SC45.
*/

