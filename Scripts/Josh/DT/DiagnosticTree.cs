using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DiagnosticTree 
{
    public string complaintName;
    public string author;
    public string createdOn, lastUpdated;
    public bool inDevelopment = false;
    public string addon_data;
    public List<DiagnosticStep> steps;
}
