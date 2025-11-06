using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class TestNewDT : MonoBehaviour
{
    public bool editMode = false;
    public DiagnosticTreeManager manager;
    public DTPlayer dtPlayer;
    public TextAsset asset;
    public int loadDataId = 0;
    public bool loadAsNewTree=false,convToString = false,getFromString=false,
        getSteps,saveNow=false,loadNowToString=false,loadFromFile=false,showStepNum;
    [Space(10)]
    public string result = "";
    [Space(10)]
    public DiagnosticTree tree;
    void Update()
    {
        if (editMode)
        {
            if (showStepNum)
            {
                showStepNum = false;
                dtPlayer.ShowStep(loadDataId);
            }
            if (loadFromFile)
            {
                Debug.Log("Loading from file");
                loadFromFile = false;
              tree=  DiagnosticTreeFactory.LoadFromFile(asset);
            }
            if (loadAsNewTree)
            {
              
                loadAsNewTree = false;
                LoadToNew(loadDataId);
            }
            if (getSteps)
            {
                getSteps = false;
                tree.steps = manager.GetSteps();
            }
            if (convToString)
            {
                convToString = false;
                //  tree.steps[0].answers = new DTAnswer[] {new DTAnswer.DirectAnswer("TEST DIRECT 2"),new DTAnswer.InputAnswerGreater("TEST TEST")}
                result = JsonUtility.ToJson(tree);
            }
            if (getFromString)
            {
                getFromString = false;
                tree = JsonUtility.FromJson<DiagnosticTree>(result);
            }
            if (saveNow)
            {
                Debug.Log("Saving " + tree.complaintName);
                saveNow = false;
                DiagnosticTreeFactory.SaveToFile(tree);
            }
            if (loadNowToString)
            {
                loadNowToString = false;
                DiagnosticTree t = DiagnosticTreeFactory.LoadFromFile(asset);
                result = JsonUtility.ToJson(t);
            }
            editMode = false;
        }
    }
    void LoadToNew(int id)
    {
        manager.LoadData(id);
        tree.steps = manager.GetSteps();
        if (tree.steps.Count > 0)
        {
            for (int i = 0; i < tree.steps.Count; i++)
            {

                Debug.Log("STEP: " + i+" inst "+tree.steps[i].instruction);
                if (tree.steps[i].output == null)
                    tree.steps[i].output = new DiagnosticStepOutput();
                // Yes and No Buttons
                if (tree.steps[i].on_no.Length > 0 && tree.steps[i].on_yes.Length > 0)
                {
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.YesNo;
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.Direct;
                    List<DiagnosticStepOutput.NamedOutput> namedOutputs = new List<DiagnosticStepOutput.NamedOutput>();
                    namedOutputs.Add(new DiagnosticStepOutput.NamedOutput("Yes", tree.steps[i].on_yes));
                    namedOutputs.Add(new DiagnosticStepOutput.NamedOutput("No", tree.steps[i].on_no));
                    tree.steps[i].output.outputs = namedOutputs.ToArray();
                }
                else// Next if On Yes Only has id
                if (tree.steps[i].on_yes.Length > 0)
                {
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.Next;
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.Direct;
                    tree.steps[i].output.outputs = new DiagnosticStepOutput.NamedOutput[]
                    {
                        new DiagnosticStepOutput.NamedOutput("Next",tree.steps[i].on_yes)
                    };
                }
                else// Next if Blank on yes and no and Method field
                if (tree.steps[i].method.Length < 1)
                {
                    if ((i + 1) < tree.steps.Count)
                    {
                        tree.steps[i].output.method = DiagnosticStepOutput.Method.Next;
                        tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.Direct;
                        tree.steps[i].output.outputs = new DiagnosticStepOutput.NamedOutput[]
                        {
                        new DiagnosticStepOutput.NamedOutput("Next",tree.steps[i+1].activity_id)
                        };
                    }
                }
                //Dropdown
                if (tree.steps[i].method.ToLower().Contains("opt"))
                {
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.DropDown;
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.Direct;
                    if (tree.steps[i].buttonNames.Length > 0)
                    {
                        string[] names = tree.steps[i].buttonNames;
                        Debug.Log("<color=green>Found Dropdown Options:</color>" + tree.steps[i].buttonNames.Length);
                        List<DiagnosticStepOutput.NamedOutput> namedOps = new List<DiagnosticStepOutput.NamedOutput>();
                        for (int j = 0; j < names.Length; j++)
                        {
                            Debug.LogError("INPUT ID FOR " + tree.steps[i].instruction);
                            namedOps.Add(new DiagnosticStepOutput.NamedOutput(names[j],tree.steps[i].buttons[j+1]));
                        }
                        tree.steps[i].output.outputs = namedOps.ToArray();
                    }
                    else
                        Debug.LogError("No Button Names Found For Dropdown!");
                }
                //Table
                if (tree.steps[i].method.ToLower().Contains("tab"))
                {
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.Table;
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.AtleastXInputsInRange;
                    tree.steps[i].output.vars = new float[] { 0.04f, 200, 3f };
                    List<DiagnosticStepOutput.NamedInput> totalInputs = 
                        new List<DiagnosticStepOutput.NamedInput>(GetTableInputsFor(tree.steps[i], 10, "SB "));
                    totalInputs.AddRange(GetTableInputsFor(tree.steps[i], 10, "SC "));
                    tree.steps[i].output.inputs = totalInputs.ToArray();
                }
                //Range
                if (tree.steps[i].method.ToLower().Contains("rang"))
                {
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.InputValues;
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.AnyInputInRange;
                    tree.steps[i].output.inputs = new DiagnosticStepOutput.NamedInput[] { new DiagnosticStepOutput.NamedInput("Enter Value", 0) };
                }
                //Range 2 Vals
                if (tree.steps[i].method.ToLower().Contains("calc")) {
                    tree.steps[i].output.inputs = new DiagnosticStepOutput.NamedInput[]
                    {
                        new DiagnosticStepOutput.NamedInput("Charging Current",0),
                        new DiagnosticStepOutput.NamedInput("Battery Ah Capacity",0)
                    };
                    tree.steps[i].output.method = DiagnosticStepOutput.Method.InputValues;
                    tree.steps[i].output.vars = new float[] { 0.1f };//variable X
                    tree.steps[i].output.logic = DiagnosticStepOutput.InputOutputLogic.InputALessThanXTimesB;
                    }
                    
            }
            Debug.Log("Finished Conversion");
        }
    }
    public static DiagnosticStepOutput.NamedInput[] GetTableInputsFor(DiagnosticStep step,int numberInputs,string labelPrefix)
    {
        List<DiagnosticStepOutput.NamedInput> inputs = new List<DiagnosticStepOutput.NamedInput>();
        for (int i = 0; i < numberInputs; i++)
        {
            inputs.Add(new DiagnosticStepOutput.NamedInput(labelPrefix + (i + 1), 0));
        }
        return inputs.ToArray();
    }
}
