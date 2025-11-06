using IndieMarc.CurvedLine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalActiveWires : MonoBehaviour
{
    public List<GameObject> AllWires;

    public void DestroyWires() {
        // Destroy all existing wires
        foreach (GameObject g in AllWires) {
            Debug.Log(g.name);
            foreach (Transform t in g.GetComponent<CurvedLine3D>().paths) {
                if (t.GetComponent<ConnectedObjects>() != null) {
                    t.GetComponent<ConnectedObjects>().isUsed = false;
                    t.GetComponent<ConnectedObjects>().ClearAdditionalNodes();
                }
            }
            Destroy(g);
        }
        AllWires = new List<GameObject>();
    }
}
