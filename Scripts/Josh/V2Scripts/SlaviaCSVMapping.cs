using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlaviaCSVMapping : MonoBehaviour {
    public bool Node2;
    public bool Node3;
    public bool Node4;

    public EndConnectorAndPin[] AllEndConnAndPins;

    public GameObject ManualObject;
    public GameObject AutoObject;
    public GameObject ManualNodes;
    public GameObject AutoNodes;

    private string[] headerArray;

    public List<string> ConnName;
    public List<GameObject> ConnDesig;
    public List<GameObject> ConnGO;
    public GameObject[] Auto_connectors;
    public GameObject[] Manual_connectors;
    string[] lines;
    public string[] splitData, splitData1, designatedPinAssign, detailSplitData;
    private GameObject temp;
    private Connector conn;
    private int wiresLen, NodeCount = 1;
    private int tempCount = 0, m = 1;
    public string Csv_name;
    public int ExtendedWireColumn = 0;
    public int DetailCount = 0;
    public GameObject AllConnectors;
    public Material CentralHarnessConns;
    public Material MappedConnMat;
    Renderer[] rend;
    BoxCollider boxCollider;
    string currentVariant;

    string tempString;

    bool toBeIncluded = false;

    public enum variant { active, ambition, style };

    void Start() {
        //PlayerPrefs.DeleteKey("variant");
        //PlayerPrefs.SetString("variant", "style");
        Csv_name = PlayerPrefs.GetString("cfd");
        TextAsset data = Resources.Load<TextAsset>(Csv_name);
        lines = data.text.Split('\n');

        AllEndConnAndPins = new EndConnectorAndPin[lines.Length + 10];
        for (int j = 0; j < AllEndConnAndPins.Length; j++) {
            AllEndConnAndPins[j] = new EndConnectorAndPin();
        }

        if (Csv_name == "Climatronic") // to enable and disable auto and manual parts accordingly
        {
            AutoNodes.SetActive(true);
            AutoObject.SetActive(true);

            ManualNodes.SetActive(false);
            ManualObject.SetActive(false);
        }

        if (Csv_name == "Climatic (9AB)") // to enable and disable auto and manual parts accordingly
        {
            AutoNodes.SetActive(false);
            AutoObject.SetActive(false);

            ManualNodes.SetActive(true);
            ManualObject.SetActive(true);
        }

        headerArray = lines[0].Split(',');

        if (Csv_name == "Slavia Airbag") { ExtendedWireColumn = 16; } // depreciated

        if (headerArray[11].ToLower() == "node") { Node2 = true; } // to check if the csv have extra node (2) 
        if (headerArray[14].ToLower() == "node") { Node3 = true; } // to check if the csv have extra node (3) 

        StartMapping();
    }

    public void StartMapping() {
        int tempInt = 0; //for nodes
        int tempInt1 = 0; //for wires

        // check the CSV line by line
        for (int i = 1, j = 1; i < lines.Length; i++, j++) // go through line by line in the csv
        {
            //m = i;
            // identify the blocks
            if (lines[i].Trim() == ",,,,,,,,,,,,,,,,,,,,") // if found an empty line with exact number of commas in the csv, consider that block and start mappimng
            {
                // get the first line of that block
                int currentBlockStartLine = i - j + 1;
                splitData = lines[currentBlockStartLine].Split(','); //for match

                // get the current variant from PlayerPrefs
                currentVariant = PlayerPrefs.GetString("variant");

                // checking for the variant starts here
                // this is only for the Dropdown

                int startPoint = i - j + 1; // this is the starting point of the block above the current blank row
                int endPoint = i; // the current blank row

                Debug.Log(currentVariant + " " + lines[1] + " " + splitData[splitData.Length - 1]);

                for (int row = 0; row < (endPoint - startPoint); row++) {
                    if (currentVariant == variant.active.ToString() && splitData[splitData.Length - 1].ToLower().StartsWith("yes")) { toBeIncluded = true; } //if any one of the row in that block has 'yes' in it, include that connector in the list
                    if (currentVariant == variant.ambition.ToString() && splitData[splitData.Length - 2].ToLower().StartsWith("yes")) { toBeIncluded = true; }
                    if (currentVariant == variant.style.ToString() && splitData[splitData.Length - 3].ToLower().StartsWith("yes")) { toBeIncluded = true; }
                }

                //Debug.Log(splitData[4] + " " + toBeIncluded);
                if (toBeIncluded) // if that connector has to be included, then only proceed
                {
                    // load data depending on the variant
                    if (splitData[4] != "" && GameObject.Find(splitData[4]) != null) // add that connector in the lists
                    {
                        ConnDesig.Add(GameObject.Find(splitData[4]));
                        ConnName.Add(splitData[1]);
                    } else if (splitData[4] == "" || splitData[4] == "Lug") {
                        if (splitData[2] != "" && GameObject.Find(splitData[2])) {
                            ConnDesig.Add(GameObject.Find(splitData[2]));
                            ConnName.Add(splitData[1]);
                        }
                    }
                }

                if (splitData[4] != "" && GameObject.Find(splitData[4]) != null) // assign the connector gameobject to 'temp' to use it later
                {
                    temp = GameObject.Find(splitData[4]);
                } else {
                    temp = GameObject.Find(splitData[2]);
                }

                if (temp == null) {
                    j = 0;
                    continue;
                }

                conn = temp.GetComponent<Connector>();

                if (conn == null) {
                    Debug.Log("Null" + " " + temp);
                }

                conn.component = temp;
                conn.componentDesignation = splitData[2];
                conn.connectorDesignation = splitData[4];
                conn.connectorName = splitData[1];

                if (conn != null) {
                    int max = 0;
                    // get the highest pin number to create the required Wire class objects beforehand
                    for (int l = 0; l < j; l++) {
                        splitData = lines[i - l - 1].Split(',');
                        if (splitData[5] != "" && splitData[5].Length < 4) {
                            tempInt = System.Convert.ToInt32(splitData[5]);
                            if (max < tempInt) { max = tempInt; }
                        }
                    }

                    if (conn.name == "T5c") { max = 5; }
                    if (conn.name == "T20e") { max = 20; }
                    if (conn.name == "T4w") { max = 4; }
                    if (conn.name == "T3ar") { max = 3; }
                    if (conn.name == "T2ds") { max = 2; }
                    if (conn.name == "T6ac") { max = 6; }
                    if (conn.name == "T6y") { max = 6; }
                    if (conn.name == "T73c") { max = 73; }
                    if (conn.name == "T6z") { max = 6; }
                    if (conn.name == "T16j") { max = 16; }
                    if (conn.name == "T2du") { max = 2; }
                    if (conn.name == "T2dv") { max = 2; }
                    if (conn.name == "T10e") { max = 10; }
                    if (conn.name == "T26e") { max = 26; }
                    if (conn.name == "T10z") { max = 10; }
                    if (conn.name == "T10s") { max = 10; }
                    if (conn.name == "T38xa") { max = 38; }
                    if (conn.name == "T10h") { max = 10; }
                    if (conn.name == "T10x") { max = 10; }
                    if (conn.name == "T10y") { max = 10; }
                    if (conn.name == "T4av") { max = 4; }
                    if (conn.name == "T4aw") { max = 4; }
                    if (conn.name == "T2df") { max = 2; }
                    if (conn.name == "T38xb") { max = 38; }
                    if (conn.name == "T4v") { max = 4; }
                    if (conn.name == "T40a") { max = 40; }
                    if (conn.name == "T2m") { max = 2; }
                    if (conn.name == "T25a") { max = 25; }
                    if (conn.name == "T10j") { max = 10; }
                    if (conn.name == "T81a") { max = 81; }
                    if (conn.name == "T4bz") { max = 4; }
                    if (conn.name == "T38d") { max = 38; }
                    if (conn.name == "T28xd") { max = 28; }
                    if (conn.name == "T28xc") { max = 28; }
                    if (conn.name == "T10t") { max = 10; }
                    if (conn.name == "T4c") { max = 4; }
                    if (conn.name == "T16b") { max = 16; }
                    if (conn.name == "T73a") { max = 73; }
                    if (conn.name == "T3c") { max = 3; }
                    if (conn.name == "T10a") { max = 10; }
                    if (conn.name == "T6a") { max = 6; }
                    if (conn.name == "T6d") { max = 6; }
                    if (conn.name == "T3ak") { max = 3; }
                    if (conn.name == "T18a") { max = 18; }
                    if (conn.name == "T4ao") { max = 4; }
                    if (conn.name == "T2z") { max = 2; }

                    conn.wires = new Wire[max];
                    for (int p = 0; p < max; p++) {
                        conn.wires[p] = new Wire();
                    }
                } else {
                    Debug.Log(temp + " does not have a connector script");
                }
                tempInt = 0;

                //for nodes creation within each wire
                for (int l = 0; l < j - 1; l++) {
                    splitData = lines[i - l - 1].Split(',');
                    splitData1 = lines[i - l - 2].Split(',');
                    if (splitData[5] != "") {
                        tempInt1 = System.Convert.ToInt32(splitData[5]);
                    } else {
                        continue;
                    }

                    if (splitData[10] != "")  // checking 2 rows at once and figuring out whether it is continous, sometimes this piece of code gives incorrect number of nodes if the rows are not sorted in the csv in ascending order, use hard coded code below in that case
                    {
                        if (splitData1[10] != "") {
                            if (splitData[5] == splitData1[5]) {
                                NodeCount++;
                                //continue;
                            } else {
                                if (conn.wires[tempInt1 - 1].nodes == null) {
                                    conn.wires[tempInt1 - 1].nodes = new Node[NodeCount];
                                    for (int p = 0; p < NodeCount; p++) {
                                        conn.wires[tempInt1 - 1].nodes[p] = new Node();
                                    }
                                    NodeCount = 1;
                                }
                            }
                        } else {
                            if (conn.wires[tempInt1 - 1].nodes == null) {
                                conn.wires[tempInt1 - 1].nodes = new Node[NodeCount];
                                for (int p = 0; p < NodeCount; p++) {
                                    conn.wires[tempInt1 - 1].nodes[p] = new Node();
                                }
                                NodeCount = 1;
                            }
                        }
                    } else // some hard coded values to get correct number of ndoes
                      {
                        if ((Csv_name == "DQ200(0EG)" && (splitData[2] == "SB15" || splitData[2] == "SC53")) || (Csv_name == "Slavia Mobile Telephone Systems(EK2)" && splitData[2] == "N-369") || (Csv_name == "Slavia Climatronic KC0" && splitData[2] == "N-B317") || (Csv_name == "Automatic GB 09G (PR - 0ER)" && (splitData[2] == "SB15" || splitData[2] == "SC53")) || splitData1[10] == "" && splitData1[1] != "" && splitData1[1] != "Sliding Sunroof Motor" && splitData1[1] != "Radiator Fan Control Unit" && splitData[2] != "J794" && splitData[2] != "J126" && splitData[2] != "EX21" && splitData[2] != "J255" && (splitData[2] != "J519" && Csv_name == "Centre Console Switches (4D0)") && (splitData[2] != "J104" && Csv_name == "Centre Console Switches (4D0)") || splitData[2] == "H" || (Csv_name == "Slavia Ent&Start Auth (4I3)" && splitData[5] == "38")) {
                            //Debug.Log(splitData[2]);
                            conn.wires[tempInt1 - 1].nodes = new Node[2];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                            conn.wires[tempInt1 - 1].nodes[1] = new Node();
                        } else if (Csv_name == "Automatic GB 09G (PR - 0ER)" && splitData[5] == "10" && splitData[4] == "T10r" || (Csv_name == "Basic Equipment" && splitData[2] == "SC58")) {
                            conn.wires[tempInt1 - 1].nodes = new Node[4];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                            conn.wires[tempInt1 - 1].nodes[1] = new Node();
                            conn.wires[tempInt1 - 1].nodes[2] = new Node();
                            conn.wires[tempInt1 - 1].nodes[3] = new Node();
                        } else if (Csv_name == "Basic Equipment" && splitData[5] == "57") {
                            conn.wires[tempInt1 - 1].nodes = new Node[5];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                            conn.wires[tempInt1 - 1].nodes[1] = new Node();
                            conn.wires[tempInt1 - 1].nodes[2] = new Node();
                            conn.wires[tempInt1 - 1].nodes[3] = new Node();
                            conn.wires[tempInt1 - 1].nodes[4] = new Node();
                        } else if (Csv_name == "Basic Equipment" && splitData[5] == "8" && splitData[4] == "T10xc") {
                            conn.wires[tempInt1 - 1].nodes = new Node[2];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                            conn.wires[tempInt1 - 1].nodes[1] = new Node();
                        } else {
                            conn.wires[tempInt1 - 1].nodes = new Node[1];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                        }
                        if (conn.wires[tempInt1 - 1].nodes == null && splitData1[10] != "") {
                            conn.wires[tempInt1 - 1].nodes = new Node[1];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                        }
                    }
                }

                tempInt = 0;

                //go through line by line in a group and assign all wire values
                for (int l = 1; l < j; l++) {
                    splitData = lines[i - j + l].Split(',');
                    if (splitData[5] != "") {
                        tempInt = System.Convert.ToInt32(splitData[5]);
                    } else {
                        continue;
                    }

                    conn.wires[tempInt - 1].wireNumber = tempInt;
                    if (ExtendedWireColumn != 0 && splitData[ExtendedWireColumn] != "")  // only for airbag cfd which has extra values
                    {
                        conn.wires[tempInt - 1].nodes[0].ExtraComponent = new ExtendedWireComponent();
                        conn.wires[tempInt - 1].nodes[0].ExtraComponent.crossSection = System.Convert.ToDouble(splitData[19]);
                        conn.wires[tempInt - 1].nodes[0].ExtraComponent.colorCode = splitData[18];
                        if (splitData[20] != "") {
                            conn.wires[tempInt - 1].nodes[0].ExtraComponent.node = splitData[20];
                        }
                        conn.wires[tempInt - 1].nodes[0].ExtraComponent.endConnectorDesignation = splitData[16];
                        conn.wires[tempInt - 1].nodes[0].ExtraComponent.pinNumber = System.Convert.ToInt32(splitData[17]);
                        if (splitData[20] == "") {
                            conn.wires[tempInt - 1].nodes[0].ExtraComponent.extraConnectorDesignation = splitData[21];
                            conn.wires[tempInt - 1].nodes[0].ExtraComponent.extraComponentDesignation = splitData[23];
                            conn.wires[tempInt - 1].nodes[0].ExtraComponent.extraPinNumber = System.Convert.ToInt32(splitData[22]);
                        }
                    }

                    if (splitData[6] == "")  // assigning color code and cross section values
                    {
                        conn.wires[tempInt - 1].colorCode = splitData[10];
                    } else {
                        conn.wires[tempInt - 1].colorCode = splitData[6];
                    }

                    if (splitData[9] != "" || splitData[7] != "") {
                        if (splitData[7] == "") {
                            if (Csv_name == "Basic Equipment") {
                                conn.wires[tempInt - 1].crossSection = System.Convert.ToDouble(splitData[10]);

                            } else {
                                conn.wires[tempInt - 1].crossSection = System.Convert.ToDouble(splitData[9]);
                            }
                        } else {
                            conn.wires[tempInt - 1].crossSection = System.Convert.ToDouble(splitData[7]);
                        }


                    }


                    int m = 0;
                    Transform obj = temp.transform.GetChild(2).GetChild(0).GetChild(0);
                    if (obj.childCount != 0) // assigning correct pin to the connector script
                    {
                        foreach (Transform child in obj) {
                            if (m < conn.wires.Length) {
                                conn.wires[m].pin = child.gameObject;
                                m++;
                            }
                        }
                    }

                }
                int LC = 0; //LineCount (depreciated)

                detailSplitData = lines[0].Split(',');
                for (int e = 0; e < detailSplitData.Length; e++) {
                    if (detailSplitData[e] == "Details (+/-)") {
                        DetailCount = e;
                    }
                }

                for (int w = 0; w < conn.wires.Length; w++) // assigning node values inside the Wires
                {
                    //Debug.Log(conn + " " + tempInt + " " + conn.wires.Length);

                    if (tempInt != 0 && conn.wires[tempInt - 1] != null && conn.wires[tempInt - 1].nodes != null) {
                        for (int n = 0; n < conn.wires[tempInt - 1].nodes.Length; n++) {
                            splitData = lines[i - j + LC + 1].Split(',');

                            if (splitData[5] != "") {
                                tempInt = System.Convert.ToInt32(splitData[5]);
                            } else {
                                continue;
                            }

                            toBeIncluded = false;
                            startPoint = i - j + 1; // this is the starting point of the block above the current blank row
                            endPoint = i; // the current blank row

                            if (currentVariant == variant.active.ToString() && splitData[splitData.Length - 1].ToLower().StartsWith("yes")) { toBeIncluded = true; }
                            if (currentVariant == variant.ambition.ToString() && splitData[splitData.Length - 2].ToLower().StartsWith("yes")) { toBeIncluded = true; }
                            if (currentVariant == variant.style.ToString() && splitData[splitData.Length - 3].ToLower().StartsWith("yes")) { toBeIncluded = true; }

                            if (toBeIncluded) {
                                conn.wires[tempInt - 1].nodes[n].toInclude = true;
                            }

                            if (conn.wires[tempInt - 1].nodes[n].details == "") // depreciated
                            {
                                conn.wires[tempInt - 1].nodes[n].details = splitData[DetailCount];
                            }

                            // some hard coding for details as the detail column is not same in all cfds

                            if (Csv_name != "Slavia Ent&Start Auth (4I3)" && Csv_name != "Slavia Power Steering" && Csv_name != "Slavia LED Headlights(8IY)" && Csv_name != "Slavia Rear Parking Aid(PDC,7X3)" && Csv_name != "Multifunction Steering Wheel" && Csv_name != "Centre Console Switches (4D0)" && Csv_name != "Centre Console Switches (4D3)" && Csv_name != "Slavia Elec Adj Ext Mirror" && Csv_name != "Slavia Radio System(8UK,8RU)" &&
                                Csv_name != "Radio System (8UW)" && Csv_name != "Radio System(8UK,8RL)" && Csv_name != "Slavia Power Steering" &&
                                Csv_name != "Automatic GB 09G (PR - 0ER)" && Csv_name != "Slavia Wireless Data Transfer(EL1)" && Csv_name != "DQ200(0EG)" && Csv_name != "Climatic (9AB)" && Csv_name != "Slavia Background lighting (QQ1)" && splitData[25] != "" && Csv_name != "Tail LGT LED(8SA)" && Csv_name != "Tail LGT LED(8VG)" && Csv_name != "Halogen Headlights(8ID)") {
                                conn.wires[tempInt - 1].nodes[n].details = splitData[25];
                            } else if ( Csv_name == "Halogen Headlights(8ID)" || Csv_name == "Slavia Ent&Start Auth (4I3)" || Csv_name == "Slavia Power Steering" || Csv_name == "Slavia LED Headlights(8IY)" || Csv_name == "Rear Parking Aid(PDC, 7X3)" || Csv_name == "Slavia Multifunction Steering Wheel" || Csv_name == "Centre Console Switches (4D3)" || Csv_name == "Slavia Radio System(8UK,8RU)" || Csv_name == "Radio System(8UK,8RL)" || Csv_name == "Radio System (8UW)" || Csv_name == "Tail LGT LED(8VG)" || Csv_name == "Tail LGT LED(8SA)" || Csv_name == "Centre Console Switches (4D0)" || Csv_name == "Slavia Elec Adj Ext Mirror" || Csv_name == "Auto anti-dazzle Mirror" && splitData[28] != "" && Csv_name != "Slavia Background lighting (QQ1)" && Csv_name != "Climatic (9AB)" && Csv_name != "DQ200(0EG)" && Csv_name != "Slavia Wireless Data Transfer(EL1)" && Csv_name != "Automatic GB 09G (PR - 0ER)") {
                                conn.wires[tempInt - 1].nodes[n].details = splitData[28];
                            } else if (Csv_name != "Automatic GB 09G (PR - 0ER)" && Csv_name == "Slavia Background lighting (QQ1)" && Csv_name != "Climatic (9AB)" && Csv_name != "DQ200(0EG)" && Csv_name != "Slavia Wireless Data Transfer(EL1)") {
                                conn.wires[tempInt - 1].nodes[n].details = splitData[32];
                            } else if (Csv_name == "Climatic (9AB)" || Csv_name == "DQ200(0EG)" || Csv_name == "Automatic GB 09G (PR - 0ER)") {
                                {
                                    if (splitData[34] != "") {
                                        conn.wires[tempInt - 1].nodes[n].details = splitData[34];
                                    }
                                }
                            }
                            if (Csv_name == "Basic Equipment" || Csv_name == "Reversing Camera (KA1)" || Csv_name == "Slavia Seat Belt Warning System" || Csv_name == "Seat Ventilation (4D3)" || Csv_name == "CS Power Window (4R1)" || Csv_name == "CS Power Window (4R4)" || Csv_name == "Slavia Wireless Data Transfer(EL1)" && splitData[31] != "" || Csv_name == "Slavia Rear Parking Aid(PDC,7X3)") {
                                conn.wires[tempInt - 1].nodes[n].details = splitData[31];
                            }
                            if (conn.wires[tempInt - 1].nodes[n].details == null || conn.wires[tempInt - 1].nodes[n].details == "" || conn.wires[tempInt - 1].nodes[n].details == " ") {
                                conn.wires[tempInt - 1].nodes[n].details = "---";
                            }

                            if (splitData[7] != "" || splitData[8] != "" || splitData[12] != "" || splitData[14] != "" || splitData[11] != "" || splitData[17] != "" || splitData[31] != "" || splitData[19] != "" || splitData[34] != "") // start assigning end connector values if the line is not empty
                            {
                                if (Csv_name == "Slavia Wireless Data Transfer(EL1)" || Csv_name == "CS Power Window (4R1)" || Csv_name == "Slavia CS Power Window (4R4)") {
                                    conn.wires[tempInt - 1].nodes[n].endComponentDesignation = splitData[20];
                                } else if (Csv_name == "Slavia Elec Adj Ext Mirror" || Csv_name == "Halogen Headlights(8ID)" || Csv_name == "Slavia LED Headlights(8IY)" || Csv_name == "Tail LGT LED(8SA)" || Csv_name == "Tail LGT LED(8VG)" || Csv_name == "Centre Console Switches (4D0)" || Csv_name == "Slavia Power Steering" || Csv_name == "Slavia Radio System(8UK,8RU)" || Csv_name == "Slavia Radio System(8UK,8RL)") {
                                    conn.wires[tempInt - 1].nodes[n].endComponentDesignation = splitData[17];
                                    
                                }
                                else {
                                    conn.wires[tempInt - 1].nodes[n].endComponentDesignation = splitData[14];
                                }
                                conn.wires[tempInt - 1].nodes[n].endPointDesignation = splitData[11];
                                if (Csv_name != "Basic Equipment" && Csv_name != "Reversing Camera (KA1)" && Csv_name != "Slavia Seat Belt Warning System" && Csv_name != "Seat Ventilation (4D3)" && Csv_name != "CS Power Window (4R1)" && Csv_name != "Slavia CS Power Window (4R4)" && Csv_name != "Slavia Wireless Data Transfer(EL1)" && splitData[1] != "Fuse 7.5" && Csv_name != "Centre Console Switches (4D0)" && Csv_name != "Centre Console Switches (4D3)" && Csv_name != "Slavia Rear Parking Aid(PDC,7X3)" && Csv_name != "Slavia Background lighting (QQ1)" && Csv_name != "Slavia Multifunction Steering Wheel" && Csv_name != "Halogen Headlights(8ID)" && Csv_name != "LED Headlights(8IY)" && Csv_name != "Slavia Ent&Start Auth (4I3)" && Csv_name != "Slavia Elec Adj Ext Mirror" && Csv_name != "Tail LGT LED(8SA)" && Csv_name != "Tail LGT LED(8VG)" && Csv_name != "Slavia Power Steering" && Csv_name != "Climatic (9AB)" && Csv_name != "Radio System (8UW)" && Csv_name != "Slavia Radio System(8UK,8RL)" && Csv_name != "Slavia Radio System(8UK,8RU)" && Csv_name != "CS Power Window (4R1)" && Csv_name != "Slavia CS Power Window (4R4)" && Csv_name != "Basic Equipment" && Csv_name != "Seat Ventilation (4D3)" && Csv_name != "Seat Belt Warning System" && Csv_name != "Reversing Camera (KA1)" && Csv_name != "DQ200(0EG)" && Csv_name != "Automatic GB 09G (PR - 0ER)") {
                                    if (splitData[11] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[11]);
                                    }
                                    if (splitData[12] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[12]);
                                    }
                                } else if (Csv_name != "Basic Equipment" && Csv_name != "Reversing Camera (KA1)" && Csv_name == "Slavia Rear Parking Aid(PDC,7X3)" && Csv_name != "Slavia Seat Belt Warning System" && Csv_name != "Seat Ventilation (4D3)" && Csv_name != "CS Power Window (4R1)" && Csv_name != "Slavia CS Power Window (4R4)" && Csv_name != "Automatic GB 09G (PR - 0ER)" && Csv_name != "DQ200(0EG)" && Csv_name != "Climatic (9AB)" && Csv_name != "Slavia Wireless Data Transfer(EL1)" && (splitData[1] == "Fuse 7.5" || Csv_name == "Centre Console Switches (4D0)" || Csv_name == "Centre Console Switches (4D3)" || Csv_name == "Slavia Multifunction Steering Wheel" || Csv_name == "Rear Parking Aid(PDC, 7X3)" || Csv_name == "Slavia Background lighting (QQ1)" || Csv_name == "Halogen Headlights(8ID)" || Csv_name == "LED Headlights(8IY)" || Csv_name == "Slavia Ent&Start Auth (4I3)" || Csv_name == "Slavia Elec Adj Ext Mirror" || Csv_name == "Tail LGT LED(8SA)" || Csv_name == "Tail LGT LED(8VG)" || Csv_name == "Slavia Power Steering" || Csv_name == "Radio System (8UW)" || Csv_name == "Slavia Radio System(8UK,8RL)" || Csv_name == "Slavia Radio System(8UK,8RU)" || Csv_name == "CS Power Window (4R1)" || Csv_name == "Slavia CS Power Window (4R4)" || Csv_name == "Basic Equipment" || Csv_name == "Seat Ventilation (4D3)" || Csv_name == "Seat Belt Warning System" || Csv_name == "Reversing Camera (KA1)")) {
                                    if (splitData[15] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[15]);
                                    }
                                    if (splitData[14] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[14]);
                                    } 
                                    else if (Csv_name != "Basic Equipment" && Csv_name != "Reversing Camera (KA1)" && Csv_name != "Slavia Seat Belt Warning System" && Csv_name != "Seat Ventilation (4D3)" && Csv_name != "CS Power Window (4R1)" && Csv_name != "Slavia CS Power Window (4R4)" && Csv_name != "Automatic GB 09G (PR - 0ER)" && Csv_name != "DQ200(0EG)" && Csv_name != "Climatic (9AB)" && Csv_name != "Slavia Rear Parking Aid(PDC,7X3)" && Csv_name != "Slavia Wireless Data Transfer(EL1)" && (splitData[1] == "Fuse 7.5" || Csv_name == "Slavia LED Headlights(8IY)")) {
                                        if (splitData[14] != "") {
                                            conn.wires[tempInt - 1].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[14]);
                                        }
                                        if (splitData[13] != "") {
                                            conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[13]);
                                        }
                                    }
                                        
                                    } else if (Csv_name == "Basic Equipment" || Csv_name == "Reversing Camera (KA1)" || Csv_name == "Slavia Seat Belt Warning System" || Csv_name == "Seat Ventilation (4D3)" || Csv_name == "Slavia Wireless Data Transfer(EL1)" || Csv_name == "CS Power Window (4R1)" || Csv_name == "Slavia CS Power Window (4R4)" || Csv_name == "Slavia Rear Parking Aid(PDC,7X3)") {
                                    if (splitData[18] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[18]);
                                    }
                                    if (splitData[17] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[17]);
                                    } else {
                                        if (splitData[20] != "" && GameObject.Find(splitData[20]) && (Csv_name != "Basic Equipment" || splitData[20] == "SB10" || splitData[20] == "811" || splitData[20] == "44" || splitData[20] == "SC30")) {
                                            conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[20]);
                                        }

                                    }
                                } else if (Csv_name == "DQ200(0EG)" || Csv_name == "Climatic (9AB)" || Csv_name == "Automatic GB 09G (PR - 0ER)") {
                                    if (splitData[17] != "" && splitData[17] != "NO INFORMATION") {
                                        conn.wires[tempInt - 1].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[17]);
                                    }
                                    if (splitData[16] != "") {
                                        conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[16]);
                                    } else if (splitData[19] != "" && GameObject.Find(splitData[19])) {
                                        conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[19]);
                                    }
                                }

                                //assigning missed out colorcode and cross sections where they were not in the designated column

                                if (splitData[6] == "") {
                                    conn.wires[tempInt - 1].colorCode = splitData[10];
                                } else {
                                    conn.wires[tempInt - 1].colorCode = splitData[6];
                                }

                                conn.wires[tempInt - 1].nodes[n].nodeColorCode = conn.wires[tempInt - 1].colorCode;

                                if (splitData[9] != "" || splitData[7] != "") {
                                    if (splitData[7] == "") {

                                        conn.wires[tempInt - 1].crossSection = System.Convert.ToDouble(splitData[9]);
                                    } else {
                                        conn.wires[tempInt - 1].crossSection = System.Convert.ToDouble(splitData[7]);
                                    }
                                }
                                conn.wires[tempInt - 1].nodes[n].nodeCrossSection = conn.wires[tempInt - 1].crossSection;


                                if (splitData[8] != "") {
                                    conn.wires[tempInt - 1].nodes[n].NodeSTR = "N-" + splitData[8];
                                }

                                if (Node2) // if node2 is true, it means extra node is present, filling values here
                                {
                                    if (splitData[11] != "") {
                                        conn.wires[tempInt - 1].nodes[n].Node2STR = "N-" + splitData[11];

                                        if (splitData[13] != "") {
                                            conn.wires[tempInt - 1].nodes[n].node2CrossSection = System.Convert.ToDouble(splitData[13]);
                                        }

                                        if (splitData[12] != "") {
                                            conn.wires[tempInt - 1].nodes[n].node2ColorCode = splitData[12];
                                        }
                                    }
                                }

                                if (Node3) // if node3 is true, it means extra node is present, filling values here
                                {
                                    if (splitData[14] != "") {
                                        conn.wires[tempInt - 1].nodes[n].Node3STR = "N-" + splitData[14];

                                        if (splitData[15] != "") {
                                            conn.wires[tempInt - 1].nodes[n].node3CrossSection = System.Convert.ToDouble(splitData[15]);
                                        }

                                        if (splitData[16] != "") {
                                            conn.wires[tempInt - 1].nodes[n].node3ColorCode = splitData[16];
                                        }
                                    }
                                }

                                // some hard coding for the cfds where the data is not in same columns

                                if (Csv_name != "Climatic (9AB)" && splitData[11] != null && GameObject.Find(splitData[11]) && conn.wires[tempInt - 1].nodes[n].endPointObj == null) {
                                    conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[11]);
                                } else if (Csv_name != "Climatic (9AB)" && conn.wires[tempInt - 1].nodes[n].endPointObj == null) {
                                    conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[14]);
                                }

                                if (conn.wires[tempInt - 1].nodes[n].endPointObj == null && Csv_name != "Climatic (9AB)") {
                                    conn.wires[tempInt - 1].nodes[n].endPointObj = GameObject.Find(splitData[17]);
                                }

                                if (conn.wires[tempInt - 1].nodes[n].endPointObj == null && splitData[17] != "") {
                                    GameObject[] tempNode = FindObjectOfType<AllNodesV2>().nodes;

                                    tempString = "N-" + splitData[17];

                                    foreach (GameObject go in tempNode) {
                                        // find and enable that node
                                        if (go.name == tempString) {
                                            conn.wires[tempInt - 1].nodes[n].endPointObj = go;
                                        }
                                    }
                                }
                                if (conn.wires[tempInt - 1].nodes[n].endPointObj != null) {
                                    //if(conn.wires[tempInt - 1].nodes[n].endPointObj.name == "T6y" || conn.wires[tempInt - 1].nodes[n].endPointObj.name == "T6ac" || conn.wires[tempInt - 1].nodes[n].endPointObj.name == "T6z")
                                    {
                                        //Debug.Log(m-1 + " " + i + " EndptObj name: " + conn.wires[tempInt - 1].nodes[n].endPointObj.name + " Pin no. : " + conn.wires[tempInt - 1].nodes[n].endPointPinNum);
                                    }
                                    //Debug.Log(i);
                                    AllEndConnAndPins[m - 1].endConnector = conn.wires[tempInt - 1].nodes[n].endPointObj;
                                    AllEndConnAndPins[m - 1].startConnector = conn.component;
                                    AllEndConnAndPins[m - 1].ReverseWire = conn.wires[tempInt - 1];
                                    AllEndConnAndPins[m - 1].include = toBeIncluded;


                                    if (conn.wires[tempInt - 1].nodes[n].endPointPinNum != 0) {
                                        AllEndConnAndPins[m - 1].endPinNum = conn.wires[tempInt - 1].nodes[n].endPointPinNum;
                                        AllEndConnAndPins[m - 1].startPinNum = conn.wires[tempInt - 1].wireNumber;
                                    }
                                    if (AllEndConnAndPins[m - 1].endConnector != null) {
                                        m++;
                                    }


                                }

                                //if(m <= i)
                                {
                                    // if (AllEndConnAndPins[m-1].endConnector != null) { m++; }
                                }
                                // assigning endptPin according to the cfd as they are in different columns

                                if (conn.wires[tempInt - 1].nodes[n].endPointObj != null && conn.wires[tempInt - 1].nodes[n].endPointObj.name != "811") {
                                    if (splitData[18] != ""
                                        && (Csv_name == "Slavia Wireless Data Transfer(EL1)"
                                        || Csv_name == "CS Power Window (4R1)"
                                        || Csv_name == "Slavia CS Power Window (4R4)"
                                        || Csv_name == "Seat Ventilation (4D3)"
                                        || Csv_name == "Slavia Seat Belt Warning System"
                                        || Csv_name == "Reversing Camera (KA1)"
                                        || Csv_name == "Basic Equipment")) {
                                        conn.wires[tempInt - 1].nodes[n].endPointPin = conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(splitData[18]) - 1).gameObject;
                                    } else if (Csv_name != "Automatic GB 09G (PR - 0ER)"
                                          && Csv_name != "DQ200(0EG)"
                                          && Csv_name != "Climatic (9AB)"
                                          && splitData[12] != ""
                                          && splitData[11] != ""
                                          && splitData[11] != "43"
                                          && conn.wires[tempInt - 1].wireNumber != 0
                                          && conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).childCount > 0
                                          && (Csv_name == "Airbag"
                                          || Csv_name == "ABS"
                                          || Csv_name == "Panoramic Sliding Sunroof"
                                          || Csv_name == "Slavia Radiator Fan"
                                          || Csv_name == "Slavia Mobile Telephone Systems(EK2)"
                                          || Csv_name == "Slavia Climatronic KC0"
                                          || Csv_name == "Slavia ABS"
                                          || Csv_name == "Slavia Airbag"
                                          || Csv_name == "Slavia Panaromic Sliding Sunroof(3FE)")) {
                                        conn.wires[tempInt - 1].nodes[n].endPointPin = conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(splitData[12]) - 1).gameObject;
                                    } else if (Csv_name != "Automatic GB 09G (PR - 0ER)"
                                          && Csv_name != "DQ200(0EG)"
                                          && Csv_name != "Climatic (9AB)"
                                          && splitData[15] != ""
                                          && conn.wires[tempInt - 1].wireNumber != 0
                                          && conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).childCount > 0
                                          || (Csv_name == "Rear Parking Aid(PDC, 7X3)" && splitData[17] != "43")
                                          || (Csv_name == "Slavia Background lighting (QQ1)" && splitData[17] != "43")
                                          || (Csv_name == "Slavia Elec Adj Ext Mirror" && splitData[17] != "43")
                                          || (Csv_name == "Slavia Auto anti-dazzle Mirrir(4L6)" && splitData[17] != "43")
                                          || (Csv_name == "Halogen Headlights(8ID)" && splitData[17] != "SC16"
                                              && splitData[17] != "SC35" && splitData[17] != "SC13"
                                              && splitData[17] != "SC40" && splitData[17] != "43"
                                              && splitData[17] != "44")
                                          || (Csv_name == "LED Headlights(8IY)" && splitData[17] != "SC16"
                                              && splitData[17] != "SC35" && splitData[17] != "SC13"
                                              && splitData[17] != "SC40" && splitData[17] != "43"
                                              && splitData[17] != "44")
                                          || (Csv_name == "Tail LGT LED(8SA)" && splitData[17] != "SC16"
                                              && splitData[17] != "SC35" && splitData[17] != "SB9"
                                              && splitData[17] != "SC40" && splitData[17] != "SB")
                                          || (Csv_name == "Tail LGT LED(8VG)" && splitData[17] != "SC16"
                                              && splitData[17] != "SC35" && splitData[17] != "SB9"
                                              && splitData[17] != "SC40" && splitData[17] != "SB")
                                          || (Csv_name == "Slavia Power Steering" && splitData[17] != "811")
                                          || (Csv_name == "Slavia Ent&Start Auth (4I3)" && splitData[17] != "811"
                                              && splitData[17] != "43" && splitData[17] != "44")
                                          || Csv_name == "Radio System (8UW)"
                                          || Csv_name == "Radio System(8UK,8RL)"
                                          || Csv_name == "Slavia Radio System(8UK,8RU)"
                                          || Csv_name == "Slavia Multifunction Steering Wheel") {
                                        if (conn.wires[tempInt - 1].nodes[n].endPointObj.tag != "Node") {
                                            conn.wires[tempInt - 1].nodes[n].endPointPin = conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(splitData[15]) - 1).gameObject;
                                        }

                                    } else if (Csv_name == "Slavia LED Headlights(8IY)" && splitData[17] != "SC16"
                                              && splitData[17] != "SC35" && splitData[17] != "SC13"
                                              && splitData[17] != "SC40" && splitData[17] != "43"
                                              && splitData[17] != "44") {
                                        if (conn.wires[tempInt - 1].nodes[n].endPointObj.tag != "Node") {
                                            conn.wires[tempInt - 1].nodes[n].endPointPin = conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(splitData[14]) - 1).gameObject;
                                        }
                                        }
                                    else if (((splitData[19] != "J519" || splitData[16] == "T73c") && (splitData[19] != "J217" || splitData[16] == "T81a" || splitData[16] == "T10j") && (splitData[19] != "SC" || splitData[16] == "T2a") && (splitData[19] != "J623" || splitData[16] == "T94a") && splitData[19] != "SC31" && splitData[19] != "SC46" && splitData[19] != "SB12" && splitData[19] != "SB15") && (Csv_name == "Automatic GB 09G (PR - 0ER)" || Csv_name == "DQ200(0EG)" || Csv_name == "Climatic (9AB)") && conn.wires[tempInt - 1].nodes[n].endPointObj.tag != "Node") {
                                        if (conn.wires[tempInt - 1].nodes[n].endPointObj.tag != "Node") {
                                            conn.wires[tempInt - 1].nodes[n].endPointPin = conn.wires[tempInt - 1].nodes[n].endPointObj.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(System.Convert.ToInt32(splitData[17]) - 1).gameObject;
                                        }
                                    }
                                }
                                LC++;
                            }

                        }
                    }
                }
                j = 0;
            }
        }

        for (int v = 0; v < AllEndConnAndPins.Length; v++) {
            for (int w = 0; w < ConnDesig.Count; w++) {


                if (AllEndConnAndPins[v].endConnector != null && AllEndConnAndPins[v].endConnector.name == ConnDesig[w].name) {
                    Debug.Log("Outside " + ConnDesig[w].name + " " + AllEndConnAndPins[v].endPinNum);

                    if (!ConnDesig[w].name.StartsWith("SB") && !ConnDesig[w].name.StartsWith("SC") && ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].wireNumber == 0) {
                        Debug.Log("Inside" + ConnDesig[w].name + " " + AllEndConnAndPins[v].endPinNum);
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].colorCode = AllEndConnAndPins[v].ReverseWire.colorCode;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].wireNumber = AllEndConnAndPins[v].endPinNum;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].crossSection = AllEndConnAndPins[v].ReverseWire.crossSection;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes = new Node[1];
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0] = new Node();
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].toInclude = AllEndConnAndPins[v].include;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].NodeSTR = AllEndConnAndPins[v].ReverseWire.nodes[0].NodeSTR;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].nodeCrossSection = AllEndConnAndPins[v].ReverseWire.nodes[0].nodeCrossSection;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].nodeColorCode = AllEndConnAndPins[v].ReverseWire.nodes[0].nodeColorCode;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].endComponentDesignation = AllEndConnAndPins[v].ReverseWire.nodes[0].endComponentDesignation;
                        //Debug.Log(AllEndConnAndPins[v].startPinNum);
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].endPointPinNum = AllEndConnAndPins[v].startPinNum;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].details = AllEndConnAndPins[v].ReverseWire.nodes[0].details;
                        ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].endPointObj = AllEndConnAndPins[v].startConnector;
                        //Debug.Log(AllEndConnAndPins[v].startConnector + " " + AllEndConnAndPins[v].startPinNum);
                        if (!AllEndConnAndPins[v].startConnector.name.StartsWith("SC") && !AllEndConnAndPins[v].startConnector.name.StartsWith("SB") && !AllEndConnAndPins[v].startConnector.name.StartsWith("SA")) {
                            ConnDesig[w].GetComponent<Connector>().wires[AllEndConnAndPins[v].endPinNum - 1].nodes[0].endPointPin = AllEndConnAndPins[v].startConnector.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(AllEndConnAndPins[v].startPinNum - 1).gameObject;
                        }
                    }

                }
            }
        }

        ResetConnMat();

    }


    public void ResetConnMat() {

        for (int i = 0; i < AllConnectors.transform.childCount; i++) // change all connector materials to blue in All connectors
        {
            for (int j = 0; j < ConnDesig.Count; j++) {
                if (AllConnectors.transform.GetChild(i).name == ConnDesig[j].name) {
                    rend = ConnDesig[j].GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in rend) {
                        if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                            r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)" || r.material.name == "Font Material" || r.material.name == "Node_Mat") {
                            //Debug.Log("Pass");
                            continue;
                        }
                        r.material = CentralHarnessConns;
                    }
                    boxCollider = AllConnectors.transform.GetChild(i).GetComponent<BoxCollider>();
                    boxCollider.enabled = true;
                }
            }
        }

        for (int i = 0; i < Manual_connectors.Length; i++) // change all connector materials to blue in manual
        {
            for (int j = 0; j < ConnDesig.Count; j++) {
                if (Manual_connectors[i].name == ConnDesig[j].name) {
                    //Debug.Log(ConnDesig[j].name + " " + AllConnectors.transform.GetChild(i).name);
                    rend = ConnDesig[j].GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in rend) {
                        if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                            r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)" || r.material.name == "Font Material" || r.material.name == "Node_Mat") {
                            //Debug.Log("Pass");
                            continue;
                        }
                        r.material = CentralHarnessConns;
                    }
                    boxCollider = Manual_connectors[i].GetComponent<BoxCollider>();
                    boxCollider.enabled = true;
                }
            }
        }

        for (int i = 0; i < Auto_connectors.Length; i++) // change all connector materials to blue in Auto
        {
            for (int j = 0; j < ConnDesig.Count; j++) {
                if (Auto_connectors[i].name == ConnDesig[j].name) {
                    //Debug.Log(ConnDesig[j].name + " " + AllConnectors.transform.GetChild(i).name);
                    rend = ConnDesig[j].GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in rend) {
                        if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                            r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)" || r.material.name == "Font Material" || r.material.name == "Node_Mat") {
                            //Debug.Log("Pass");
                            continue;
                        }

                        for (int m = 0; m < r.materials.Length; m++) {
                            r.materials[m] = CentralHarnessConns;
                        }
                    }
                    boxCollider = Auto_connectors[i].GetComponent<BoxCollider>();
                    boxCollider.enabled = true;
                }
            }
        }
    }
}
