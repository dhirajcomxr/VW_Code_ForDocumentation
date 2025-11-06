using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapping : MonoBehaviour
{
    string[] lines;
    string[] splitData, splitData1;
    //int NumOfLinesPerGroup = 0;
    private GameObject temp;
    private Connector conn;
    private int wiresLen;

    void Start() 
    {
        TextAsset data = Resources.Load<TextAsset>("NewCSV");
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

            if(lines[i].Trim() == ",,,,,,,,,,,,,,,,,,,,,,,,")
            {
                //Debug.Log(i);

                splitData = lines[i - j + 1].Split(','); //for match

                temp = GameObject.Find(splitData[4]);
                conn = temp.GetComponent<Connector>();

                if(conn != null)
                {
                    for(int l = 0; l<j; l++) // get the last pin number to create the required Wire class objects beforehand
                    {
                        splitData = lines[i-l-1].Split(',');
                        if(splitData[6] != "")
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

                /*for(int l = 0; l<j-1; l++) //for nodes creation within each wire
                {
                    splitData = lines[i - l - 1].Split(',');
                    splitData1 = lines[i - l - 2].Split(',');

                    tempInt1 = System.Convert.ToInt32(splitData[7]);

                    //Debug.Log(i-l-1 + " " + splitData[7]);

                    //Debug.Log(i - l - 1);

                    if(splitData[11] != "")
                    {
                        if(splitData1[11] != "")
                        {
                            if(splitData[7] == splitData1[7])
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
                        
                    }
                }*/
                
                tempInt = 0; //for wires
                tempInt1 = 0; //for nodes
                temp = null; conn = null;

                for(int l = 1; l<j; l++) //go through line by line in a group and assign all values
                {
                    splitData = lines[i - j + l].Split(',');
                    tempInt = System.Convert.ToInt32(splitData[6]);
                    tempInt1 = System.Convert.ToInt32(splitData[6]);

                    //Debug.Log(splitData.Length);
                    //Debug.Log(i-j+l);
                    
                    temp = GameObject.Find(splitData[4]); //link the conn with the new group's particular connector's name from the hierarchy
                    conn = temp.GetComponent<Connector>();

                    //conn.component = GameObject.Find(splitData[0]);
                    //conn.connectorDesignation = splitData[3];
                    //conn.componentDesignation = splitData[1];

                    conn.wires[tempInt-1].wireNumber = tempInt;
                    conn.wires[tempInt-1].colorCode = splitData[5];
                    conn.wires[tempInt-1].crossSection = System.Convert.ToDouble(splitData[6]);

                    int n = 0;

                    foreach (Transform obj in temp.transform)  //assigning all pin/plane gameobjects
                    {

                        if(obj.childCount != 0)
                        {
                            foreach(Transform child in obj)
                            {
                                if(n < conn.wires.Length)
                                {
                                    conn.wires[n].pin = child.gameObject;
                                    n++;
                                }
                            }
                        }
                    }
                    //Debug.Log(splitData[10]);
                    //conn.wires[tempInt-1].nodes[tempInt1-1].endPointPinNum = System.Convert.ToInt32(splitData[10]);

                }
                j = 0;
                

            }
            

        }
        
    }
/*
                        splitData[0] is compoment (Type: GameObject)
                        splitData[1] is componentDesignation (Type: String)
                        splitData[2] is connectorPartNumber (Type: )
                        splitData[3] is connectorDesignation (Type: String)
                        splitData[4] is wireNumber (Type: int)
                        splitData[5] is colorCode (Type: String)
                        splitData[6] is crossSection (Type: float)
                        splitData[7] is Node 
                        splitData[8] is 
                        splitData[9] is 
                        splitData[10] is 
                        splitData[11] is 
                        splitData[12] is 
                        splitData[13] is 
                    */
}
