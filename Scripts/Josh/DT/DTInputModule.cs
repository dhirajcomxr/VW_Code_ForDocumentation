using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class DTInputModule : MonoBehaviour
{
    [SerializeField] bool debug = false;
    bool errorFoundInInput = false;
    UnityAction<DiagnosticStep> onInputRecieved;
   public UnityAction<string> onInputError,onInput;

    [SerializeField] GameObject[] inputMethodPanels;
 //   [SerializeField] Button nextButton, yesButton, noButton;
    [Space(10)]
    [SerializeField] GameObject singleInputPanel;
    [SerializeField] UiInputFieldLabel inputValue;
    [Space(10)]
    [SerializeField] GameObject doubleInputPanel;
    [SerializeField] UiInputFieldLabel abInputValA, abInputValB;
    [Space(10)]
    [SerializeField] Dropdown dropdown;
    [SerializeField] UiInputFieldLabelList table;
    [SerializeField] UiInputFieldLabel tableRowItem;
    [Space(10)]
    [SerializeField] GameObject inputTextPanel;
    [SerializeField] UiInputFieldLabel inputText;
    [Header("Runtime")]
    [SerializeField] DiagnosticStep currentStep;
    [SerializeField] DiagnosticStepOutput.Method currentStepMethod;
    public float[] inputVals;
    [SerializeField] int selectedOption;
    [SerializeField] InputModuleType moduleType;
    [SerializeField] GameObject submitButton;
    public enum InputModuleType
    {
        Next, YesNo, Options, Dropdown, Table, TwinInput
    }
    public void SetInputEvaluator(UnityAction<DiagnosticStep> evaluator)
    {
        onInputRecieved = evaluator;
    }

    public bool YesNoMethod;
    public void SetInputFor(DiagnosticStep step)
    {
        Debug.Log("Input Selected " + step.output.method.ToString());
    
        currentStep = step;
        currentStepMethod = step.output.method;
        setStateForGameObjects(new GameObject[] { singleInputPanel, doubleInputPanel, inputTextPanel}, false);
        //Disable all panels 
        foreach (var item in inputMethodPanels)
        {
            item.SetActive(false);
        }
        //Enable Panel for currently selected Panel
        inputMethodPanels[(int)step.output.method].SetActive(true);
        switch (step.output.method)
        {
            case DiagnosticStepOutput.Method.Next:
                break;
            case DiagnosticStepOutput.Method.YesNo:
                {
                    YesNoMethod = true;
                }
                break;
            case DiagnosticStepOutput.Method.DropDown:
                {
                    dropdown.ClearOptions();
                    List<string> dropdownOptions = new List<string>(step.output.outputs.Length);
                    for (int i = 0; i < step.output.outputs.Length; i++)
                    {
                        dropdownOptions.Add(step.output.outputs[i].name);
                    }
                    dropdown.AddOptions(dropdownOptions);
                }
                break;
            case DiagnosticStepOutput.Method.InputValues:
                int totalInputs = step.output.inputs.Length;
                if (totalInputs == 1)
                {
                    inputValue.label.text = step.output.inputs[0].name;

                    singleInputPanel.SetActive(true);
                }
                if (totalInputs == 2)
                {
                    doubleInputPanel.SetActive(true);
                    abInputValA.label.text = step.output.inputs[0].name;
                    abInputValB.label.text = step.output.inputs[1].name;
                }
                break;
            case DiagnosticStepOutput.Method.Table:
                {
                    List<string> tableValueLabels = new List<string>(step.output.inputs.Length);
                    for (int i = 0; i < step.output.inputs.Length; i++)
                    {
                        tableValueLabels.Add(step.output.inputs[i].name);
                    }
                    table.Setup(tableValueLabels.ToArray(), "Enter Value");

                }
                break;
            case DiagnosticStepOutput.Method.InputText:
                inputText.label.text = step.output.textInput.name;
                inputTextPanel.SetActive(true);
                break;
            default:

                break;
        }
    }
    void setStateForGameObjects(GameObject[] gs,bool state)
    {
        for (int i = 0; i < gs.Length; i++)
        {
            gs[i].SetActive(state);
        }
    }

    public void OnTableSubmit()
    {
        List<string> tableValues = table.GetAllInputs();
        string answerString = "";
        int totalInputsRecieved = 0;
        if (tableValues.Count < 1)
        {
            errorFoundInInput = true;
            onInputError?.Invoke("Please Input Values to Continue");
            Debug.LogError("There seems to be an issue with this table, Please Check", table.gameObject);
        }
        else
        {
            if (currentStep.output.inputs.Length != tableValues.Count)
                Debug.LogError("There seems to be a mismatch between available and required inputs");
            DiagnosticStepOutput.NamedInput[] namedInputs = currentStep.output.inputs;
          
            for (int i = 0; i < namedInputs.Length; i++)
            {
                float curVal = 0;
                if (tableValues[i].Length > 0)
                {
                    if (!float.TryParse(tableValues[i], out curVal))
                        Debug.LogError("Please Check InputField Setting for " + table.name, table);
                    namedInputs[i].val = curVal;
                    totalInputsRecieved++;
                    if (i > 0)
                        answerString += ",";
                    answerString += namedInputs[i].name + ":" + curVal;
                }
            }
           
        }
        if (totalInputsRecieved < 1)
        {
            onInputError?.Invoke("Please Input Values to Continue");
            Debug.LogError("There seems to be an issue with this table, Please Check", table.gameObject);
        }
        else
        {
            onInput?.Invoke(answerString);
            OnInputRecieved();
        }
      
    }
    public void OnNextButton()
    {
        selectedOption = currentStep.output.selectedOutput;
        onInput?.Invoke("Next");
        OnInputRecieved();      
    }
    public void OnSubmitText()
    {
        if (inputText.inputField.text.Length < 0)
        {
            onInputError?.Invoke("Please enter input to Continue");
        }
        else
        {
            currentStep.output.textInput.val = inputText.inputField.text;
            onInput?.Invoke(currentStep.output.textInput.name + ":" + currentStep.output.textInput.val);
            OnInputRecieved();
        }
    }
    public void OnYesButton()
    {
        selectedOption = 0;
        onInput?.Invoke("Yes");
        OnInputRecieved();
      
    }
    public void OnNoButton()
    {
        selectedOption = 1;
        onInput?.Invoke("No");
        OnInputRecieved();
       
    }
    /// <summary>
    /// Dhiraj
    /// Changes made on 28th Jan 2025
    /// Added "submitButton.SetActive(false);"
    /// In the DT where evere DT have Dropdwon option after completing 1st step other steps "Next" button is enabled before selecting drop down option
    /// </summary>
    public void OnDropdownDone()
    {
        
        submitButton.SetActive(false);
        selectedOption = dropdown.value;
        onInput?.Invoke(dropdown.options[dropdown.value].text);
        OnInputRecieved();
       
    }
    public void OnSubmitValues()
    {
        int totalInputs = currentStep.output.inputs.Length;
        float curVal=0, curValA=0, curValB=0;
        errorFoundInInput = false;
        if (totalInputs == 1)
        {
             curVal = 0;
            if (!float.TryParse(inputValue.inputField.text, out curVal))
            {
                errorFoundInInput = true;
                onInputError?.Invoke("Please Input Value to Continue");
                
                Debug.LogError("Please Check InputField Setting for " + table.name, table);
            }
            currentStep.output.inputs[0].val = curVal;
        }
        else
            if (totalInputs == 2)
        {
            curValA = 0;
            curValB =0;
            if (!float.TryParse(abInputValA.inputField.text, out curValA))
            {

                abInputValA.inputField.Select();
                errorFoundInInput = true;
                onInputError?.Invoke("Please Input Value to Continue!");
                Debug.LogError("Please Check InputField Setting for " + table.name, table);
            }
            if (!float.TryParse(abInputValB.inputField.text, out curValB))
            {
                abInputValB.inputField.Select();
                errorFoundInInput = true;
                onInputError?.Invoke("Please Input Value to Continue!");
                Debug.LogError("Please Check InputField Setting for " + table.name, table);
            }
           
            currentStep.output.inputs[0].val = curValA;
            currentStep.output.inputs[1].val = curValB;
        }
        if (!errorFoundInInput)
        {
            if (totalInputs == 1)
                onInput?.Invoke(currentStep.output.inputs[0].name + ":" + curVal);
            else
                  if (totalInputs == 2)
                onInput?.Invoke(currentStep.output.inputs[0].name + ":" + curValA + "," +
                    currentStep.output.inputs[1].name + ":" + curValB);

            OnInputRecieved();         
        }
        
    }
    public void OnInputRecieved()
    {
        if(debug)
        Debug.Log("Recieved Input for DT");
        Debug.Log("Before" + currentStep.output.selectedOutput);
        currentStep.output.selectedOutput = selectedOption;
        Debug.Log("After"+currentStep.output.selectedOutput);
        onInputRecieved?.Invoke(currentStep);
    }
    //public void Setup(string[] names)
    //{

    //    switch (moduleType)
    //    {
    //        case InputModuleType.Next:
    //            break;
    //        case InputModuleType.YesNo:
    //            break;
    //        case InputModuleType.Options:
    //            dropdown.ClearOptions();
    //            dropdown.AddOptions(new List<string>(names));
    //            break;
    //        case InputModuleType.Dropdown:
    //            dropdown.ClearOptions();
    //            dropdown.AddOptions(new List<string>(names));
    //            break;
    //        case InputModuleType.Table:
    //            for (int i = 0; i < names.Length; i++)
    //            {
    //                inputFields[i].placeholder.gameObject.GetComponent<Text>().text = names[i];
    //            }
    //            break;
    //        case InputModuleType.TwinInput:
    //            inputFields[0].placeholder.gameObject.GetComponent<Text>().text = names[0];
    //            inputFields[1].placeholder.gameObject.GetComponent<Text>().text = names[1];
    //            break;
    //        default:
    //            break;
    //    }

    //    if (buttons.Length > 0)
    //    {
    //        for (int i = 0; i < buttons.Length; i++)
    //        {
    //            int ip = i;
    //            buttons[i].onClick.AddListener(() => OnSelect(ip));
    //        }
    //    }
    //}
    //public void OnSelect(int id)
    //{
    //    Debug.Log("Selected : " + id);      
    //}
    //public void Evaluate()
    //{
    //    if (inputFields.Length > 0)
    //    {
    //        inputVals = new float[inputFields.Length];
    //        for (int i = 0; i < inputFields.Length; i++)
    //        {
    //            float.Parse(inputFields[i].text);
    //        }
    //    }       
    //}
}
