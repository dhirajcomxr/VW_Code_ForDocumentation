using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(Steps))]
public class CameraAngleHelper : MonoBehaviour {

    public Steps RAndRSteps;
    [Tooltip("Drag the csv file here")]
    [SerializeField] TextAsset DismantlingOCP;
    [SerializeField] TextAsset AssemblyOCP;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    [ContextMenu("Export Camera Angles")]
    void ExportOCP() {
        List<string> dSteps = new List<string>();
        List<Step> steps = RAndRSteps.steps;
        dSteps.Add("Sr.No.,Rotation");
        for (int i = 0; i < steps.Count; i++) {
            if (steps[i].overrideCameraPosition != null) {
                dSteps.Add(
                i + ","
                    + steps[i].overrideCameraPosition.transform.position.x + ","
                    + steps[i].overrideCameraPosition.transform.position.y + ","
                    + steps[i].overrideCameraPosition.transform.position.z + ","
                    + steps[i].overrideCameraPosition.transform.eulerAngles.x + ","
                    + steps[i].overrideCameraPosition.transform.eulerAngles.y + ","
                    + steps[i].overrideCameraPosition.transform.eulerAngles.z
                );
            }
        }
        string dStepPath = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + "DismantlingOCP.csv";
        File.WriteAllLines(dStepPath, dSteps.ToArray());

        List<string> aSteps = new List<string>();
        List<Step> assemblySteps = RAndRSteps.assemblySteps;
        aSteps.Add("Sr.No.,Rotation");
        for (int i = 0; i < assemblySteps.Count; i++) {
            if (assemblySteps[i].overrideCameraPosition != null) {
                aSteps.Add(
                    i + ","
                    + assemblySteps[i].overrideCameraPosition.transform.position.x + ","
                    + assemblySteps[i].overrideCameraPosition.transform.position.y + ","
                    + assemblySteps[i].overrideCameraPosition.transform.position.z + ","
                    + assemblySteps[i].overrideCameraPosition.transform.eulerAngles.x + ","
                    + assemblySteps[i].overrideCameraPosition.transform.eulerAngles.y + ","
                    + assemblySteps[i].overrideCameraPosition.transform.eulerAngles.z
                    );
            }
        }
        string aStepPath = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + "AssemblyOCP.csv";
        File.WriteAllLines(aStepPath, aSteps.ToArray());
    }

    [ContextMenu("Import Camera Angles")]
    void LoadOCP() {
        string[] dismantlingLines = Regex.Split(DismantlingOCP.text, System.Environment.NewLine);
        for (int i = 1; i < dismantlingLines.Length; i++) {
            if (Regex.Split(dismantlingLines[i], ",")[0] != "") {
                int stepNo = int.Parse(Regex.Split(dismantlingLines[i], ",")[0]);

                // position
                string px = Regex.Split(dismantlingLines[i], ",")[1];
                string py = Regex.Split(dismantlingLines[i], ",")[2];
                string pz = Regex.Split(dismantlingLines[i], ",")[3];

                // rotation
                string rx = Regex.Split(dismantlingLines[i], ",")[4];
                string ry = Regex.Split(dismantlingLines[i], ",")[5];
                string rz = Regex.Split(dismantlingLines[i], ",")[6];

                if (px != "" && py != "" && pz != "") {
                    if (RAndRSteps.steps[stepNo].overrideCameraPosition != null) {
                        RAndRSteps.steps[stepNo].overrideCameraPosition.position = stringToVector3(px, py, pz);
                        RAndRSteps.steps[stepNo].overrideCameraPosition.rotation = Quaternion.Euler(stringToVector3(rx, ry, rz));
                    }
                    else {
                        Debug.Log(stepNo + " is Null");
                    }
                }
            }
        }

        string[] assemblyLines = Regex.Split(AssemblyOCP.text, System.Environment.NewLine);
        for (int i = 1; i < assemblyLines.Length; i++) {
            if (Regex.Split(assemblyLines[i], ",")[0] != "") {
                int stepNo = int.Parse(Regex.Split(assemblyLines[i], ",")[0]);

                // position
                string px = Regex.Split(assemblyLines[i], ",")[1];
                string py = Regex.Split(assemblyLines[i], ",")[2];
                string pz = Regex.Split(assemblyLines[i], ",")[3];

                // rotation
                string rx = Regex.Split(assemblyLines[i], ",")[4];
                string ry = Regex.Split(assemblyLines[i], ",")[5];
                string rz = Regex.Split(assemblyLines[i], ",")[6];

                if (px != "" && py != "" && pz != "") {
                    if (RAndRSteps.assemblySteps[stepNo].overrideCameraPosition != null) {
                        RAndRSteps.assemblySteps[stepNo].overrideCameraPosition.position = stringToVector3(px, py, pz);
                        RAndRSteps.assemblySteps[stepNo].overrideCameraPosition.rotation = Quaternion.Euler(stringToVector3(rx, ry, rz));
                    }
                    else {
                        Debug.Log(stepNo + " is Null");
                    }
                }
            }
        }
        Debug.Log("Done");
    }

    Vector3 stringToVector3(string x, string y, string z) {
        float x1 = float.Parse(x);
        float y1 = float.Parse(y);
        float z1 = float.Parse(z);
        return new Vector3(x1, y1, z1);
    }
}