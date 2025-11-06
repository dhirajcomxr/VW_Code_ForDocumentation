using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PartSequenceEximProcessor : MonoBehaviour
{
    [HideInInspector]
   public string[] infoMsg = {"Assign Object with the Steps Component to main",
        "Assign Part Sequence CSV file to partSequenceFile" }; 
    [SerializeField]public Steps main;
    [SerializeField] TextAsset partSequenceFile;
    private List<Dictionary<string, object>> data;
    private int totalSteps;
    [SerializeField] List<PartSequence> sequence;
    [SerializeField] List<Step> curAssm, curDism;

   // [HideInInspector]
    [SerializeField]
    List<Step> mainAssembly, mainDismantling;
   [HideInInspector] public int curr=0;
    [System.Serializable]
   public class PartSequence
    {
        public string name, number, assembly, dismantling;
    }
    [System.Serializable]
    class HeaderNames
    {
        public string name="SECTION", number="NUMBER", assembly="ASSEMBLY STEP", dismantling="DISMANTLING STEP";
    }
    public int GetSavedMainDismCount() => mainDismantling.Count;
    public int GetSavedMainAsmCount() => mainAssembly.Count;
    public int GetCurrAsmCount() => curAssm.Count;
    public int GetCurrDismCount() => curDism.Count;
    public bool IsCurDismEqualToSavedMainDism()
    {
        return curDism.Count == mainDismantling.Count;
    }
    public bool IsCurAsmEqualToSavedMainAsm()
    {
        return curAssm.Count == mainAssembly.Count;
    }
    public void Clear()
    {
        curDism = new List<Step>();
        curAssm = new List<Step>();
        mainAssembly = new List<Step>();
        mainDismantling = new List<Step>();
        sequence = new List<PartSequence>();
        data = null;
    }
    int SequenceIdFromName(string pName)
    {
        Debug.Log("Searching for :" + pName);
        int result = -1;
        List<int> possible = new List<int>();
        string ipName = pName.ToUpper().Replace(" ", "");
        Debug.Log("Searching for :" + ipName +" in "+sequence.Count+" rows");
        for (int i = 0; i < sequence.Count; i++)
        {
            string name = sequence[i].name.ToUpper().Replace(" ","");
        
            if (name.Length > 0)
                if (name.Contains(ipName))
                {
                    if(name==ipName)
                    {
                        curr = result = i;
                        Debug.Log("Found sequence " + i);
                        return result;
                    }
                    else
                    possible.Add(i);                      
                }
        }
        if (possible.Count > 0)
        {
            Debug.Log(possible.Count + "number of result/s found");
            result = GetHighlyProbableStep(possible, pName);
        }
       
        Debug.Log("Result for " + ipName+" is "+result);
        return result;
    }
    public int GetSequenceIdFor(string pName)
    {
        int result = -1;
        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].name == pName)
                result = i;
        }
        return result;

    }
    public void PlaySequenceFor(string pName)
        => PlayStepSequence(GetSequenceIdFor(pName));
    int GetHighlyProbableStep(List<int> possibility,string ip)
    {
        int smallestIndex = possibility[0];
       
        int Cmp(int index) => sequence[possibility[index]].name.ToUpper().Replace(" ", "").Replace(ip, "").Length;
        if (possibility.Count>0)
        {
            int smallest = Cmp(0);
            for (int i = 0; i < possibility.Count; i++)
            {
                int curr = Cmp(i);
                if (smallest > curr)
                {
                    smallest = curr;
                    smallestIndex = possibility[i];
                }
            }  
        }
        return smallestIndex;
    }
    public void StepSequenceFromName(string pName)
    {
        int seqId = SequenceIdFromName(pName);
        if (seqId >= 0)
            PlayStepSequence(seqId);
        else
            Debug.Log("No sequence found for Part Name:" + pName);
    }
    void Reset()
    {
        if (!main)
            main = FindObjectOfType<Steps>();
    }
    HeaderNames refHeaders, headers;
    // Start is called before the first frame update
    void Start()
    {
     //   LoadSequenceToSteps();
    }
   
    // Update is called once per frame
   
    public void LoadSequence() => LoadFromCSV(partSequenceFile);
    void LoadSequenceToSteps()
    {
        Debug.Log("Start new Load");
        if (main)
        {
            mainAssembly = main.assemblySteps;
            mainDismantling = main.steps;
        }
        Debug.Log("Start new Load: ASM: "+mainAssembly.Count+" DISM:"+mainDismantling.Count);
    }
    public void LoadStepsFrom(Steps stepRnr)
    {
        main = stepRnr;
        if (main)
        {
            
            mainAssembly = main.assemblySteps;
            mainDismantling = main.steps;
        }
    }
    public void PlayStepSequence(int c)
    {
        Debug.Log("Playing: " + c);
   //     Debug.Log("STEP SEQ: "+c+"| ASS ST: "+mainAssembly[0].animatedObject,curAssm[0].animatedObject);
        if(sequence.Count<1)
        {
            Debug.Log("COUNT LOW!");
        }
        if (c < sequence.Count)
        {
            curr = c;
        }
        else
            curr = 0;
        PlaySequence(sequence[curr]);

    }
    public void ResetSequence()
    {
        if (mainAssembly.Count < 1 || mainDismantling.Count < 1)
            LoadSequenceToSteps();
        curAssm = mainAssembly;
        curDism = mainDismantling;
        if (Application.isPlaying)
        {
            Debug.Log("ASM:A " + mainAssembly[0].animatedObject, mainAssembly[0].animatedObject);
        //    main.assemblySteps = curAssm;
        //    main.steps = curDism;
        }
    }
    public void SetSelectionToSteps()
    {
        if (main)
        {
            if (curAssm.Count > 0)
                main.assemblySteps = curAssm;
            if (curDism.Count > 0)
                main.steps = curDism;
        }
        else
            Debug.LogError("Cannot Find Main Steps Object!");
    }
    public void PlaySequence(PartSequence ps)
    {
        //   Debug.Log("REACHED");
        if (mainAssembly.Count < 1 || mainDismantling.Count < 1)
            LoadSequenceToSteps();

          curAssm = curDism = new List<Step>();

        // curAssm = new List<Step>();
        // curDism = new List<Step>();
        if (ps.assembly != "")
        {
            //    Debug.Log("ASSM");
            Debug.Log("ASM:1 " + mainAssembly[0].animatedObject, mainAssembly[0].animatedObject);
              curAssm = GetSteps(ps.assembly, mainAssembly);
         //   curAssm = mainAssembly;

        }
        if (ps.dismantling != "")
        {
            //    Debug.Log("DISM");
             curDism = GetSteps(ps.dismantling, mainDismantling);
          //  curDism = mainAssembly;

        }

        if (Application.isPlaying)
        {
            Debug.Log("ASM:A " + mainAssembly[0].animatedObject, mainAssembly[0].animatedObject);
           main.assemblySteps = curAssm;
            main.steps = curDism;
        }
    }
    List<Step> GetSteps(string stepseq,List<Step> stepInput)
    {
        Debug.Log("Step seq: " + stepseq);
        string[] st = stepseq.Replace(":;:",",").Split(',');
        Debug.Log("ROWS FOR INPUT: " + st.Length + " AND DATA: " + stepInput.Count);
     //   Debug.Log("STL:"+st.Length+"  SI:"+stepInput.Count+" txt:"+stepseq.Replace(":;:",","));
        List<Step> stepList = new List<Step>();
        for (int i = 0; i < st.Length; i++)
        {
            int stepNum = -1;
            if (int.TryParse(st[i], out stepNum))
            {
                    Debug.Log("STEP NUM: " + stepNum);
                if (stepNum < stepInput.Count)
                    stepList.Add(stepInput[stepNum]);
                else
                    Debug.Log("STEPNUM: " + stepNum);
            }
            //if (int.TryParse(st[i], out stepNum))
            //{
            //    //   Debug.Log("STEPNUM:" + stepNum);
            //    if (stepInput[stepNum] == null)
            //        Debug.Log("<color=red> STEP:  </color>" + stepNum);
            //    Step stp = stepInput[stepNum];
            //    if (stp.animatedObject == null)
            //        Debug.Log("NULL ANIM FOR" + stepNum);
            //    else
            //        Debug.Log("HAS ANIM FOR" + stepNum);
            //    stepList.Add(stp);               
            //}           
        }
        if (stepList.Count < 1)
        {
            if (stepseq.ToUpper().Contains("COMPLETE"))
            {
                stepList = stepInput;
                Debug.Log("Adding Complete Process..");
            }
            else
            Debug.LogError("NO STEPS found in " + stepseq);
        }
        Debug.Log("BEFORE STEPLIST SUBM: "+stepList.Count);
      //  Debug.Log("SL:" + (stepList==null));      
        return stepList;
    }
    public void LoadData(TextAsset seqFile)
           => LoadFromCSV(seqFile);
    void LoadFromCSV(TextAsset loadFileAsset)
    {
        Debug.Log("Loading Sequence from " + loadFileAsset.name);
        partSequenceFile = loadFileAsset;
        data = CSVSimple.Read(loadFileAsset);
        totalSteps = data.Count;
      
        InitialiseData();
        Debug.Log(totalSteps + " records loaded from " + loadFileAsset.name);

    }
    public void Next() => PlayStepSequence(++curr);
    public bool hasMain => main;
    public bool hasFile => partSequenceFile;
    public List<PartSequence> GetFullSequence() => sequence;
    public List<PartSequence> GetResultsFor(string search)
    {
        Debug.Log("<color=blue>Results</color>");
        return sequence.FindAll(
          delegate (PartSequence pt)
          {
              return pt.name.Replace(" ", "").ToUpper().Contains(search.Replace(" ", "").ToUpper());
          }
          );
    }
    private void InitialiseData()
    {
        UpdateHeaders();
        sequence = new List<PartSequence>();
        for (int i = 0; i < data.Count; i++)
        {
            PartSequence part = new PartSequence();
            part.name = data[i][headers.name].ToString();
            if(data[i].ContainsKey(headers.number))
            part.number = data[i][headers.number].ToString();
            part.assembly = data[i][headers.assembly].ToString();
            if(part.assembly=="")
            {
                part.assembly = "COMPLETE";
                Debug.Log("Updating Assembly of " + part.name + " to include complete Assembly Process");
            }
            part.dismantling = data[i][headers.dismantling].ToString();
            if (part.dismantling == "")
            {
                part.dismantling = "COMPLETE";
                Debug.Log("Updating Dismantling of " + part.name + " to include complete Dismantling Process");
            }
            sequence.Add(part);
        }
    }
    private void UpdateHeaders()
    {
        refHeaders = headers = new HeaderNames();
        string[] fileHeaders = new string[data[0].Keys.Count];
        data[0].Keys.CopyTo(fileHeaders, 0);
        Debug.Log("DATA:" + data.Count + " | FH:" + fileHeaders.Length + " | keys" + data[0].Keys.Count);

        for (int i = 0; i < fileHeaders.Length; i++)
        {
         //   Debug.Log(+i+" "+fileHeaders[i]);
            headers.name = fileHeaders[i].ToString().ToUpper().Contains(refHeaders.name) ? fileHeaders[i] : headers.name;
            headers.number = fileHeaders[i].ToString().ToUpper().Contains(refHeaders.number) ? fileHeaders[i] : headers.number;
            headers.assembly = fileHeaders[i].ToString().ToUpper().Contains(refHeaders.assembly) ? fileHeaders[i] : headers.assembly;
            headers.dismantling = fileHeaders[i].ToString().ToUpper().Contains(headers.dismantling) ? fileHeaders[i] : headers.dismantling;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PartSequenceEximProcessor))]
public class PartSequenceEximProcessorEditor : Editor
{
    public override void OnInspectorGUI()
    {  
        PartSequenceEximProcessor script = (PartSequenceEximProcessor)target;
        string msg = "";
        if (!script.hasMain)
            msg += script.infoMsg[0]+ System.Environment.NewLine ;
        if (!script.hasFile)
            msg +=  script.infoMsg[1];
        if (msg != "")
            EditorGUILayout.HelpBox(msg, MessageType.Info, true);
        if(script.hasFile)
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Load Sequence", GUILayout.MinHeight(25)))
                script.LoadSequence();
            EditorGUILayout.BeginHorizontal();

            script.curr = EditorGUILayout.IntField(script.curr,GUILayout.MaxWidth(40));
            if (GUILayout.Button("Go",GUILayout.MaxWidth(60)))
                script.PlayStepSequence(script.curr);
            if (GUILayout.Button("Next"))
                script.Next();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
       
        DrawDefaultInspector();
    }
}
#endif
