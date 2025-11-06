using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HarnessPathMapper : MonoBehaviour {

    public GameObject harness;
    public GameObject pointer;
    public GameObject prefab;
    public GameObject rootNode, lastNode;
    public float distanceThreshold = 0.1f;
    
    int counter = 0;
    string tag = "Node";

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, 100f)) {
            pointer.transform.position = hit.point;
            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt)) {
                if (lastNode == null && counter > 0) {
                    Debug.LogError("<color=yellow>Make a selection before starting the mapping</color>");
                    return;
                }
                if (hit.transform.tag != tag) {
                    GameObject g = Instantiate(prefab);
                    counter++;
                    g.transform.position = hit.point;
                    g.SetActive(true);
                    g.name = "Node" + counter.ToString("00000");
                    g.transform.SetParent(rootNode.transform);
                    if (lastNode != null) {
                        List<GameObject> fwd = g.GetComponent<ConnectedObjects>().linkedObjects.ToList<GameObject>();
                        fwd.Add(lastNode);
                        g.GetComponent<ConnectedObjects>().linkedObjects = fwd.ToArray();

                        List<GameObject> rev = lastNode.GetComponent<ConnectedObjects>().linkedObjects.ToList<GameObject>();
                        rev.Add(g);
                        lastNode.GetComponent<ConnectedObjects>().linkedObjects = rev.ToArray();
                    }
                    lastNode = g;
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                if (hit.transform.tag == tag) {
                    lastNode = hit.transform.gameObject;
                }
            }
        }
        else {
            pointer.transform.position = Vector3.one * 10000f;
        }

        if (Input.GetKeyDown(KeyCode.Delete) && lastNode != null) {
            Destroy(lastNode);
        }
    }

    [ContextMenu("Add Colliders")]
    public void AddMeshColliders() {
        MeshRenderer[] renderers = harness.GetComponentsInChildren<MeshRenderer>();
        foreach (Renderer r in renderers) {
            r.gameObject.AddComponent<MeshCollider>();
        }
    }

    [ContextMenu("Check Distance")]
    public void checkDistance() {
        ConnectedObjects[] objs = rootNode.GetComponentsInChildren<ConnectedObjects>();
        foreach (ConnectedObjects obj in objs) {
            foreach (GameObject g in obj.linkedObjects) {
                obj.tooClose = Vector3.Distance(g.transform.position, this.transform.position) < distanceThreshold ? true : obj.tooClose;
            }
        }
    }


    [ContextMenu("Cleanup Nodes")]
    public void CleanupNodes() {
        ConnectedObjects[] objs = rootNode.GetComponentsInChildren<ConnectedObjects>();
        foreach (ConnectedObjects obj in objs) {
            if (obj.gameObject.GetComponent<Collider>() != null) {
                Destroy(obj.gameObject.GetComponent<Collider>());
                Destroy(obj.gameObject.GetComponent<MeshRenderer>());
                Destroy(obj.gameObject.GetComponent<MeshFilter>());
            }
        }
    }
}
