using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SearchBar : MonoBehaviour
{
    [SerializeField]
    private InputField inputText;

    public List<GameObject> SelectableConns;
    public List<string> SelectableConnsName;
    public string CFDname;

    private GameObject target;
    private Connector targetConn;

    //public NewCSVMapping15L _1L;
    public CentralHarnessCSVMapping centralHarness;
    public SlaviaCSVMapping SlaviacentralHarness;

    private string Connectorname;
    private string ConnectorDesign;
    private string ComponentDesign;
    private LineDetectorV2 LD;

    private void Start()
    {
        LD = FindObjectOfType<LineDetectorV2>();
            //SelectableConns = SlaviacentralHarness.ConnDesig;
            //SelectableConnsName = SlaviacentralHarness.ConnName;
    }

    // Update is called once per frame
    void Update()
    {        
        foreach(GameObject g in SelectableConns)
        {
            //Debug.Log(g.name);
            Connectorname = g.GetComponent<Connector>().connectorName;
            ConnectorDesign = g.GetComponent<Connector>().connectorDesignation;
            ComponentDesign = g.GetComponent<Connector>().componentDesignation;

            //Debug.Log(inputText.text);

            if ((Connectorname.ToLower() == inputText.text.ToLower()
                || ConnectorDesign.ToLower() == inputText.text.ToLower()
                || ComponentDesign.ToLower() == inputText.text.ToLower())
                && inputText.text.ToLower() != "")
            {
                //Debug.Log(inputText.text.ToLower());
                LD.SelectConnector(g);
                Debug.Log("--- Selected: " + g.name);

                //Debug.Log("Selected");
                inputText.text = "";
                this.transform.GetComponent<SearchBar>().enabled = false;
            }
        }

    }
}
