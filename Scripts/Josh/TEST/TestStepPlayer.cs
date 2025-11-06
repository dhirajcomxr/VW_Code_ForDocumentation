using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class TestStepPlayer : MonoBehaviour
{
    [SerializeField] string longString = "";
    [SerializeField] bool doIt = false;
 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (doIt)
        {
            doIt = false;
            longString = ProcessStringForExport(longString);
        }
    }

    string ProcessStringForExport(string ip) => ip.Replace(",", ":;:").Replace(System.Environment.NewLine, ":::").Replace("\n",":::");
    string ProcessStringForImport(string ip) => ip.Replace(":;:", ",").Replace(":::", System.Environment.NewLine);
}
//[CustomEditor(typeof(TestStepPlayer))]
//public class TestStepPlayerEditor : Editor
//{

//    public SerializedProperty longStringProp;
//    void OnEnable()
//    {
//        longStringProp = serializedObject.FindProperty("longString");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        longStringProp.stringValue = EditorGUILayout.TextArea(longStringProp.stringValue, GUILayout.MaxHeight(75));
//        serializedObject.ApplyModifiedProperties();
//        DrawDefaultInspector();
//    }
//}

