using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTAnswerEvaluator : MonoBehaviour
{
    public static string Evaluate(DiagnosticStep step)
    {
        string result = null;

        for (int i = 0; i < step.answers.Length; i++)
        {
            string id = EvaluateAnswer(step.answers[i]);
            if (id != null)
                result = id;
        }
      
        return result;
    }
   static string EvaluateAnswer(DTAnswer answer)
    {
        DTAnswer.Method method = answer.method;
        string nextStepId = answer.stepId;
        bool result = false;
        string evalResult = null;
        DTAnswer.NamedFloat val0 = answer.vals[0];
        DTAnswer.NamedFloat val1 = answer.vals[1];
        switch (method)
        {
            case DTAnswer.Method.AnyInputGreaterThan:
               
                for (int i = 0; i < answer.inputs.Length; i++)
                {
                    if (answer.inputs[i].val > val0.val)
                    {
                        result = true;
                    }
                }
                break;
            case DTAnswer.Method.AnyInputLesserThan:
              
                for (int i = 0; i < answer.inputs.Length; i++)
                {
                    if (answer.inputs[i].val < val0.val)
                    {
                        result = true;
                    }
                }
                break;
            case DTAnswer.Method.InputEquals:
                for (int i = 0; i < answer.inputs.Length; i++)
                {
                    if (answer.inputs[i].val == val0.val)
                    {
                        result = true;
                    }
                }
                break;
            case DTAnswer.Method.AtleastYInputsLesserThan:
                int times = 0;
                for (int i = 0; i < answer.inputs.Length; i++)
                {
                    if (answer.inputs[i].val < val0.val)
                    {
                        times++;
                    }
                }
                if (times >= val1.val)
                    result = true;
                break;
            default:
                break;
        }
        if (result)
            return answer.stepId;
        else
            return null;
        
    }
}
