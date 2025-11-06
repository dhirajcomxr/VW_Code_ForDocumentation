using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTStepFactory : MonoBehaviour
{
  public static DiagnosticStep CreateDiagnosticStep(string inst,string quest,string curActId,string dtBlockId,string caution,string spec,string stage,DTAnswer[] answers)
    {
        DiagnosticStep result = new DiagnosticStep();
        result.instruction = inst;
        result.question = quest;
        result.activity_id = curActId;
        result.diagnostic_block_number = dtBlockId;
        result.caution_notes = caution;
        result.specs = spec;
        result.stage = stage;
        result.answers = answers;
        return result;
    }


}
