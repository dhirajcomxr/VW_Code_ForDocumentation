using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllNodesV2 : MonoBehaviour {

    public GameObject[] nodes;

    [ContextMenu("Populate Nodes")]
    void PopulateNodes() {
        List<GameObject> n = new List<GameObject>();

        GameObject[] objs = FindObjectsOfType<GameObject>(true);
        for (var i = 0; i < objs.Length; i++) {
            if (objs[i].name.StartsWith("N-") && !objs[i].name.Contains(" ")) {
                n.Add(objs[i]);
            }
        }
        nodes = n.ToArray();
    }

    public void DisableAllNodes() {
        foreach (GameObject g in nodes) {
            g.SetActive(false);
        }
    }
}
