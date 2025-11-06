using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartNameUpdater : MonoBehaviour {

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    [ContextMenu("Update Gameobject Names")]
    void UpdateNames() {
        Transform[] gos = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in gos) {
            if (t.name.Contains("/")) {
                string newName = t.name.Replace("/", "--");
                t.name = newName;
            }
        }
    }
}
