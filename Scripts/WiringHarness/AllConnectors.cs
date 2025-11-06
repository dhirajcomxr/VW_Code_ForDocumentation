using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AllConnectors : MonoBehaviour
{
    public GameObject[] allConnectors;

    public GameObject[] currentEnPtObjs;
   
    public Material Faded;
    public Material ConnectorMat;
    public Material MappedConnectorMat;
    Renderer[] children;
    Material tempMat;
    public CentralHarnessCSVMapping CentralCSV;
    Renderer[] rend;

   
    public void ChangeTOBlack() // change all connector materials to black
    {
        for (int m = 0; m < allConnectors.Length; m++)
        {
            children = allConnectors[m].GetComponents<Renderer>();

            for (int n = 0; n < children.Length; n++)
            {
                children[n].material = MappedConnectorMat;
            }
        }
        
    }

    public void StartConnMat(Connector startConn) // get start connector and assign it's material to blue
    {
        for (int i = 0; i < allConnectors.Length; i++)
        {
            for (int j = 0; j < CentralCSV.ConnDesig.Count; j++)
            {
                if (allConnectors[i].transform.parent.name == CentralCSV.ConnDesig[j].name && allConnectors[i].transform.parent.name != startConn.name)
                {
                    rend = CentralCSV.ConnDesig[j].GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in rend)
                    {
                        if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                            r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)")
                        {
                            //Debug.Log("Pass");
                            continue;
                        }

                        foreach (Material m in r.materials)
                        {
                            r.material = MappedConnectorMat;
                            if (r.materials.Length > 3)
                            {
                                //r.materials[3] = new Material(CentralHarnessConns);

                                Material[] mats = r.materials;
                                mats[3] = new Material(MappedConnectorMat);
                                mats[2] = new Material(MappedConnectorMat);
                                mats[1] = new Material(MappedConnectorMat);

                                r.materials = mats;

                            }
                        }

                        //r.material = MappedConnectorMat;
                    }
                    
                }
            }
            //Debug.Log(allConnectors[i].name + " " + startConn.name);
            if(allConnectors[i].name == startConn.name)
            {
                //Debug.Log("entered");
                rend = startConn.GetComponentsInChildren<Renderer>();

                foreach (Renderer r in rend)
                {
                    if (r.material.name == "FadedMat" || r.material.name == "WhitePlane (Instance)" || r.material.name == "WhitePlane" || r.material.name == "LiberationSans SDF Material (Instance)" ||
                        r.material.name == "Plane_Mat" || r.material.name == "Plane_Mat (Instance)" || r.material.name == "ConnectorBasePlate (Instance)" || r.material.name == "ConnectorHoles (Instance)")
                    {
                        //Debug.Log("Pass");
                        continue;
                    }
                    Debug.Log(r.materials.Length);
                    for(int ij = 0; ij < r.materials.Length; ij++)
                    {
                        r.materials[ij] = ConnectorMat;
                    }
                    r.material = ConnectorMat;
                }
            }
        }
    }
    
    public void ChangeMat(GameObject[] allEndpoints) // assign end connector material to blue when a wire is selected
    {
        currentEnPtObjs = allEndpoints;

        for (int j = 0; j < allEndpoints.Length; j++)
        {
            if(allEndpoints[j] != null)
            {
                children = allEndpoints[j].GetComponentsInChildren<Renderer>();

                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].material.name == "WhitePlane (Instance)" || children[i].material.name == "WhitePlane" ||
                        children[i].material.name == "LiberationSans SDF Material (Instance)" || children[i].material.name == "Plane_Mat" || children[i].material.name == "Plane_Mat (Instance)" || children[i].material.name == "ConnectorBasePlate (Instance)" || children[i].material.name == "ConnectorHoles (Instance)" || children[i].material.name == "Font Material" || children[i].material.name == "Node_Mat" || children[i].material.name == "Font Material (Instance)" || children[i].material.name == "Node_Mat (Instance)")
                    {
                        continue;
                    }

                    if (children[i].material.name == "MappedConnector" || children[i].material.name == "MappedConnector (Instance)")
                    {
                        //continue;
                    }

                    children[i].material = ConnectorMat;

                }
            }
        }
    }

    public void RestoreMaterials()
    {
        for (int j = 0; j < currentEnPtObjs.Length; j++)
        {
            if (currentEnPtObjs[j] != null)
            {
                children = currentEnPtObjs[j].GetComponentsInChildren<Renderer>();
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].material.name == "WhitePlane (Instance)" || children[i].material.name == "WhitePlane" ||
                        children[i].material.name == "LiberationSans SDF Material (Instance)" || children[i].material.name == "Plane_Mat" || children[i].material.name == "Plane_Mat (Instance)" || children[i].material.name == "ConnectorBasePlate (Instance)" || children[i].material.name == "ConnectorHoles (Instance)")
                    {
                        //Debug.Log("Pass");
                        continue;
                    }

                    if (children[i].material.name == "MappedConnector" || children[i].material.name == "MappedConnector (Instance)")
                    {
                        //continue;
                    }

                    children[i].material = MappedConnectorMat;
                }
            }
        }
    }
}

