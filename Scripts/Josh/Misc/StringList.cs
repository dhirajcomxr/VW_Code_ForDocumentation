using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "Data", menuName = "Objects/String List ", order = 1)]
[System.Serializable]
public class StringList : ScriptableObject
{
    
    public List<string> list;
}
#if UNITY_EDITOR
[CustomEditor(typeof(StringList))]
[CanEditMultipleObjects]
public class StringListEditor : Editor
{
    int sel;
    string jsonData;
    public override void OnInspectorGUI()
    {
        StringList script = (StringList)target;
        DrawDefaultInspector();
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Update References after changes", MessageType.None);

        jsonData = EditorGUILayout.TextArea(jsonData);
        if (GUILayout.Button("Export To Json"))
        {
             jsonData = JsonUtility.ToJson(script);
        }
        //if (GUILayout.Button("Import from Json"))
        //{
        //    script.list = JsonUtility.FromJson<StringList>(jsonData).list;
        //}
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.EndHorizontal();
    }
}
#endif
