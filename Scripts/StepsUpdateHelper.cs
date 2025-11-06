using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StepsUpdateHelper : MonoBehaviour {

    public Steps stepsMain;
    public string stepsCSV = "";
    public Steps.Process process;

    [Header("Add Steps")]
    [Tooltip("First = 0\nLast = -1")]
    public int insertAt = -1;

    [Header("Update Step Text")]
    public string separator = ":;:";

    [Header("CSV Column Mapping. Col A = 0.")]
    public int Locate = 1;
    public int StepInstr = 2;
    public int locked = 3;
    public int cautionNotes = 5;
    public int torqueVal = 6;
    public int toolUsed = 7;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    [ContextMenu("Add Steps")]
    public void UpdateSteps() {
        TextAsset data = Resources.Load(stepsCSV) as TextAsset;
        string[] lines = Regex.Split(data.text, System.Environment.NewLine);
        Debug.Log(lines.Length);
        int startPosition = insertAt;
        for (int i = 1; i < lines.Length; i++) {
            Debug.Log(lines[i].ToString());
            string locateText = Regex.Split(lines[i], ",")[Locate];
            string stepInstr = Regex.Split(lines[i], ",")[StepInstr];
            string isLocked = Regex.Split(lines[i], ",")[locked];
            string caution = Regex.Split(lines[i], ",")[cautionNotes];
            string torque = Regex.Split(lines[i], ",")[torqueVal];
            string toolName = Regex.Split(lines[i], ",")[toolUsed];

            Step s = new Step();
            s.locateObjectText = locateText;
            s.stepInstructions = stepInstr;
            s.isLocked = (isLocked.ToLower() == "true") ? true : false;
            s.cautionNotes = caution;
            s.torque = torque;
            s.specialToolName = toolName;
            if (process == Steps.Process.Dismantling) {
                if (startPosition == -1) {
                    stepsMain.steps.Add(s);
                }
                else if (startPosition == 0) {
                    stepsMain.steps.Insert(0, s);
                    startPosition++;
                }
                else if (stepsMain.steps.Count > startPosition) {
                    stepsMain.steps.Insert(startPosition, s);
                    startPosition++;
                }
                else {
                    Debug.LogError("Invalid Insert Index");
                }
            }
            else if (process == Steps.Process.Assembly) {
                if (startPosition == -1) {
                    stepsMain.assemblySteps.Add(s);
                }
                else if (startPosition == 0) {
                    stepsMain.assemblySteps.Insert(0, s);
                    startPosition++;
                }
                else if (stepsMain.assemblySteps.Count > startPosition) {
                    stepsMain.assemblySteps.Insert(startPosition, s);
                    startPosition++;
                }
                else {
                    Debug.LogError("Invalid Insert Index");
                }
            }
        }
    }

    [ContextMenu("Update Step Text")]
    public void UpdateStepText() {
        TextAsset data = Resources.Load(stepsCSV) as TextAsset;
        string[] lines = Regex.Split(data.text, System.Environment.NewLine);
        for (int i = 1; i < lines.Length; i++) {
            int srNo = int.Parse(Regex.Split(lines[i], ",")[0]);
            string locate = Regex.Split(lines[i], ",")[Locate];
            string stepInstr = Regex.Split(lines[i], ",")[StepInstr];
            string caution = Regex.Split(lines[i], ",")[cautionNotes];
            string toolName = Regex.Split(lines[i], ",")[toolUsed];
            List<Step> s = new List<Step>();
            if (process == Steps.Process.Dismantling) {
                s = stepsMain.steps;
            }
            if (process == Steps.Process.Assembly) {
                s = stepsMain.assemblySteps;
            }
            s[srNo - 1].locateObjectText = locate.Replace(separator,",");
            s[srNo - 1].stepInstructions = stepInstr.Replace(separator, ",");
            s[srNo - 1].cautionNotes = caution.Replace(separator, ",");
            s[srNo - 1].specialToolName = toolName.Replace(separator, ",");
            if (torqueVal != -1) {
                string torque = Regex.Split(lines[i], ",")[torqueVal].Replace(separator, ",");
                s[srNo - 1].torque = torque;
            }
        }
    }
}
