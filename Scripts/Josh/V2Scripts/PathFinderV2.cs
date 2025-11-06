using IndieMarc.CurvedLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PathFinderV2 : MonoBehaviour {

    public GameObject wireContainer;
    public GameObject wire;
    public GlobalActiveWiresV2 globalWires;

    public bool drawGizmos = false;
    public int currentProcesses = 0;
    public float offset = 0.0015f;

    float timeOut = 5f;
    GameObject[] finalPath;

    Color A;
    Color B;
    float H, S, V;
    MatLerp matLerp;

    // Start is called before the first frame update
    void Start() {
        globalWires = GetComponent<GlobalActiveWiresV2>();
        if (wireContainer == null) {
            GameObject g = new GameObject();
            g.transform.SetParent(this.transform);
            g.transform.position = Vector3.zero;
            g.name = "WireContainer";
            wireContainer = g;
        }
    }

    // Update is called once per frame
    void Update() {

    }

    [ContextMenu("Create Wire")]
    public void CreateWire(GameObject from, GameObject to, int precision, Material mat = null) {
        // generate a random ID for each wire
        int wID = UnityEngine.Random.Range(0, 999999);

        // create a wire for the starting to the mid point
        if (from != null) {
            List<GameObject> gos = new List<GameObject>();
            gos.Add(from);
            finalPath = new GameObject[0];
            Transform[] points = MapA2B(gos, from, to, Time.time);

            Debug.Log("Wire length is " + points.Length);
            if (points.Length > 0) {
                Transform[] updatedPoints = RemoveOverlap(points);
                GameObject newWire = Instantiate(wire);
                if (wire == null) { Debug.Log("null"); }
                //Debug.Log(wire);
                newWire.transform.position = Vector3.zero;
                newWire.GetComponent<CurvedLine3D>().NewMesh();
                newWire.GetComponent<CurvedLine3D>().paths = updatedPoints;

                if (mat != null) {
                    newWire.GetComponent<CurvedLine3D>().material = mat;
                    newWire.GetComponent<MeshRenderer>().material = mat;
                }
                newWire.GetComponent<CurvedLine3D>().precision = precision;

                newWire.GetComponent<CurvedLine3D>().Refresh();
                newWire.name = "W-" + wID;
                newWire.transform.SetParent(wireContainer.transform);
                globalWires.AllWires.Add(newWire);

                matLerp = newWire.GetComponent<MatLerp>();
                A = mat.GetColor("_Color1");
                B = mat.GetColor("_Color2");
                matLerp.colA1 = A;
                matLerp.colB1 = B;
            }
        }
        else {
            Debug.Log("From is null");
        }
        currentProcesses = 0;
    }

    Transform[] RemoveOverlap(Transform[] points) {
        // create and maintain a new list
        List<Transform> updatedPoints = new List<Transform>();

        foreach (Transform t in points) {
            ConnectedObjects conn = t.GetComponent<ConnectedObjects>();
            if (conn.isUsed) {
                GameObject g = new GameObject();
                g.transform.SetParent(t);
                int count = conn.additionalNodes.Count;
                switch (count) {
                    case 0: g.transform.localPosition = new Vector3(offset, offset, 0); break;
                    case 1: g.transform.localPosition = new Vector3(-offset, offset, 0); break;
                    case 2: g.transform.localPosition = new Vector3(0, offset, offset); break;
                    case 3: g.transform.localPosition = new Vector3(0, 0, -offset); break;
                    case 4: g.transform.localPosition = new Vector3(offset, offset, offset); break;
                    case 5: g.transform.localPosition = new Vector3(offset, offset, -offset); break;
                    case 6: g.transform.localPosition = new Vector3(-offset, offset, offset); break;
                    case 7: g.transform.localPosition = new Vector3(-offset, offset, -offset); break;
                    default: g.transform.localPosition = new Vector3(0, 0, 0); break;
                }
                updatedPoints.Add(g.transform);
                conn.additionalNodes.Add(g);
            }
            else {
                updatedPoints.Add(t);
                conn.isUsed = true;
            }
        }
        return updatedPoints.ToArray();
    }

    Transform[] MapA2B(List<GameObject> path, GameObject nextPoint, GameObject goal, float startTime) {
        if (Time.time - startTime > timeOut) {
            Debug.Log("Timeout");
            return finalPath.Select(f => f.transform).ToArray();
        }
        // get all the connected points for that gameobject
        GameObject[] objs = nextPoint.GetComponent<ConnectedObjects>().linkedObjects;
        for (int i = 0; i < objs.Length; i++) {
            currentProcesses++;
            // a condition to make sure that it is not reversed when it reaches the end
            if (!path.Contains(objs[i])) {
                List<GameObject> newPath = new List<GameObject>();
                newPath.AddRange(path);
                newPath.Add(objs[i]);

                // check if the selected object is the end object
                if (objs[i] == goal) {
                    if (finalPath.Length == 0 || finalPath.Length > newPath.ToArray().Length) {
                        finalPath = newPath.ToArray();
                    }
                    Transform[] transArray = finalPath.Select(f => f.transform).ToArray();
                    //wire.GetComponent<CurvedLine3D>().paths = transArray;
                    //wire.GetComponent<CurvedLine3D>().Refresh();
                    currentProcesses--;
                    return transArray;
                }
                else {
                    // the current object is not a node. Continue searching.
                    //Debug.Log(nextPoint.name + " ==> " + objs[i].name);
                    MapA2B(newPath, objs[i], goal, startTime);
                }
            }
            else {
                // same object is found. Probably reached the end.
                //PrintPath("Reached End: ", path);
            }
            currentProcesses--;
        }
        return finalPath.Select(f => f.transform).ToArray();
    }

    void PrintPath(string prefix, List<GameObject> gos) {
        string s = prefix;
        foreach (GameObject g in gos) {
            s += g.name + " => ";
        }
        s = s.TrimEnd(new char[] { ' ', '=', '>' });
        Debug.Log(s);
    }
}