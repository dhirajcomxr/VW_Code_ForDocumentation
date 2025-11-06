
[System.Serializable]
public class DTAnswer 
{  
        public string stepId;
    public Method method;
    public enum Method
    {
        AnyInputGreaterThan,AnyInputLesserThan,InputEquals,AtleastYInputsLesserThan,YInputsGreaterThanX
    }
    public NamedFloat[] vals, inputs;
    
    [System.Serializable]
    public class NamedFloat
    {
        public string name;
        public float val;
    
    }
}
