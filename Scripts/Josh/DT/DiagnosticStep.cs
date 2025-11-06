using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class DiagnosticStep
{
 
    public string instruction, activity_id, diagnostic_block_number, part_number,
        on_yes, on_no, question, activity, scan, caution_notes, reference, image,
        specs, stage, method;
    [SerializeField] public bool isDialog;
    [SerializeField] public DiagnosticStepOutput.NamedStringValue dialogData;

    public string[] imgs = new string[5], buttons = new string[10],
        buttonNames = new string[10];
    [SerializeField] public DiagnosticStepOutput output;
   [SerializeField] public DTAnswer[] answers;

    [SerializeField] public ScanOption scanOption;
    public enum ScanOption
    {
        Image, Video, Audio
    }
}
