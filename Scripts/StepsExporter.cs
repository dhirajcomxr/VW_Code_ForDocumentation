using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

public class StepsExporter : MonoBehaviour {

    public Steps stepsScript;
    public string moduleName = "";
    public string fileName = "";
    public bool isAssembly = false;

    [ContextMenu("Export Steps to CSV")]
    void ExportToCSV() {
        string pathPrefix = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar + moduleName + "_";

        List<string> dSteps = new List<string>();
        dSteps.Add("Sr.No.,Locate part instruction,Step Instruction,isLocked,Tool Name,Torque,Caution Notes");
        for (int i = 0; i < stepsScript.steps.Count; i++) {
       /*     if (stepsScript.steps[i].isLocked.ToString().ToLower() != "true") {*/
                dSteps.Add(
                    (i + 1) + ","
                    + stepsScript.steps[i].locateObjectText.Replace(",", "") + ","
                    + stepsScript.steps[i].stepInstructions.Replace(",", "") + ","
                    + stepsScript.steps[i].isLocked.ToString() + ","
                    + stepsScript.steps[i].specialToolName.Replace(",", "") + ","
                    + stepsScript.steps[i].torque.Replace(",", "") + ","
                    + stepsScript.steps[i].cautionNotes.Replace(",", "")
                    );
          //  }
        }
        string dStepPath = pathPrefix + "DismantlingSteps.csv";
        File.WriteAllLines(dStepPath, dSteps.ToArray());

        List<string> aSteps = new List<string>();
        aSteps.Add("Sr.No.,Locate part instruction,Step Instruction,isLocked,Tool Name,Torque,Caution Notes");
        for (int i = 0; i < stepsScript.assemblySteps.Count; i++) {
           /* if (stepsScript.assemblySteps[i].isLocked.ToString().ToLower() != "true") {*/
                aSteps.Add(
                (i + 1) + ","
                + stepsScript.assemblySteps[i].locateObjectText.Replace(",", "") + ","
                + stepsScript.assemblySteps[i].stepInstructions.Replace(",", "") + ","
                + stepsScript.assemblySteps[i].isLocked.ToString() + ","
                + stepsScript.assemblySteps[i].specialToolName.Replace(",", "") + ","
                + stepsScript.assemblySteps[i].torque.Replace(",", "") + ","
                + stepsScript.assemblySteps[i].cautionNotes.Replace(",", "")
                );
           // }
        }
        string aStepPath = pathPrefix + "Assembly.csv";
        File.WriteAllLines(aStepPath, aSteps.ToArray());
    }

    [ContextMenu("Update Steps")]
    public void ReadSteps() {
        if (fileName == "") {
            return;
        }
        string pathPrefix = Application.dataPath + Path.DirectorySeparatorChar + "Dump" + Path.DirectorySeparatorChar;
        string filePath = pathPrefix + fileName;
        Debug.Log(filePath);
        Debug.Log("[Path]"+filePath);

        string[] lines = File.ReadAllLines(filePath);
        Debug.Log(lines.Length);
        for (int i = 1; i < lines.Length; i++) { // skip the 1st line
            string[] s = Regex.Split(lines[i], ",");
            int index = int.Parse(s[0]) - 1;
            string locateTxt = s[1];
            string instr = s[2];
            string toolName = s[4];
            string torque = s[5];
            string cautionNotes = s[6];
            if (isAssembly) {
                stepsScript.assemblySteps[index].locateObjectText = locateTxt;
                stepsScript.assemblySteps[index].stepInstructions = instr;
                stepsScript.assemblySteps[index].specialToolName = toolName;
                stepsScript.assemblySteps[index].torque = torque;
                stepsScript.assemblySteps[index].cautionNotes = cautionNotes;
            }
            else {
                stepsScript.steps[index].locateObjectText = locateTxt;
                stepsScript.steps[index].stepInstructions = instr;
                stepsScript.steps[index].specialToolName = toolName;
                stepsScript.steps[index].torque = torque;
                stepsScript.steps[index].cautionNotes = cautionNotes;
            }
        }
    }
}
