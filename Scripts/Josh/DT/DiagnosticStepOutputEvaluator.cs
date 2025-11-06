using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class DiagnosticStepOutputEvaluator
{
    public static UnityAction<string> onDisplayText;

    public static Dictionary<int, float> savedValues;

    public static string Evaluate(DiagnosticStep step)
    {
        Debug.Log("Evaluate for " + step.instruction);
        return Evaluate(step.output);
    }
    public static string Evaluate(DiagnosticStepOutput output)
    {
        string result = "";
        switch (output.method)
        {
            case DiagnosticStepOutput.Method.Next:
                output.selectedOutput = 0;
                result = output.outputs[0].outputId;
                string msgTst = EvaluateLogic(output);
                break;
            case DiagnosticStepOutput.Method.YesNo:
                result = output.outputs[output.selectedOutput].outputId;
                msgTst = EvaluateLogic(output);
                break;
            case DiagnosticStepOutput.Method.DropDown:
                result = output.outputs[output.selectedOutput].outputId;
                msgTst = EvaluateLogic(output);
                break;
            case DiagnosticStepOutput.Method.InputValues:
                result = EvaluateLogic(output);
                break;
            case DiagnosticStepOutput.Method.Table:
                result = EvaluateLogic(output);
                break;
            case DiagnosticStepOutput.Method.InputText:
                result = EvaluateLogic(output);
                break;
            default:
                break;
        }
        return result;
    }
    static float[] GetValuesFromNamedInputs(DiagnosticStepOutput.NamedInput[] inputs)
    {
        float[] result = new float[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            result[i] = inputs[i].val;
        }
        return result;

    }

    static string EvaluateLogic(DiagnosticStepOutput dtOutput)
    {
        float[] inputVals = GetValuesFromNamedInputs(dtOutput.inputs);
        float min = 0, max = 0, multiplier = 0;
        int x = 0;
        if (dtOutput.vars.Length > 0)
        {
            multiplier = dtOutput.vars[0];
            min = dtOutput.vars[0];
        }
        if (dtOutput.vars.Length > 1)
            max = dtOutput.vars[1];
        if (dtOutput.vars.Length > 2)
        {

            x = (int)dtOutput.vars[2];
        }

        bool found = false;
        switch (dtOutput.logic)
        {
            case DiagnosticStepOutput.InputOutputLogic.Direct:
                found = true;
                break;
            case DiagnosticStepOutput.InputOutputLogic.AnyInputInRange:
                for (int i = 0; i < inputVals.Length; i++)
                {
                    //Input Field range Set Here
                    Debug.Log("<color=blue>INPUT FOR ANY_RANGE num</color>" + i + " is " + inputVals[i]);
                    if (min <= inputVals[i] && max >= inputVals[i])
                        found = true;
                }
                break;
            case DiagnosticStepOutput.InputOutputLogic.AtleastXInputsInRange:
                {
                    int times = 0;
                    Debug.Log("<color=blue>Total INPUTS FOR AtleastXInputsInRange</color> is " + inputVals.Length);
                    for (int i = 0; i < inputVals.Length; i++)
                    {

                        if (inputVals[i] > min && inputVals[i] < max)
                        {
                            Debug.Log(inputVals[i] + " is between " + min + " and " + max);
                            times++;
                        }
                    }
                    Debug.Log("<color=blue>Total Times FOR</color> is " + times + ", min:" + min + ", max :" + max);
                    if (times > x)
                        found = true;
                }
                break;
            case DiagnosticStepOutput.InputOutputLogic.InputALessThanXTimesB:
                Debug.Log("A(" + inputVals[0] + ") < (B (" + inputVals[1] + ")* " + multiplier);
                found = (inputVals[0] < (inputVals[1] * multiplier));
                break;
            case DiagnosticStepOutput.InputOutputLogic.InputAGreaterThanXTimesB:
                Debug.Log("A(" + inputVals[0] + ") > (B (" + inputVals[1] + ")* " + multiplier);
                found = (inputVals[0] > (inputVals[1] * multiplier));
                break;
            case DiagnosticStepOutput.InputOutputLogic.SaveValTo:
                Debug.Log("Saving " + inputVals[0] + " to " + dtOutput.selectors[0]);
                SaveValue(dtOutput.selectors[0], inputVals[0]);
                found = true;
                break;
            case DiagnosticStepOutput.InputOutputLogic.InputALessThanXTimesSavedStepInput:
                Debug.Log($"Seclector Count : {dtOutput.selectors.Length}");
//DHIRAJ COMMENTED : 07Apr2025

                // if (!savedValues.ContainsKey(dtOutput.selectors[0]))
                // {
                //     Debug.LogError("No Value Saved At " + x);
                // }
                // else
                // {
                //     float savedVal = savedValues[dtOutput.selectors[0]];
                //     Debug.Log("A(" + inputVals[0] + ") < (B (" + savedVal + ")* " + multiplier);
                //     found = (inputVals[0] < (savedVal * multiplier));
                // }
                break;
            case DiagnosticStepOutput.InputOutputLogic.DisplayTopXVals:
                {
                    string valStr = GetNameOfTop(dtOutput);
                    if (valStr.Length > 0)
                        found = true;
                    Debug.Log("Top Vals :" + valStr);
                    onDisplayText?.Invoke(dtOutput.displayText.Replace("{{VALS}}", valStr));
                }

                break;
            case DiagnosticStepOutput.InputOutputLogic.ShowMessageOnSelectedX:
                Debug.Log("Selected: " + dtOutput.selectedOutput + ", compare Against:" + dtOutput.selectors[0]);
                if (dtOutput.selectedOutput == dtOutput.selectors[0])
                    onDisplayText?.Invoke(dtOutput.displayText);
                break;
            default:
                break;
        }
        if (found)
        {
            //if (dtOutput.logic == DiagnosticStepOutput.InputOutputLogic.Direct)
            //{
            //    Debug.Log("Selected Output: " + dtOutput.selectedOutput);
            //    return dtOutput.outputs[dtOutput.selectedOutput].outputId;
            //}
            //else
            //{
            Debug.Log("OUTPUT TRUE: " + dtOutput.outputs[0].name);

            return dtOutput.outputs[0].outputId;
            //}
        }
        else
        {

            Debug.Log("OUTPUT False: " + dtOutput.outputs[1].name);
            return dtOutput.outputs[1].outputId;

        }
    }
    static string GetNameOfTop(DiagnosticStepOutput dtOp)
    {
        string resultStr = "";
        List<int> topValIds = new List<int>();
        List<DiagnosticStepOutput.NamedInput> namedIpCopy = new List<DiagnosticStepOutput.NamedInput>(dtOp.inputs);
        namedIpCopy.Sort((a, b) => b.val.CompareTo(a.val));// desc
        int topX = dtOp.selectors[0];
        for (int i = 0; i < topX; i++)
        {
            if (namedIpCopy[i] != null)
            {
                resultStr += namedIpCopy[i].name;
                if (i > 0)
                    resultStr += ", ";
            }
        }
        return resultStr;
    }
    static void SaveValue(int index, float val)
    {
        if (savedValues == null)
        {
            savedValues = new Dictionary<int, float>();
        }
        if (savedValues.ContainsKey(index))
            savedValues[index] = val;
        else
            savedValues.Add(index, val);
    }


}
