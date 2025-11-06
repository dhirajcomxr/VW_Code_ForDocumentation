using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ExplosionRecorder : MonoBehaviour
{
    [HideInInspector] public List<Vector3>  startPts,endPts;
    [HideInInspector] public string msg="",errorMsg="";
    [HideInInspector] public bool isDirty;
    [HideInInspector] public GameObject g;
    [Range(1, 5)]
    public int autoExplodeFactor = 2;
    private void Reset()
    {
        if (isDirty)
        {
            PlayStartPoints();
        }
        startPts = endPts = new List<Vector3>();
        RecordStartPoints();
    }
    
    
    public void AutoExplode()
    {
        bool can = true;
        if (endPts != null)
            if (endPts.Count > 1)
                can = false;
        if(can)
        {
            int total = transform.childCount;
            for (int i = 0; i < total; i++)
            {
                endPts.Add(CalcExplodePosition(transform.GetChild(i).gameObject, transform.gameObject, true));
            }
        }

    }
    //private void OnDestroy()
    //{
    //    PlayPoints(startPts);
    //}
    public void RecordStartPoints() => startPts = RecordPoints();
    public void RecordEndPoints() => endPts = RecordPoints();

    public void PlayStartPoints() => PlayPoints(startPts);
    public void PlayEndPoints()
    {
        isDirty = true;
        PlayPoints(endPts);
    }

    private List<Vector3> RecordPoints()
    {
        msg = "";
        int total = transform.childCount;
        List<Vector3> points= new List<Vector3>();
        for (int i = 0; i < total; i++)
        {
            points.Add(transform.GetChild(i).position);
        }
        msg = "Recorded " + total + " Objects";
        return points;
    }
    private void PlayPoints(List<Vector3> points)
    {
        int total = points.Count;
        if (transform.childCount != total)
            errorMsg = "Recorded points do not match number of children for this object,\n please click record again";
        else
        {
            for (int i = 0; i < total; i++)
            {
                transform.GetChild(i).position = points[i];
            }
            msg = "Restored positions for " + total + " objects";
        }

    }
    
   private Vector3 GetPartCenter(GameObject part)
    {
        Bounds b = new Bounds();
        foreach (MeshRenderer mr in part.GetComponentsInChildren<MeshRenderer>(true)) { b.Encapsulate(mr.bounds); }
        return b.center;
    }
    private Vector3 CalcExplodePosition(GameObject part, GameObject root, bool calcFromCenter)
    {
        Vector3 partPosition = part.transform.position;
        Vector3 rootPosition = root.transform.position;
        if (calcFromCenter) { partPosition = GetPartCenter(part); rootPosition = GetPartCenter(root); }
        Vector3 explodePos = partPosition + ((partPosition - rootPosition) * autoExplodeFactor);
        return explodePos;
    }
    
}
#if UNITY_EDITOR
[CustomEditor(typeof(ExplosionRecorder))]
[CanEditMultipleObjects]
public class ExplosionRecoderEditor : Editor
{
    [MenuItem("Tools/Add ExplosionRecoder")]
    public static void AddWithAutoExplode()
    {
        foreach (var item in Selection.gameObjects)
        {
            if (item.GetComponent<ExplosionRecorder>())
                Debug.LogError("Explosion Recorder already present!", item);
            else
            {
                var exp = item.AddComponent<ExplosionRecorder>();
                exp.RecordStartPoints();
                exp.AutoExplode();
            }
        }
    }
    List<Vector3> st, ed;
    ExplosionRecorder myTarget;

    public override void OnInspectorGUI()
    {
       
        var myTargets = targets;
        
        DrawDefaultInspector();
        int total = myTargets.Length;
        if (total > 1)
        {


            if (GUILayout.Button("Go to Start State "))
                foreach (ExplosionRecorder item in targets)
                    item.PlayStartPoints();

            if (GUILayout.Button("Go to End State"))
                foreach (ExplosionRecorder item in targets)
                    item.PlayEndPoints();
          

        }
            for (int i = 0; i < total; i++)
        {
          
            ExplosionRecorder myTarget = (ExplosionRecorder)myTargets[i];
           
            if (total > 1)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
            myTarget.isDirty = EditorGUILayout.Foldout(myTarget.isDirty, "For: "+myTarget.startPts.Count, true);
               
                myTarget.g = (GameObject)EditorGUILayout.ObjectField("", myTarget.gameObject, typeof(GameObject), true);
               
                EditorGUILayout.EndHorizontal();
               
            }
            else myTarget.isDirty = true;
           
            if (myTarget.isDirty)
            {
        
                EditorGUILayout.BeginVertical();
 
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Record Start"))
                {
                    myTarget.RecordStartPoints();
                    st = myTarget.startPts;
                }
                if (GUILayout.Button("Record End"))
                {
                    myTarget.RecordEndPoints();
                    ed = myTarget.endPts;
                }
                EditorGUILayout.EndHorizontal();


                if (myTarget.msg.Length > 0)
                    EditorGUILayout.HelpBox(myTarget.msg, MessageType.Info);
                if (myTarget.errorMsg.Length > 0)
                    EditorGUILayout.HelpBox(myTarget.msg, MessageType.Error);
                EditorGUILayout.BeginHorizontal();
                if (myTarget.startPts.Count > 0)
                    if (GUILayout.Button("Go To Start State"))
                        myTarget.PlayStartPoints();

                if (myTarget.endPts.Count > 0)
                {
                    if (GUILayout.Button("Go To End State"))
                        myTarget.PlayEndPoints();
                }
                else
                      if (GUILayout.Button("Auto Explode"))
                {
                    myTarget.AutoExplode();
                    ed = myTarget.endPts;
                }

                EditorGUILayout.EndHorizontal();
                if (total > 1)
                {
                    ;
                }
                EditorGUILayout.EndVertical();

            }
       
        }
      
    } 
 
}
#endif
