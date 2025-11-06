using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IndieMarc.CurvedLine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ConnectedWire : MonoBehaviour {
    public GameObject model;
    public Wire wire;
    public GameObject[] allEndObjs;
    GameObject temp1, temp2, component;
    public Connector conn;
    string tempStr;
    GameObject[] tempGO, temp;
    GameObject temp12;
    GameObject tempNodeGO;
    GameObject tempNodeGizmo;
    GameObject TempEndComp;
    GlobalActiveWires GlobalWires;
    PathFinder pF;
    public GameObject prefabWire;
    public Material ConnMat;
    public Material mappedMat;
    GameObject tempWire;
    Material mat;
    string[] str;
    string mat_str;
    //string csvLine;
    Material[] tempMatArray;
    Renderer[] rend;
    AllConnectors AllC;
    LineDetector LD;
    LabelChanger LC;
    GameObject TempText;
    GameObject TempFuse;
    int count = 0;
    float XOffset = 0;
    float YOffset = 0;
    float ZOffset = 0;
    GameObject endTag;
    GameObject endPin;
    CentralHarnessCSVMapping centralCsv;
    Scene scene;
    GameObject startingPt, nodePt, endPt;

    private void Start() {
        centralCsv = FindObjectOfType<CentralHarnessCSVMapping>();
        LD = FindObjectOfType<LineDetector>();
        LC = FindObjectOfType<LabelChanger>();
        //scene = SceneManager.GetActiveScene();

        if (SceneManager.sceneCount > 1)
            scene = SceneManager.GetSceneAt(1);
        else
            scene = SceneManager.GetSceneAt(0);

        //Debug.Log(scene.name);
        if (scene.name != "Central Wiring Harness (Arpit's final)" && scene.name != "Taigun Central Harness") // only for 1L and 1.5L scene
        {
            temp12 = GameObject.Find("5Q0_906_379");

            count = temp12.transform.GetChild(2).GetChild(0).GetChild(2).childCount;

            for (int i = 0; i < count - 1; i++) {
                temp12.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(i + 1).gameObject.SetActive(false);
            }
        }


        count = 0;

        GlobalWires = FindObjectOfType<GlobalActiveWires>();
        AllC = FindObjectOfType<AllConnectors>();
        ConnMat = AllC.ConnectorMat;
        mappedMat = AllC.MappedConnectorMat;
    }

    public void DisableNodes()
    {
        tempGO = FindObjectOfType<AllNodes>().AllNodess;

        if (tempGO != null)
        {
            foreach (GameObject go in tempGO)
            {
                if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness")
                {
                    if ((centralCsv.Csv_name != "Mobile Telephone Systems" || (go.name != "N-369" && go.name != "N-B478")) && (centralCsv.Csv_name != "Panoramic Sliding Sunroof" || (go.name != "N-B341" && go.name != "N-386")) && (centralCsv.Csv_name != "Climatronic" || go.name != "N-B317" && go.name != "N-B699") && (centralCsv.Csv_name != "Multifunction Steering Wheel" || go.name != "N-367"))
                    {
                        go.SetActive(false);
                    }
                }
            }
        }
    }

    public void ShowWire() {

        // clear the tags
        LookAtForTags tag;
        GameObject[] currentConns = FindObjectOfType<AllConnectors>().currentEnPtObjs;
        foreach (GameObject c in currentConns) {
            if (c != null) {
                tag = c.GetComponentInChildren<LookAtForTags>(true);
                tag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "-";
                tag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "-";
                tag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "-";
                tag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "-";
                tag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "-";
                tag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = "-";
                Debug.Log("Data Cleared");
            }
        }
        GameObject mainConn = FindObjectOfType<LineDetector>().connector.gameObject;
        if (mainConn != null) {
            tag = mainConn.GetComponentInChildren<LookAtForTags>(true);
            tag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "-";
            tag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "-";
            tag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "-";
            tag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "-";
            tag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "-";
            tag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = "-";
        }

        //set Tag to active
        LD.tempTag.SetActive(true);
        // reset everything
        temp = GameObject.FindGameObjectsWithTag("Tag");
        foreach (GameObject g in temp) // disable all previous tags
        {
            g.SetActive(false);
        }
        // enable the tag for selected connector
        temp12 = conn.component.transform.Find("Tag").gameObject;
        temp12.SetActive(true);

        // disable all previous front and back labels
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Label")) {
            go.SetActive(false);
        }

        // enable the front and back labels of the selected connector
        conn.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject.SetActive(true);
        conn.component.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject.SetActive(true);

        startingPt = null;
        nodePt = null;

        LD.DisableAllOutlineEffects();
        LD.DisableAllWires();
        LD.DisableAllArrows();
        LD.GetComponent<PartHighlighter>().RemoveAllHighlights();


        // Destroy all existing wires
        GlobalWires.DestroyWires();

        // check if connector is null
        if (conn == null) { return; }

        pF = FindObjectOfType<PathFinder>();
        tempMatArray = FindObjectOfType<AllMats>().mats;
        component = conn.component.gameObject;
        // assign starting point. if no gizmo mapper script present on the back labels, assign gizmo mapper of the connector script, if back labels have gizmo mapper, assign correct value according to the pin
        if (component.GetComponent<GizmoMapper>() != null) {
            // see if canvas is the child (only for 1L and 1.5L scene)
            if (conn.transform.GetChild(2).GetChild(0).GetChild(2).childCount == 1) {
                startingPt = conn.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(0).GetChild(wire.wireNumber - 1).GetComponent<GizmoMapper>().connectedGizmo;
            }
            else if (conn.transform.GetChild(2).GetChild(0).GetChild(2).childCount == 0 || conn.transform.GetChild(2).GetChild(0).GetChild(2).childCount != conn.transform.GetChild(2).GetChild(0).GetChild(0).childCount + 1) {
                startingPt = conn.transform.GetComponent<GizmoMapper>().connectedGizmo;
            }
            else {
                startingPt = conn.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(wire.wireNumber - 1).GetComponent<GizmoMapper>().connectedGizmo;
            }
        }

        // enable its arrows
        temp2 = conn.component.transform.Find("arrows").gameObject;
        temp2.SetActive(true);

        // disable all existing nodes

        DisableNodes();

        // check if the wire contains a node
        if (wire.nodes[0].NodeSTR != null && component.GetComponent<GizmoMapper>() != null) {

            tempGO = FindObjectOfType<AllNodes>().AllNodess;

            tempStr = wire.nodes[0].NodeSTR;
            foreach (GameObject go in tempGO) {
                // find and enable that node
                if (go.name == tempStr) {
                    go.SetActive(true);
                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                    break;
                }
            }
        }


        // identify the direction of the arrow and enable it accordingly
        bool arrowDirection = wire.nodes[0].details == "G"
                        || wire.nodes[0].details == "Gnd"
                        || wire.nodes[0].details == "-ve"
                        || wire.nodes[0].details == "+5V"
                        || wire.nodes[0].details == "+12V - ckt. 87"
                        || wire.nodes[0].details == "+12V-ckt .87";
        temp2.transform.GetChild(0).gameObject.SetActive(!arrowDirection);
        temp2.transform.GetChild(1).gameObject.SetActive(arrowDirection);

        // identify and assign the material
        str = wire.colorCode.Split('/');
        if (str.Length == 2) {
            mat_str = str[0] + "-" + str[1];
        }
        else {
            mat_str = str[0];
        }

        foreach (Material m in tempMatArray) {
            if (m.name.ToLower() == mat_str.ToLower()) {
                //Debug.Log("FOUND");
                mat = m;
            }
        }

        //From pin label to starting point
        //pF.CreateWire(conn.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(0).GetChild(wire.wireNumber - 1).GetComponent<GizmoMapper>().connectedGizmo, startingPt, 2, mat);

        // create a wire from the starting point to node if a node is found
        if (nodePt != null) {
            if (startingPt == null) { Debug.Log("Starting pt null"); }

            pF.CreateWire(startingPt, nodePt, 2, mat);

        }

        // check if the wire splits into multiple wires via a node
        // after splitting each individual wire is considered as a node here...
        if (LD.tempTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == "-"
         && LD.tempTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text == "-") {

            LD.tempTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = wire.crossSection.ToString();
            LD.tempTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wire.colorCode.ToString();
            LD.tempTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[0].details;
            LD.tempTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = conn.connectorDesignation;
            LD.tempTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = wire.wireNumber.ToString();
            LD.tempTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = conn.componentDesignation;
        }
        else {

            LD.tempTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.crossSection.ToString();
            LD.tempTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.colorCode.ToString();
            LD.tempTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[0].details;
            LD.tempTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text += " and " + conn.connectorDesignation;
            LD.tempTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.wireNumber.ToString();
            LD.tempTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = conn.componentDesignation;
        }
        allEndObjs = new GameObject[wire.nodes.Length + 2];

        tempGO = FindObjectOfType<AllNodes>().AllNodess;

        for (int i = 0; i < wire.nodes.Length; i++) {
            if (component.GetComponent<GizmoMapper>() != null) {

                if (wire.nodes[i].toInclude || (scene.name != "Central Wiring Harness (Arpit's final)" && scene.name != "Taigun Central Harness")) // if that wire has to be included, then only preceed
                {
                    if ((scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") && centralCsv.Node2 && wire.nodes[i].Node2STR != null) {
                        Debug.Log(wire.nodes[i].Node2STR);
                        foreach (GameObject g in tempGO) {
                            if (g.name == wire.nodes[i].Node2STR) {
                                //Debug.Log(g.name);
                                g.SetActive(true);
                                tempNodeGO = g;
                            }
                        }

                        tempNodeGizmo = tempNodeGO.GetComponent<GizmoMapper>().connectedGizmo;

                        pF.CreateWire(nodePt, tempNodeGizmo, 2, mat); // create wire from node pt to extra node pt

                        nodePt = tempNodeGizmo; // assign previous starting point as end point to continue creating wire from there

                        if (centralCsv.Node3) // if that wire has to be included, then only preceed
                        {
                            foreach (GameObject g in tempGO) {
                                if (g.name == wire.nodes[i].Node3STR) {
                                    Debug.Log("node3 " + g.name);
                                    g.SetActive(true);
                                    tempNodeGO = g;
                                }
                            }

                            //tempNodeGO = GameObject.Find(wire.nodes[i].Node3STR);
                            tempNodeGizmo = tempNodeGO.GetComponent<GizmoMapper>().connectedGizmo;

                            pF.CreateWire(nodePt, tempNodeGizmo, 2, mat); // create wire from node pt to extra node pt


                            nodePt = tempNodeGizmo; // assign previous starting point as end point to continue creating wire from there
                        }
                    }
                    if (wire.nodes[i].endPointObj != null && (wire.nodes[i].endPointObj.tag != "Node" || wire.nodes[i].endPointObj.name == "N-B341" || wire.nodes[i].endPointObj.name == "672" || wire.nodes[i].endPointObj.name == "639" || wire.nodes[i].endPointObj.name == "685" || wire.nodes[i].endPointObj.name == "673" || wire.nodes[i].endPointObj.name == "N-B407" || wire.nodes[i].endPointObj.name == "N-B398")) {
                        allEndObjs[i] = wire.nodes[i].endPointObj;
                        if (allEndObjs[i] != null) {
                            //Debug.Log(allEndObjs[i]);
                            if (allEndObjs[i].transform.Find("Tag")) {
                                endTag = allEndObjs[i].transform.Find("Tag").gameObject;
                            }
                            if (endTag != null && endTag.transform.childCount > 1) // assign end tag values
                            {
                                if (wire.nodes[i].nodeCrossSection != 0) {
                                    if (endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].nodeCrossSection.ToString();
                                    }
                                    else {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].nodeCrossSection.ToString();
                                    }
                                }
                                else {
                                    if (endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = wire.crossSection.ToString();
                                    }
                                    else {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.crossSection.ToString();
                                    }
                                }

                                if (wire.nodes[i].nodeColorCode != "") {
                                    if (endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].nodeColorCode.ToString();
                                    }
                                    else {
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].nodeColorCode.ToString();
                                    }
                                }
                                else {
                                    if (endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wire.colorCode.ToString();
                                    }
                                    else {
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].nodeColorCode.ToString();
                                    }
                                }

                                if (endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                    endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].details;
                                }
                                else {
                                    endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].details;
                                }

                                if (wire.nodes[i].endPointObj.name != "43") {
                                    if (endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endPointObj.name;
                                    }
                                    else {
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].endPointObj.name;
                                    }
                                }

                                if (endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                    endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endPointPinNum.ToString();
                                }
                                else {
                                    endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].endPointPinNum.ToString();
                                }

                                if (endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                    endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endComponentDesignation;
                                }
                                else {
                                    endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endComponentDesignation;
                                }

                                if (wire.nodes[i].endPointObj.name == "43") {
                                    if (endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text == "-") {
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endPointObj.name;
                                    }
                                    else {
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endPointObj.name;
                                    }
                                }
                                endTag.SetActive(true);
                            }
                        }

                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && centralCsv.Csv_name == "Ent&Start Auth (4I3)" && conn.name != "T4bx" && conn.name == "T73c" && i != 0 && (wire.wireNumber == 46 || (wire.wireNumber == 47))) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].NodeSTR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }

                        // some extra node have nested nodes, with different variations, have hard coded those values for particular cfds
                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && centralCsv.Csv_name == "Radio System(8UK,8RU)" && conn.name != "T4bz" && conn.name == "T12g" && i != 0 && (wire.wireNumber == 6 || (wire.wireNumber == 12))) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].NodeSTR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }

                        // some extra node have nested nodes, with different variations, have hard coded those values for particular cfds

                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && centralCsv.Csv_name == "Basic Equipment" && (conn.name == "T73a" && i == 4 && wire.wireNumber == 1)) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].Node2STR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }

                        // some extra node have nested nodes, with different variations, have hard coded those values for particular cfds

                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && centralCsv.Csv_name == "Basic Equipment" && (conn.name == "T2fs" && i == 3 && wire.wireNumber == 2)) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].NodeSTR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }

                        // some extra node have nested nodes, with different variations, have hard coded those values for particular cfds

                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && (centralCsv.Csv_name == "CS Power Window (4R1)" || centralCsv.Csv_name == "CS Power Window (4R3)") && (conn.name == "T38xa" && i == 3 && wire.wireNumber == 2)) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].NodeSTR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }

                        // some extra node have nested nodes, with different variations, have hard coded those values for particular cfds

                        if ((scene.name == "Taigun Central Harness" || scene.name == "Central Wiring Harness (Arpit's final)") && centralCsv.Csv_name == "LED Headlights(8IY)" && conn.name == "T3d" && i != 0 && wire.wireNumber == 1) {
                            tempGO = FindObjectOfType<AllNodes>().AllNodess;

                            tempStr = wire.nodes[i].NodeSTR;
                            foreach (GameObject go in tempGO) {
                                // find and enable that node
                                if (go.name == tempStr) {
                                    go.SetActive(true);
                                    nodePt = go.GetComponent<GizmoMapper>().connectedGizmo; // Mid Point
                                                                                            //break;
                                }
                            }
                        }
                        //Debug.Log(wire.nodes[i].endPointObj.tag);
                        if (wire.nodes[i].endPointPinNum != 0 && (wire.nodes[i].endPointObj.name != "J234" || wire.nodes[i].endPointObj.name != "J833" || wire.nodes[i].endPointObj.name != "J104") || wire.nodes[i].endPointObj.name != "5Q0_906_379") {
                            if (wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount == 1) // only for 1L and 1.5L 
                            {
                                pF.CreateWire(nodePt == null ? startingPt : nodePt, wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(0).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                                endPt = wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(0).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo;

                            }

                            // this block will be used for majority of the connectors
                            if (wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount != 1 && wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount != 0 && wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount == wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).childCount + 1) {

                                //Debug.Log(wire.nodes[i].NodeSTR);
                                if(wire.nodes[i].NodeSTR == null)
                                {
                                    pF.CreateWire(startingPt, wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                                    endPt = wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo;
                                }
                                else
                                {
                                    pF.CreateWire(nodePt == null ? startingPt : nodePt, wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                                    endPt = wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(wire.nodes[i].endPointPinNum - 1).GetComponent<GizmoMapper>().connectedGizmo;
                                }

                            }

                            if (wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount == 0 || wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(2).childCount != wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).childCount + 1) {
                                if (startingPt == null) { Debug.Log("Starting pt is null"); }

                                if (wire.nodes[i].NodeSTR == null)
                                {
                                    pF.CreateWire(startingPt, wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                                    endPt = wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo;
                                }
                                else
                                {
                                    pF.CreateWire(nodePt == null ? startingPt : nodePt, wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                                    endPt = wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo;
                                }

                                  
                            }

                        }
                        else  // for fuses where end point pin is not needed
                        {
                            pF.CreateWire(nodePt == null ? startingPt : nodePt, wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo, 2, mat);
                            endPt = wire.nodes[i].endPointObj.GetComponent<GizmoMapper>().connectedGizmo;
                        }

                        wire.nodes[i].endPointObj.SetActive(true);

                        //Debug.Log( "Wire number:  " + i + 1);
                        if (wire.nodes[i].ExtraComponent != null) // to create wires for extra component in Airbags only
                        {
                            if (wire.nodes[i].ExtraComponent.extraConnectorDesignation != null) {
                                allEndObjs[i + 2] = GameObject.Find(wire.nodes[i].ExtraComponent.extraConnectorDesignation);
                            }

                            if (wire.nodes[i].ExtraComponent.endConnectorDesignation != null) {
                                //Debug.Log("Airbags ExtraTag values 1");
                                allEndObjs[i + 1] = GameObject.Find(wire.nodes[i].ExtraComponent.endConnectorDesignation);

                                startingPt = endPt;
                                str = wire.nodes[i].ExtraComponent.colorCode.Split('/');
                                if (str.Length == 2) {
                                    mat_str = str[0] + "-" + str[1];
                                }
                                else {
                                    mat_str = str[0];
                                }

                                foreach (Material m in tempMatArray) {
                                    if (m.name.ToLower() == mat_str.ToLower()) {
                                        //Debug.Log("FOUND");
                                        mat = m;
                                    }
                                }

                                TempEndComp = GameObject.Find(wire.nodes[i].ExtraComponent.endConnectorDesignation);
                                endTag = TempEndComp.transform.Find("Tag").gameObject;
                                endTag.SetActive(true);
                                if (endTag.transform.childCount > 1) // assign end tag values
                                {
                                    //Debug.Log("Airbags ExtraTag values 2");
                                    if (endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == "-"
                                     && endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text == "-") {

                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.crossSection.ToString();
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.colorCode.ToString();
                                        endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[0].details;
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.endConnectorDesignation;
                                        endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.pinNumber.ToString();
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endComponentDesignation;
                                    }
                                    else {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.crossSection.ToString();
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.colorCode.ToString();
                                        endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[0].details;
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.endConnectorDesignation;
                                        endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.pinNumber.ToString();
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].endComponentDesignation;
                                    }
                                    endTag.SetActive(true);
                                }
                                endPt = TempEndComp.GetComponent<GizmoMapper>().connectedGizmo;

                                pF.CreateWire(startingPt, TempEndComp.GetComponent<GizmoMapper>().connectedGizmo, 2, mat);

                                string tempNodeSTR;
                                GameObject[] tempNodeGO = FindObjectOfType<AllNodes>().AllNodess;

                                startingPt = endPt;

                                if (wire.nodes[i].ExtraComponent.node != null) // creating wires for the connectors having extra component
                                {
                                    tempNodeSTR = "N-" + wire.nodes[i].ExtraComponent.node;

                                    foreach (GameObject g in tempNodeGO) {
                                        if (g.name == tempNodeSTR) {
                                            g.SetActive(true);
                                            endPt = g.GetComponent<GizmoMapper>().connectedGizmo;
                                        }
                                    }

                                    pF.CreateWire(startingPt, endPt, 2, mat);
                                }

                                // to create wires for extra component in Airbags only
                                if (wire.nodes[i].ExtraComponent.extraConnectorDesignation != null) {
                                    TempEndComp = GameObject.Find(wire.nodes[i].ExtraComponent.extraConnectorDesignation);

                                    startingPt = endPt;

                                    pF.CreateWire(startingPt, TempEndComp.GetComponent<GizmoMapper>().connectedGizmo, 2, mat);

                                    temp2 = TempEndComp.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
                                    temp2.SetActive(true);

                                    temp2 = TempEndComp.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
                                    temp2.SetActive(true);

                                    temp2 = TempEndComp.transform.Find("Tag").gameObject;
                                    temp2.SetActive(true);

                                    temp12 = TempEndComp.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(wire.nodes[i].ExtraComponent.extraPinNumber - 1)).gameObject;

                                    FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().Highlight(temp12);
                                }
                            }
                        }
                        // to create labels near the pins for huge connectors where not all front labels are present
                        if ((conn.name == "T73a" || conn.name == "T40a" || conn.name == "T46b" || conn.name == "T90a" || conn.name == "T73c" || conn.name == "T38a" || conn.name == "T38xa" || conn.name == "T81a" || conn.name == "T38d") && wire.wireNumber > 1) {
                            if (conn.name != "T38a" && conn.name != "T38d" && conn.name != "T40a") {
                                Debug.Log(conn.name);
                                XOffset = -0.0022f; // assigning an offsent so the labels is a litte outside the pin where it is visible
                                ZOffset = 0.0006f;
                            }
                            else if (conn.name == "T38d") {
                                XOffset = 0.0015f;
                                ZOffset = 0.002f;
                            }
                            else if(conn.name == "T40a")
                            {
                                XOffset = 0.0f;
                                YOffset = 0.002f;
                                ZOffset = -0.0006f;
                            }
                            else {
                                XOffset = 0.0022f;
                                ZOffset = 0.0006f;
                            }


                            Vector3 newPos = new Vector3(wire.pin.transform.position.x + XOffset, wire.pin.transform.position.y + YOffset, wire.pin.transform.position.z + ZOffset); // assigning new correct position
                            TempText = Instantiate(LC.TMPrefab, newPos, conn.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).rotation, conn.transform.GetChild(2).GetChild(0).GetChild(1)); // Instantiating
                            TempText.GetComponent<TextMeshPro>().text = wire.wireNumber.ToString(); // assigning the pin number to the label
                            if (conn.name == "T73a" && TempText.GetComponent<TextMeshPro>().text == "72")
                            {
                                TempText.transform.localScale = new Vector3(0.000005403734f, 0.000005403734f, 0.000005403734f); // assigning the scale
                            }
                            else
                            {
                                TempText.transform.localScale = conn.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).localScale; // assigning the scale
                            }

                            TempText.tag = "Label"; // tagging the text
                            TempText.name = "AAB";
                        }

                        //Debug.Log(wire.nodes[i].ExtraComponent.extraConnectorDesignation);
                        // to create labels near the pins for huge connectors where not all back labels are present
                        if (wire.nodes[i].endPointObj != null && (wire.nodes[i].endPointObj.name == "5Q0_906_379" || wire.nodes[i].endPointObj.name == "T38c" ||
                            wire.nodes[i].endPointObj.name == "T28c" || wire.nodes[i].endPointObj.name == "T26e" ||
                            wire.nodes[i].endPointObj.name == "T28d" || wire.nodes[i].endPointObj.name == "T28xc" ||
                            wire.nodes[i].endPointObj.name == "T28xd" || wire.nodes[i].endPointObj.name == "T38a" ||
                            wire.nodes[i].endPointObj.name == "T38b" || wire.nodes[i].endPointObj.name == "T38d" ||
                            wire.nodes[i].endPointObj.name == "T38xa" || wire.nodes[i].endPointObj.name == "T38xb" ||
                            wire.nodes[i].endPointObj.name == "T73a" || wire.nodes[i].endPointObj.name == "T73c" ||
                            wire.nodes[i].endPointObj.name == "T81a" || wire.nodes[i].endPointObj.name == "T90a" || (wire.nodes[i].ExtraComponent != null && wire.nodes[i].ExtraComponent.extraConnectorDesignation == "T90a") ||
                            wire.nodes[i].endPointObj.name == "T94a" || wire.nodes[i].endPointObj.name == "T20e" ||
                            wire.nodes[i].endPointObj.name == "T16h" || wire.nodes[i].endPointObj.name == "T40a")) {
                            if (wire.nodes[i].endPointObj.transform.root.name == "1.0LEngine(Updated)") // offsets are for pin numbers which are to be displayed for big connectors. Basically we copy the pin transform values and use the same for text and add an offset to move the text a little outside and left/right, so that it don't overlap with the pin
                            {
                                XOffset = -0.0002f;
                                ZOffset = -0.002f;
                                //Debug.Log("Set for 1L");
                            }
                            else if (wire.nodes[i].endPointObj.transform.root.name == "1.5LEngine (Updated)") {
                                XOffset = -0.0022f;
                                ZOffset = 0.0006f;
                                //Debug.Log("Set for 1.5L");
                            }
                            else // different offsets for different connectors
                            {
                                if (wire.nodes[i].endPointObj.name == "T38d") {
                                    XOffset = 0.0004f;
                                    //YOffset = 0.002f;
                                    ZOffset = 0.002f;
                                }
                                else if (wire.nodes[i].endPointObj.name == "T38xb" || wire.nodes[i].endPointObj.name == "T40a" || wire.nodes[i].endPointObj.name == "T26e") {
                                    Debug.Log("T40a here");
                                    XOffset = -0.0022f;
                                    //YOffset = -0.002f;
                                    ZOffset = -0.0009f;
                                }
                                else if (wire.nodes[i].endPointObj.name == "T28xc") {
                                    XOffset = -0.0022f;
                                    //YOffset = -0.002f;
                                    ZOffset = -0.0011f;
                                }
                                else if (wire.nodes[i].endPointObj.name == "T81a") {
                                    XOffset = -0.0007f;
                                    //YOffset = -0.002f;
                                    ZOffset = -0.0017f;
                                }
                                else {
                                    XOffset = -0.0022f;
                                    ZOffset = 0.0006f;
                                }
                            }
                            // instantiating the labels accordingly

                            if (conn.connectorDesignation != "T3ad" && conn.connectorDesignation != "T3ae" && conn.connectorDesignation != "T2dj" && conn.connectorDesignation != "T2dk" && wire.nodes[i].endPointPinNum != 1 || (wire.nodes[i].endPointObj.name == "T81a" && wire.nodes[i].endPointPinNum != 1)) {
                                Vector3 newPos = new Vector3(wire.nodes[i].endPointPin.transform.position.x + XOffset, wire.nodes[i].endPointPin.transform.position.y + YOffset, wire.nodes[i].endPointPin.transform.position.z + ZOffset);
                                TempText = Instantiate(LC.TMPrefab, newPos, wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).rotation, wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(1));
                                TempText.GetComponent<TextMeshPro>().text = wire.nodes[i].endPointPinNum.ToString();
                            }
                            else if (wire.nodes[i].ExtraComponent != null && wire.nodes[i].ExtraComponent.extraPinNumber != 1 && conn.connectorDesignation != "T73a" && wire.nodes[i].ExtraComponent.extraConnectorDesignation == "T90a") {
                                TempEndComp = GameObject.Find(wire.nodes[i].ExtraComponent.extraConnectorDesignation);

                                endTag = TempEndComp.transform.Find("Tag").gameObject;
                                if (endTag.transform.childCount > 1) // assign end tag values
                                {
                                    Debug.Log("Airbag Extra part " + wire.nodes[i].ExtraComponent.extraConnectorDesignation);
                                    if (endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text == "-"
                                     && endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text == "-") {

                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].nodeCrossSection.ToString();
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = wire.colorCode.ToString();
                                        endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[0].details;
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.extraConnectorDesignation;
                                        endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.extraPinNumber.ToString();
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.extraComponentDesignation;
                                    }
                                    else {
                                        endTag.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].nodeCrossSection.ToString();
                                        endTag.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.colorCode.ToString();
                                        endTag.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[0].details;
                                        endTag.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.extraConnectorDesignation;
                                        endTag.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text += " and " + wire.nodes[i].ExtraComponent.extraPinNumber.ToString();
                                        endTag.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = wire.nodes[i].ExtraComponent.extraComponentDesignation;

                                    }
                                    endTag.SetActive(true);
                                }
                                endPin = TempEndComp.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(wire.nodes[i].ExtraComponent.extraPinNumber - 1).gameObject;


                                Vector3 newPos = new Vector3(endPin.transform.position.x + XOffset, endPin.transform.position.y, endPin.transform.position.z + ZOffset);
                                TempText = Instantiate(LC.TMPrefab, newPos, TempEndComp.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).rotation, TempEndComp.transform.GetChild(2).GetChild(0).GetChild(1));
                                TempText.GetComponent<TextMeshPro>().text = wire.nodes[i].ExtraComponent.extraPinNumber.ToString();
                            }

                            for (int n = 0; n < wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).childCount; n++) {
                                if (wire.nodes[i].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(n).name == wire.nodes[i].endPointPin.name) {
                                    count = n + 1;
                                    //Debug.Log(wire.nodes[i].endPointPinNum.ToString());
                                }
                            }

                            if (TempText != null) {
                                TempText.tag = "Label";

                                // some scales are not consistent, have hard coded for some of them
                                if (wire.nodes[i].endPointObj.transform.root.name == "1.5LEngine (Updated)" || wire.nodes[i].endPointObj.transform.root.name == "1.0LEngine(Updated)" ||
                                    centralCsv.Csv_name == "Elec Adj Ext Mirror" ||
                                    centralCsv.Csv_name == "ABS" ||
                                    (centralCsv.Csv_name == "Radio System(8UK,8RU)" && wire.nodes[i].endPointObj.name != "T28xd" && wire.nodes[i].endPointObj.name != "T28xc") ||
                                    (centralCsv.Csv_name == "Climatic (9AB)" && wire.nodes[i].endPointObj.name == "T73c") ||
                                    (centralCsv.Csv_name == "DQ200(0EG)" && wire.nodes[i].endPointObj.name == "T73c") ||
                                    (centralCsv.Csv_name == "Background lighting (QQ1)" && conn.name != "T2gb") ||
                                    wire.nodes[i].endPointObj.name == "T38xb" || (wire.nodes[i].endPointObj.name == "T38xa" && centralCsv.Csv_name != "Halogen Headlights(8ID)" && centralCsv.Csv_name != "LED Headlights(8IY)") ||
                                    wire.nodes[i].endPointObj.name == "T81a" ||
                                    wire.nodes[i].endPointObj.name == "T73c"
                                    )
                                {
                                    Debug.Log("enteringg " + TempText.GetComponent<TextMeshPro>().text);
                                    TempText.transform.localScale = new Vector3(0.0005771127f, 0.0005771127f, 0.0005771127f);
                                }
                                else {
                                    TempText.transform.localScale = new Vector3(0.000008f, 0.000008f, 0.000008f);
                                }
                            }
                        }
                    }
                    else {
                        // for starting connectors whose end point obj is null but still have to show the labels
                        Debug.Log("End Pt Obj is null");
                        if (conn.name == "T73a" || conn.name == "T90a" || conn.name == "T73c" || conn.name == "T38a" || conn.name == "T38xa" || conn.name == "T38d") {
                            if (conn.name != "T38a") {
                                XOffset = -0.0022f;
                                ZOffset = 0.0006f;
                            }
                            else {
                                XOffset = 0.0022f;
                                ZOffset = 0.0006f;
                            }

                            Vector3 newPos = new Vector3(wire.pin.transform.position.x + XOffset, wire.pin.transform.position.y, wire.pin.transform.position.z + ZOffset);
                            TempText = Instantiate(LC.TMPrefab, newPos, conn.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).rotation, conn.transform.GetChild(2).GetChild(0).GetChild(1));
                            TempText.GetComponent<TextMeshPro>().text = wire.wireNumber.ToString();
                            TempText.transform.localScale = conn.transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).localScale;
                            TempText.tag = "Label";
                        }
                    }
                }
            }
            else {
                Debug.Log(component + " Gizmo mapper is null");
            }

            if (wire.nodes[i].endPointObj != null && wire.nodes.Length > 0 && wire.nodes[i].endPointObj.tag != "Node") {
                //Debug.Log(wire.nodes[i].endPointObj);
                temp1 = wire.nodes[i].endPointObj.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels").gameObject;
                temp2 = wire.nodes[i].endPointObj.transform.GetChild(2).transform.GetChild(0).transform.Find("Labels (1)").gameObject;
            }

            if (temp1 != null) {
                //Debug.Log("inside 1");
                temp1.SetActive(true);
                if (temp1.transform.childCount > 0 && temp1.transform.GetChild(0).GetComponent<Canvas>()) // for 1L and 1.5L
                {
                    temp1.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                }
            }
            if (temp2 != null) {
                //Debug.Log("inside 2");
                temp2.SetActive(true);
                if (temp2.transform.childCount > 0 && temp2.transform.GetChild(0).GetComponent<Canvas>()) // for 1L and 1.5L
                {
                    temp2.transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                }
            }

            if (component.GetComponent<GizmoMapper>() == null) {
                wire.nodes[i].model.SetActive(true);
            }

            if (conn.transform.Find("Fuse Prefab")) // for highlighting starting fuse cubes
            {
                TempFuse = conn.transform.Find("Fuse Prefab").gameObject;
                FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().Highlight(conn.transform.Find("Fuse Prefab").gameObject);


                if (conn.name.StartsWith("SC")) {
                    XOffset = -0.004f;
                    ZOffset = -0.006f;
                }
                else if (conn.name.StartsWith("SB")) {
                    XOffset = 0.005f;
                    ZOffset = 0.009f;
                }

                Vector3 newPos = new Vector3(TempFuse.transform.position.x + XOffset, TempFuse.transform.position.y, TempFuse.transform.position.z + ZOffset);

                TempText = Instantiate(LC.TMPrefab, newPos, conn.transform.rotation, conn.transform);
                if (conn.name.StartsWith("SC")) {
                    TempText.transform.rotation = Quaternion.Euler(conn.transform.rotation.x, conn.transform.rotation.y + 100, conn.transform.rotation.z);
                }
                else if (conn.name.StartsWith("SB")) {
                    TempText.transform.rotation = Quaternion.Euler(conn.transform.rotation.x + 90, conn.transform.rotation.y - 90, conn.transform.rotation.z);
                }
                //Debug.Log(TempText.transform.rotation);
                TempText.GetComponent<TextMeshPro>().text = conn.name;
                TempText.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                TempText.tag = "Label";
                TempText.name = "FuseLabel";
            }

            if (wire.nodes[i].endPointObj != null && wire.nodes[i].endPointObj.transform.Find("Fuse Prefab")) // for highlighting end fuse cubes
            {
                TempFuse = wire.nodes[i].endPointObj.transform.Find("Fuse Prefab").gameObject;

                FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().Highlight(TempFuse);

                XOffset = 0.0022f;
                ZOffset = 0.0006f;


                Vector3 newPos = new Vector3(TempFuse.transform.position.x + XOffset, TempFuse.transform.position.y, TempFuse.transform.position.z + ZOffset);
                TempText = Instantiate(LC.TMPrefab, newPos, TempFuse.transform.rotation, TempFuse.transform);
                TempText.GetComponent<TextMeshPro>().text = wire.nodes[i].endPointObj.name;
                TempText.transform.localScale = new Vector3(0.0005771127f, 0.0005771127f, 0.0005771127f);
                TempText.tag = "Label";
                TempText.name = "FuseLabel";
            }

            FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().Highlight(wire.pin); // pin highlight
            if (wire.nodes[i].endPointPin != null) {
                FindObjectOfType<LineDetector>().GetComponent<PartHighlighter>().Highlight(wire.nodes[i].endPointPin);
            }
        }

        if (scene.name == "Central Wiring Harness (Arpit's final)" || scene.name == "Taigun Central Harness") {
            for (int i = 0; i < centralCsv.AllConnectors.transform.childCount; i++) // assign all connector mats to black in All connectors
            {
                for (int j = 0; j < centralCsv.ConnDesig.Count; j++) {
                    if (centralCsv.AllConnectors.transform.GetChild(i).name == centralCsv.ConnDesig[j].name && centralCsv.AllConnectors.transform.GetChild(i).name != conn.name) {
                        //Debug.Log(ConnDesig[j].name + " " + AllConnectors.transform.GetChild(i).name);
                        rend = centralCsv.ConnDesig[j].GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in rend) {
                            if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                                r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)" || r.material.name == "Font Material (Instance)" || r.material.name == "Node_Mat (Instance)") {
                                //Debug.Log("Entering here 1 " + r.material.name);
                                continue;
                            }
                            //r.material = mappedMat;
                        }

                    }
                }
            }

            for (int i = 0; i < centralCsv.Manual_connectors.Length; i++) // assign all connector mats to black in manual
            {
                for (int j = 0; j < centralCsv.ConnDesig.Count; j++) {
                    if (centralCsv.Manual_connectors[i].name == centralCsv.ConnDesig[j].name && centralCsv.Manual_connectors[i].name != conn.name) {
                        //Debug.Log(ConnDesig[j].name + " " + AllConnectors.transform.GetChild(i).name);
                        rend = centralCsv.ConnDesig[j].GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in rend) {
                            if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                                r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)") {
                                //Debug.Log("Pass");
                                continue;
                            }
                            r.material = mappedMat;
                        }
                    }
                }
            }

            for (int i = 0; i < centralCsv.Auto_connectors.Length; i++) // assign all connector mats to black in auto
            {
                for (int j = 0; j < centralCsv.ConnDesig.Count; j++) {
                    if (centralCsv.Auto_connectors[i].name == centralCsv.ConnDesig[j].name && centralCsv.Auto_connectors[i].name != conn.name) {
                        rend = centralCsv.ConnDesig[j].GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in rend) {
                            if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                                r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)") {
                                //Debug.Log("Pass");
                                continue;
                            }
                            r.material = mappedMat;
                        }

                    }
                }
            }
        }

        FindObjectOfType<AllConnectors>().ChangeMat(allEndObjs); // change materials for end connectors to blue
    }
}
