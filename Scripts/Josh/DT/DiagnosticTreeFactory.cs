using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DiagnosticTreeFactory : MonoBehaviour
{

    public static void SaveToFile(DiagnosticTree tree)
    {
        string fileName = tree.complaintName;
        if (tree.createdOn.Length < 1)
            tree.createdOn = System.DateTime.Now.ToShortDateString()+"-"+System.DateTime.Now.ToShortTimeString();
        tree.lastUpdated = System.DateTime.Now.ToShortDateString() + "-"+ System.DateTime.Now.ToShortTimeString();
        string jsonData = JsonUtility.ToJson(tree);
        string folderPath = Application.dataPath + Path.DirectorySeparatorChar + SAVE_FOLDER_NAME;
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        string dStepPath = folderPath + Path.DirectorySeparatorChar + fileName + ".txt";
        Debug.Log("Saving " + fileName + " to " + dStepPath.ToString());
        File.WriteAllText(dStepPath, jsonData);
    }
    public static DiagnosticTree LoadFromFile(TextAsset textAsset)
    {
        return LoadFromString(textAsset.text);
    }
    public static DiagnosticTree LoadFromString(string textAsset)
    {
      DiagnosticTree dt=  JsonUtility.FromJson<DiagnosticTree>(textAsset);
        if (dt != null)
            return dt;
        else
            return null;
    }
    public static string SAVE_FOLDER_NAME = "DT";
}
