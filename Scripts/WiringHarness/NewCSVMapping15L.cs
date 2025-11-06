using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCSVMapping15L : MonoBehaviour
{
    public List<string> ConnName;
    public List<string> ConnDesig;
    public List<GameObject> ConnGO;
    string[] lines;
    string[] splitData, splitData1, designatedPinAssign;
    private GameObject temp;
    private Connector conn;
    private int wiresLen, NodeCount = 1;
   

    void Start()
    {
       
        TextAsset data = Resources.Load<TextAsset>("1.5LCSV");
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

            if(lines[i].Trim() == ",,,,,,,,,,,,,,,,,,,,,,,,,")
            {
                //Debug.Log(i);

                splitData = lines[i - j + 1].Split(','); //for match
                //ConnName.Add(splitData[1]);
                //ConnDesig.Add(splitData[4]);

                

                temp = GameObject.Find(splitData[3]);
                //ConnGO.Add(temp);

               

              

                if(temp == null) { Debug.Log(splitData[3]); }
                
                conn = temp.GetComponent<Connector>();
                if (conn == null)
                {
                    Debug.Log("Null" + " " + temp);
                }



                conn.component = temp;
                conn.componentDesignation = splitData[2];
                conn.connectorDesignation = splitData[4];
                conn.connectorName = splitData[1];


                if(conn != null)
                {
                    for(int l = 0; l<j; l++) // get the last pin number to create the required Wire class objects beforehand
                    {
                        splitData = lines[i-l-1].Split(',');
                        if(splitData[7] != "")
                        {
                            tempInt = System.Convert.ToInt32(splitData[5]);
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

                    tempInt1 = System.Convert.ToInt32(splitData[5]);

                    //Debug.Log(i-l-1 + " " + splitData[7]);

                    //Debug.Log(i - l - 1);

                    if(splitData[10] != "")
                    {
                        if(splitData1[10] != "")
                        {
                            if(splitData[5] == splitData1[5])
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
                        if(splitData1[10] == "" && splitData1[1] != "" && splitData[1] != "FUEL METERING VALVE" && splitData[1] != "MODULE FOR CHARGING PRESSURE REGULATION" && splitData[1] != "MODULE FOR MOTOR TEMPERATURE CONTROL" && splitData[1] != "Throttle Valve Control Unit" && splitData[1] != "Injection Valve For Cylinder 3" && splitData[1] != "Injection Valve For Cylinder 2" && splitData[1] != "Injection Valve For Cylinder 1" && splitData[1] != "Injection Valve For Cylinder 4")
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

                        if(conn.wires[tempInt1 - 1].nodes == null && splitData1[10] != "")
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
                    tempInt = System.Convert.ToInt32(splitData[5]);

                    conn.wires[tempInt-1].wireNumber = tempInt;
                    conn.wires[tempInt-1].colorCode = splitData[8];
                    //Debug.Log("Done");
                    conn.wires[tempInt-1].crossSection = System.Convert.ToDouble(splitData[9]);

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
                        for(int n = 0; n < conn.wires[w].nodes.Length; n++)
                        {

                            splitData = lines[i - j + LC + 1].Split(',');
                            if (splitData[22] != "")
                            {
                                conn.wires[w].nodes[n].details = splitData[22];
                            }
                            else
                            {
                                conn.wires[w].nodes[n].details = "---";
                            }
                            
                                

                            if (splitData[11] != "" || splitData[12] != "" || splitData[14] != "" || splitData[16] != "")
                            {
                                conn.wires[w].nodes[n].endComponentDesignation = splitData[16];
                                conn.wires[w].nodes[n].endPointDesignation = splitData[14];

                                if (splitData[15] != "" && splitData[15] != "LUG")
                                {
                                    //designatedPinAssign = splitData[16].Split('/');
                                    //Debug.Log(splitData[15]);
                                    //if(splitData[1] == "Coolant Temperature Sender") { Debug.Log(splitData[15]); }
                                    conn.wires[w].nodes[n].endPointPinNum = System.Convert.ToInt32(splitData[15]);
                                }

                                conn.wires[w].nodes[n].nodeCrossSection = System.Convert.ToDouble(splitData[11]);
                                conn.wires[w].nodes[n].nodeColorCode = splitData[12];
                                if(splitData[10] != "")
                                {
                                   // Debug.Log(splitData[10]);
                                    conn.wires[w].nodes[n].NodeSTR = "N-" + splitData[10];
                                }
                                
                                //Debug.Log(splitData[16]);
                                conn.wires[w].nodes[n].endPointObj = GameObject.Find(splitData[17]);
                                //conn.wires[w].nodes[n].endPointPin = System.Convert.ToInt32(splitData[15]);
                                if (splitData[15] != ""  && splitData[17] != "036_911_137" && splitData[17] != "036_911_137 1")
                                {
                                    //Debug.Log(splitData[15]);
                                    conn.wires[w].nodes[n].endPointPin = conn.wires[w].nodes[n].endPointObj.transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform.GetChild(System.Convert.ToInt32(splitData[15]) - 1).gameObject;

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
