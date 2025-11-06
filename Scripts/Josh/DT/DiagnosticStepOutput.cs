using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DiagnosticStepOutput 
{
    
    public int selectedOutput;
    public float[] vars;
    public int[] selectors;
 
//    public KeyValuePair<string, string> io;
    public string displayText;
    public Method method;
    public InputOutputLogic logic;

    public NamedStringValue textInput;
    public NamedInput[] inputs;
    public NamedOutput[] outputs;

    #region Types
    [System.Serializable]
    public class NamedInput
    {
        public string name;
        public float val;
       public NamedInput(string _name,float _val)
        {
            this.name = _name;
            this.val = _val;
        }
        public string GetAsString()
        {
            return name + ":" + val.ToString();
        }
    }
    [System.Serializable]
    public class NamedStringValue
    {
        public string name;
        public string val;
        public NamedStringValue(string _name, string _val)
        {
            this.name = _name;
            this.val = _val;
        }
        public string GetAsString()
        {
            return name + ":" + val.ToString();
        }
    }
    [System.Serializable]
    public class NamedOutput
    {
        public string name;
        public string outputId;
        public NamedOutput(string _name,string id)
        {
            this.name = _name;
            this.outputId = id;
        }
    }
    public enum Method {
        Next,YesNo,DropDown,InputValues,Table,InputText
    }
    public enum InputOutputLogic
    {
        Direct, AnyInputInRange,
        AtleastXInputsInRange, InputALessThanXTimesB,
        InputAGreaterThanXTimesB,SaveValTo,InputALessThanXTimesSavedStepInput,DisplayTopXVals,ShowMessageOnSelectedX
    }
    #endregion
}
