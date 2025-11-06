using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IndieMarc.CurvedLine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SlaviaConnectedWire : MonoBehaviour {
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
    GlobalActiveWiresV2 GlobalWires;
    PathFinderV2 pF;
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
    AllConnectorsSlavia AllC;
    LineDetectorV2 LD;
    LabelChanger LC;
    GameObject TempText;
    GameObject TempFuse;
    int count = 0;
    float XOffset = 0;
    float YOffset = 0;
    float ZOffset = 0;
    GameObject endTag;
    GameObject endPin;
    SlaviaCSVMapping centralCsv;
    Scene scene;
    GameObject startingPt, nodePt, endPt;

    private void Start() {

        //centralCsv = FindObjectOfType<SlaviaCSVMapping>();
        //LD = FindObjectOfType<LineDetector>();
        //LC = FindObjectOfType<LabelChanger>();
        //scene = SceneManager.GetActiveScene();
        ////Debug.Log(scene.name);
        //if (scene.name != "Central Wiring Harness (Arpit's final)" && scene.name != "Slavia Central Wiring Harness") // only for 1L and 1.5L scene
        //{
        //    temp12 = GameObject.Find("5Q0_906_379");

        //    count = temp12.transform.GetChild(2).GetChild(0).GetChild(2).childCount;

        //    for (int i = 0; i < count - 1; i++) {
        //        temp12.transform.GetChild(2).GetChild(0).GetChild(2).GetChild(i + 1).gameObject.SetActive(false);
        //    }
        //}


        //count = 0;

        //GlobalWires = FindObjectOfType<GlobalActiveWires>();
        //AllC = FindObjectOfType<AllConnectorsSlavia>();
        ////ConnMat = AllC.ConnectorMat;
        ////mappedMat = AllC.MappedConnectorMat;
    }
}

