using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowInfoV2 : MonoBehaviour {

    public int pin;
    public List<int> rows;
    public GameObject content;
    private CentralHarnessMapper centralHarnessMapper;
    private LineDetectorV2 LD;


    void Start() {
        if (content == null) {

            content = transform.parent.gameObject;
            GetComponent<Toggle>().group = content.GetComponent<ToggleGroup>();
        }
            
    }
    public void LoadRows() {
        centralHarnessMapper = FindObjectOfType<CentralHarnessMapper>();
        LD = FindObjectOfType<LineDetectorV2>();
        if (LD.pinMat != null) {
            LD.RemoveHighlightPin();
        }
        centralHarnessMapper.ResetTags();
        if(LD.connector.gameObject.transform.GetChild(2).transform.GetChild(0).transform.Find("Pins").transform.childCount != 0) {
            LD.pinMat = LD.connector.gameObject.transform.GetChild(2).transform.GetChild(0).transform.Find("Pins").transform.GetChild(pin - 1).GetComponent<MeshRenderer>().material;
            LD.HighlightPin();
        }
        LD.DisableNodes(FindObjectOfType<AllNodesV2>().nodes);
        LD.DestroyExistingWires();
        centralHarnessMapper.IdentifyRows(LD.connector.gameObject, pin);
    }
}
