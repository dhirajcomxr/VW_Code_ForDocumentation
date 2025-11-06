using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class ExplodedObjects {
    public string name;
    public GameObject part;
}

public class ObjectFinder : MonoBehaviour {

    string path = "D:/parts.csv";
    public GameObject root;
    public List<ExplodedObjects> explodedObjs;
    Transform[] objs;

    // Start is called before the first frame update
    void Start() {

    }
    private void OnEnable() {
        explodedObjs = new List<ExplodedObjects>();
        if (File.Exists(path)) {
            string[] lines = File.ReadAllLines(path);
            objs = root.GetComponentsInChildren<Transform>(true);

            foreach (string line in lines) {
                if (line.Contains(",")) {
                    string partID = Regex.Split(line, ",")[0];
                    string partName = Regex.Split(line, ",")[1];
                    LookFor(partID, partName);
                }
            }
        }
        else {
            Debug.Log("File not found");
        }
    }

    void LookFor(string partID, string partName) {
        string[] split = Regex.Split(partID, " ");
        string newID = "";

        for (int i = 0; i < (split.Length > 3 ? 3 : split.Length); i++) {
            newID += split[i];
        }
        bool found = false;
        foreach (Transform t in objs) {
            string name = t.name.Replace("_", "").Replace("-", "").ToLower();
            if (name.StartsWith(newID.ToLower())) {
                ExplodedObjects obj = new ExplodedObjects();
                obj.part = t.gameObject;
                obj.name = partName;
                explodedObjs.Add(obj);
                found = true;
                break;
            }
        }
        if (!found) {
            Debug.Log(partID + " not found");
        }
    }
}
