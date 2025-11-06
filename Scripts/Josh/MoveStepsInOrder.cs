using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class MoveStepsInOrder : MonoBehaviour
{
    public bool editor = true;
    [SerializeField] public Steps main;

    [SerializeField] public int stepsToMoveFrom, stepsToMoveTo;
   
    [SerializeField] public int addToIndex;
    public string eMsg;
    public bool showInfo;

    [Header("For Locking Steps")]
    [SerializeField] public int stepsToLockFrom, stepsToLockTo;
    void Awake()
    {

        if (main == null)
            main = FindObjectOfType<Steps>();
    }
    // Start is called before the first frame update
    void Start()
    {
        GetStepsInfoDism();
        GetStepsInfoAssm();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    //[ContextMenu("Set reverse step values from dism to assm")]
    //public void DismToAssmReverseCopy()
    //{
    //    List<Step> result = new List<Step>();
    //    List<Step> dism_stl = main.steps;
    //    List<Step> assm_stl = main.assemblySteps;
    //    for (int i = 0; i < assm_stl.Count; i++)
    //    {
    //        result.Add(assm_stl[i]);
    //    }
    //    for (int i = dism_stl.Count - 1; i >= 0; i--)
    //    {
    //        result[dism_stl.Count - 1 - i].animatedObject = dism_stl[i].animatedObject;
    //        result[dism_stl.Count - 1 - i].lookAtPoint = dism_stl[i].lookAtPoint;
    //        result[dism_stl.Count - 1 - i].overrideCameraPosition = dism_stl[i].overrideCameraPosition;
    //        result[dism_stl.Count - 1 - i].isLocked = dism_stl[i].isLocked;
    //        result[dism_stl.Count - 1 - i].objsToHighlight = dism_stl[i].objsToHighlight;
    //        result[dism_stl.Count - 1 - i].objsToDisable = dism_stl[i].objsToDisable;
    //        result[dism_stl.Count - 1 - i].objsToEnable = dism_stl[i].objsToEnable;
    //        Debug.Log("from"+dism_stl[i]+" Result" + result[dism_stl.Count - 1 - i]);

    //    }
    //    main.assemblySteps = result;


    //}


    //[ContextMenu("Lock dismental steps ")]
    //public void LockSteps_Dism()
    //{
    //    List<Step> result = new List<Step>();
    //    List<Step> stl = main.steps;
    //    int count = stepsToLockTo - stepsToLockFrom + 1;
    //    List<Step> stprange = stl.GetRange(stepsToLockFrom, count);
    //    for (int i = 0; i < stl.Count; i++)
    //    {

    //        stl[i].isLocked = true;
    //        result.Add(stl[i]);
    //    }
    //    Debug.Log("Steps:" + result.Count);


    //    Debug.Log("from index" + stepsToLockFrom + " count" + count +  " steps" + result.Count);
    //    Debug.Log("Steps:");

    //    foreach (Step s in result)
    //    {
    //        Debug.Log(s.debugStepNum + "locate: " + s.locateObjectText);
    //    }
    //    main.steps = result;
    //}

    [ContextMenu("Copy dismental toolnames from sprites")]
    public void MoveSteps_Dism()
    {
        List<Step> result=new List<Step>() ;
        List<Step> stl = main.steps;
        int count = stepsToMoveTo - stepsToMoveFrom+1;
        List<Step> stprange = stl.GetRange(stepsToMoveFrom, count);
       for(int i=0; i<stl.Count;i++)
        {
            result.Add(stl[i]);
        }
        Debug.Log("Steps:"+result.Count);
        eMsg = "Moved From:" + stepsToMoveFrom + "-" + result[stepsToMoveFrom].locateObjectText + "\n To:" + stepsToMoveTo + "-" + result[stepsToMoveTo].locateObjectText + "\n added to:" + addToIndex + "-" + result[addToIndex].locateObjectText;
        result.RemoveRange(stepsToMoveFrom, count);
        Debug.Log("result stemto move from" + result[stepsToMoveFrom].locateObjectText);
        result.InsertRange(addToIndex-count+1, stprange);

        Debug.Log("from index"+stepsToMoveFrom+" count" + count + " add to index"+addToIndex+" steps"+result.Count);
        Debug.Log("Steps:");

        foreach (Step s in result)
        {
            Debug.Log(s.debugStepNum+"locate: " + s.locateObjectText);
        }
        main.steps = result;
    }
    public void GetStepsInfoDism()
    {
        List<Step> result = new List<Step>();
        List<Step> stl = main.steps;
        int count = stepsToMoveTo - stepsToMoveFrom + 1;
        for (int i = 0; i < stl.Count; i++)
        {
            result.Add(stl[i]);
        }
        Debug.Log("Steps:" + result.Count);
        eMsg = "From:" + stepsToMoveFrom + "-" + result[stepsToMoveFrom].locateObjectText + "\n To:" + stepsToMoveTo + "-" + result[stepsToMoveTo].locateObjectText + "\n added to:" + addToIndex + "-" + result[addToIndex].locateObjectText;
    }

    [ContextMenu("Copy assembly toolnames from sprites")]
    public void MoveSteps_Assm()
    {
        List<Step> result = new List<Step>();
        List<Step> stl = main.assemblySteps;
        int count = stepsToMoveTo - stepsToMoveFrom + 1;
        List<Step> stprange = stl.GetRange(stepsToMoveFrom, count);
        for (int i = 0; i < stl.Count; i++)
        {
            result.Add(stl[i]);
        }
        Debug.Log("Steps:" + result.Count);
        eMsg = "Moved From:" + stepsToMoveFrom + "-" + result[stepsToMoveFrom].locateObjectText + "\n To:" + stepsToMoveTo + "-" + result[stepsToMoveTo].locateObjectText + "\n added to:" + addToIndex + "-" + result[addToIndex].locateObjectText;
        result.RemoveRange(stepsToMoveFrom, count);
        result.InsertRange(addToIndex-count+1, stprange);

        Debug.Log("from index" + stepsToMoveFrom + " count" + count + " add to index" + addToIndex + " steps" + result.Count);
        Debug.Log("Steps:");

        foreach (Step s in result)
        {
            Debug.Log(s.debugStepNum + "locate: " + s.locateObjectText);
        }
        main.assemblySteps = result;
    }
    public void GetStepsInfoAssm()
    {
        List<Step> result = new List<Step>();
        List<Step> stl = main.assemblySteps;
        int count = stepsToMoveTo - stepsToMoveFrom + 1;
        for (int i = 0; i < stl.Count; i++)
        {
            result.Add(stl[i]);
        }
        Debug.Log("Steps:" + result.Count);
        eMsg = "From:" + stepsToMoveFrom + "-" + result[stepsToMoveFrom].locateObjectText + "\n To:" + stepsToMoveTo + "-" + result[stepsToMoveTo].locateObjectText + "\n added to:" + addToIndex + "-" + result[addToIndex].locateObjectText;
    }


}


#if UNITY_EDITOR
[CustomEditor(typeof(MoveStepsInOrder))]
[CanEditMultipleObjects]
public class MoveStepsInOrderEditor : Editor
{
    public override void OnInspectorGUI()
    {


        MoveStepsInOrder stepsCtrl = (MoveStepsInOrder)target;
        stepsCtrl.editor = EditorGUILayout.ToggleLeft(new GUIContent("Editor Mode"), stepsCtrl.editor);
        if (!stepsCtrl.editor)
            DrawDefaultInspector();
        else
        {
            if (stepsCtrl)
            {
                var mainSteps = serializedObject.FindProperty("main");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("main"), true);
                EditorGUILayout.BeginVertical();
                if (stepsCtrl.eMsg != null)
                    if (stepsCtrl.eMsg.Length > 0)
                        EditorGUILayout.HelpBox(stepsCtrl.eMsg, MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                if (stepsCtrl)
                    if (GUILayout.Button("Refresh Details"))
                    {
                        stepsCtrl.GetStepsInfoDism();
                        stepsCtrl.GetStepsInfoAssm();
                    }
                        

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stepsToMoveFrom"), true);
               
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stepsToMoveTo"), true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addToIndex"), true);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (stepsCtrl)
                    if (GUILayout.Button("Move Dismental steps"))
                        stepsCtrl.MoveSteps_Dism();
                if (stepsCtrl)
                    if (GUILayout.Button("Move Assembly Steps"))
                        stepsCtrl.MoveSteps_Assm();
                EditorGUILayout.EndHorizontal();




                //EditorGUILayout.Space();
                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("stepsToLockFrom"), true);

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("stepsToLockTo"), true);
                //EditorGUILayout.EndHorizontal();
                //EditorGUILayout.BeginHorizontal();
                //if (stepsCtrl)
                //    if (GUILayout.Button("LockStepsDism"))
                //        stepsCtrl.LockSteps_Dism();
                //if (stepsCtrl)
                //    if (GUILayout.Button("UnlockStepsDism"))
                //        stepsCtrl.MoveSteps_Assm();
                //EditorGUILayout.EndHorizontal();
                //EditorGUILayout.BeginHorizontal();
                //if (stepsCtrl)
                //    if (GUILayout.Button("LockStepsAsm"))
                //        stepsCtrl.MoveSteps_Dism();
                //if (stepsCtrl)
                //    if (GUILayout.Button("UnlockStepsAsm"))
                //        stepsCtrl.MoveSteps_Assm();
                //EditorGUILayout.EndHorizontal();


                EditorGUILayout.EndVertical();

                //            //   EditorGUILayout.PropertyField(stepList,true);
                serializedObject.ApplyModifiedProperties();
            }
            else
                DrawDefaultInspector();
        }
    }
}
#endif
