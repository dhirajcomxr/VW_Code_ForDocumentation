using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCSVMapping1L : MonoBehaviour
{
    public List<string> ConnName1;
    public List<string> ConnDesig1;
    public List<GameObject> ConnGO1;
    string[] lines;
    string[] splitData, splitData1, designatedPinAssign;
    private GameObject temp;
    private Connector conn;
    private int wiresLen, NodeCount = 1;

    void Start()
    {
        TextAsset data = Resources.Load<TextAsset>("1LCSV");
        lines = data.text.Split('\n');
        StartMapping();
    }
    
    public void StartMapping()
    {
        int tempInt = 0; //for nodes
        int tempInt1 = 0; //for wires

        for(int i = 1, j = 1; i < lines.Length; i++,j++)
        {
            //Debug.Log(lines[i]);

            if(lines[i].Trim() == ",,,,,,,,,,,,,,,,,,,,,,,,,,")
            {
                //Debug.Log(i);

                splitData = lines[i - j + 1].Split(','); //for match
                //ConnName1.Add(splitData[1]);
                //ConnDesig1.Add(splitData[5]);


                temp = GameObject.Find(splitData[4]);
                //ConnGO1.Add(temp);

                if(temp == null) { Debug.Log(splitData[4]); }
                
                conn = temp.GetComponent<Connector>();
                if (conn == null)
                {
                    Debug.Log("Null" + " " + temp);
                }



                conn.component = temp;
                conn.componentDesignation = splitData[2];
                conn.connectorDesignation = splitData[5];
                conn.connectorName = splitData[1];


                if(conn != null)
                {
                    for(int l = 0; l<j; l++) // get the last pin number to create the required Wire class objects beforehand
                    {
                        splitData = lines[i-l-1].Split(',');
                        if(splitData[8] != "")
                        {
                            tempInt = System.Convert.ToInt32(splitData[6]);
                            conn.wires = new Wire[tempInt];

                            for(int p = 0; p < tempInt; p++)
                            {
                                conn.wires[p] = new Wire();
                                //conn.wires[p].wireNumber = 1;
                            }
                            break;
                        }
                    }

                }
                else
                {
                    Debug.Log(temp + " does not have a connector script");
                }

                tempInt = 0;

                for(int l = 0; l<j-1; l++) //for nodes creation within each wire
                {
                    splitData = lines[i - l - 1].Split(',');
                    splitData1 = lines[i - l - 2].Split(',');

                    //Debug.Log(splitData[5] + " " + temp);

                    //Debug.Log(lines[i - l - 1]);

                    tempInt1 = System.Convert.ToInt32(splitData[6]);

                    //Debug.Log(i-l-1 + " " + splitData[7]);

                    //Debug.Log(i - l - 1);

                    if(splitData[11] != "")
                    {
                        
                        if(splitData1[11] != "")
                        {
                            
                            if(splitData[6] == splitData1[6])
                            {
                                NodeCount++;
                                //continue;
                            }
                            else
                            {
                                if(conn.wires[tempInt1 - 1].nodes == null)
                                {
                                    //Debug.Log(NodeCount);
                                    conn.wires[tempInt1-1].nodes = new Node[NodeCount];

                                    for (int p = 0; p < NodeCount; p++)
                                    {
                                        conn.wires[tempInt1-1].nodes[p] = new Node();
                                        //conn.wires[tempInt1-1].nodes[p].endPointPinNum = 1;
                                    }

                                    NodeCount = 1;
                                }
                            }


                            
                        }
                        else
                        {
                            if(conn.wires[tempInt1 - 1].nodes == null)
                                {
                                    //Debug.Log(NodeCount);
                                    conn.wires[tempInt1-1].nodes = new Node[NodeCount];

                                    for (int p = 0; p < NodeCount; p++)
                                    {
                                        conn.wires[tempInt1-1].nodes[p] = new Node();
                                        //conn.wires[tempInt1-1].nodes[p].endPointPinNum = 
                                    }

                                    NodeCount = 1;
                                }
                        }
                    }
                    else
                    {
                        //Debug.Log(splitData1[11]);
                        if(splitData1[11] == "" && splitData1[1] != "" && splitData[1] != "FUEL METERING VALVE" && splitData[1] != "Fuel pressure regulating valve" && splitData[1] != "Module For Charging Pressure Control" 
                            && splitData[1] != "Knock Sensor 1" || splitData[1] == "Ignition Coil 1" || splitData[1] == "Ignition Coil 2" || splitData[1] == "Ignition Coil 3" || splitData[1] == "Engine Speed Sensor Crankshaft" 
                            || splitData[1] == "Oil Pressure Sensor" || splitData[1] == "Position Sensor For Exhaust Camshaft" || splitData[1] == "Position Sensor For Inlet Camshaft" || splitData[1] == "Charge air cooling pump"
                            || splitData[1] == "Injection Valve For Cylinder 1" || splitData[1] == "Injection Valve For Cylinder 2" || splitData[1] == "Injection Valve For Cylinder 3"
                            || splitData[1] == "Exhaust camshaft control valve" || splitData[1] == "Fuel Pressure Sensor (High Pressure)" || splitData[1] == "Throttle Valve Control Unit")
                        {
                          //Debug.Log(lines[i - l - 2]);
                            //Debug.Log(splitData1[1]);
                            //if(conn.connectorName == "CHARGE AIR PRESSURE & TEMP SENSOR") { Debug.Log(lines[i - l - 1]); Debug.Log(lines[i - l - 2]); }
                            conn.wires[tempInt1-1].nodes = new Node[2];
                            conn.wires[tempInt1-1].nodes[0] = new Node();
                            conn.wires[tempInt1-1].nodes[1] = new Node();
                            //Debug.Log("Done 2" + " " + i);
                            
                        }
                        else
                        {
                            conn.wires[tempInt1 - 1].nodes = new Node[1];
                            conn.wires[tempInt1 - 1].nodes[0] = new Node();
                        }

                        if(conn.wires[tempInt1 - 1].nodes == null && splitData1[11] != "")
                        {   //Debug.Log("Done 1" + " " + i);
                            conn.wires[tempInt1-1].nodes = new Node[1];
                            conn.wires[tempInt1-1].nodes[0] = new Node();
                        }

                    }
                }

                tempInt = 0;
                
                for(int l = 1; l<j; l++) //go through line by line in a group and assign all wire values
                {
                    splitData = lines[i - j + l].Split(',');
                    tempInt = System.Convert.ToInt32(splitData[6]);

                    conn.wires[tempInt-1].wireNumber = tempInt;
                    conn.wires[tempInt-1].colorCode = splitData[9];
                    //Debug.Log("Done");
                    conn.wires[tempInt-1].crossSection = System.Convert.ToDouble(splitData[10]);

                    int m = 0;

                    //Debug.Log(temp);
                    Transform obj = temp.transform.GetChild(2).transform.GetChild(0).transform.GetChild(0);

                    if(obj.childCount != 0)
                    {
                        foreach(Transform child in obj)
                        {  
                            if (m < conn.wires.Length)
                            {
                                //conn.wires[m].pin = child.transform.GetChild(0).transform.GetChild(m).gameObject;
                                conn.wires[m].pin = child.gameObject;
                                m++;
                            }
                        }
                    }


                    
                }

                int LC = 0; //LineCount
                //Debug.Log(i);
                for(int w = 0; w < conn.wires.Length; w++)
                {
                    if(conn.wires[w] != null && conn.wires[w].nodes != null)
                    {
                         for (int n = 0; n < conn.wires[w].nodes.Length; n++)
                        {
                            splitData = lines[i - j + LC + 1].Split(',');
                            if (splitData[23] != "")
                            {
                                conn.wires[w].nodes[n].details = splitData[23];
                            }
                            else
                            {
                                conn.wires[w].nodes[n].details = "---";
                            }
                            
                            
                                

                            if (splitData[12] != "" || splitData[13] != "" || splitData[15] != "" || splitData[17] != "")
                            {
                                conn.wires[w].nodes[n].endComponentDesignation = splitData[17];
                                conn.wires[w].nodes[n].endPointDesignation = splitData[15];

                                if (splitData[17] != "")
                                {
                                    designatedPinAssign = splitData[17].Split('/');
                                    //Debug.Log(designatedPinAssign[1]);
                                    conn.wires[w].nodes[n].endPointPinNum = System.Convert.ToInt32(designatedPinAssign[1]);
                                }

                                conn.wires[w].nodes[n].nodeCrossSection = System.Convert.ToDouble(splitData[12]);
                                conn.wires[w].nodes[n].nodeColorCode = splitData[13];
                                if(splitData[11] != "")
                                {
                                   // Debug.Log(splitData[10]);
                                    conn.wires[w].nodes[n].NodeSTR = "N-" + splitData[11];
                                }
                                
                                //Debug.Log(splitData[16]);
                                conn.wires[w].nodes[n].endPointObj = GameObject.Find(splitData[18]);
                                if(splitData[16] != "")
                                {
                                    //Debug.Log(splitData[16]);
                                    conn.wires[w].nodes[n].endPointPin = conn.wires[w].nodes[n].endPointObj.transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform.GetChild(System.Convert.ToInt32(splitData[16])-1).gameObject;

                                }

                                LC++;
                                }

                                
                            
                            
                        }
                    }
                }

                j = 0;
            }


        }

    }

}
