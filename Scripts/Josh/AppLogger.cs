using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AppLogger : MonoBehaviour
{
    StepsTracker stepsTracker;
    static GameObject go;
    static AppLogger logger;
    [System.Serializable]
    public class AddonData
    {
      public  string question = "", answer = "";
        public AddonData(string q,string a)
        {
            this.question = q;
            this.answer = a;
           
        }
    }
    public static class EventType
    {
        public static string RNR = "rnr", DIAG = "diagnostic_tree", harness = "wiring_harness",
            process_dism= "dismantle", process_asm= "assembly"
            ;
    }

    public static UnityAction<KeyValuePair<string,string>[]> onLog,onProcessLog;

    public static void LogDtStep(string desc,string question,string answer)
    {
        AddonData addon = new AddonData(question, answer);
        KeyValuePair<string, string>[] keyVals = new KeyValuePair<string, string>[]
        {
            AppApiManager.UserDataKV.event_type.Value(EventType.DIAG),
            AppApiManager.UserDataKV.step_description.Value(desc),
            //AppApiManager.UserDataKV.question.Value(question),
            //AppApiManager.UserDataKV.answer.Value(answer)
            AppApiManager.UserDataKV.addon_data.Value(JsonUtility.ToJson(addon))
        };
        Debug.Log("Logging DT step: Addon: " + JsonUtility.ToJson(addon));
        ProcessLog(keyVals);
    }
    public static void LogDtStepFile(string desc,string fileIds)
    {
      
        KeyValuePair<string, string>[] keyVals = new KeyValuePair<string, string>[]
        {
            AppApiManager.UserDataKV.event_type.Value(EventType.DIAG),
                AppApiManager.UserDataKV.step_description.Value(desc),
            AppApiManager.UserDataKV.file_ids.Value(fileIds),       
        };
        ProcessLog(keyVals);
    }
    public static void LogEventDesc(string eventType,string desc)
    {
        KeyValuePair<string, string>[] keyVals = new KeyValuePair<string, string>[]
    {
            AppApiManager.UserDataKV.event_type.Value(eventType.ToString()),
            AppApiManager.UserDataKV.step_description.Value(desc)
    };
        Debug.Log("<color=white>[Logging]</color> " + "event: " + eventType + ", desc: "+ desc);
       // Mixpanel.Track("DT_Selected","dt_name",desc);
        ProcessLog(keyVals);
    }



    public static void LogEventProcDesc(string eventType,string process,string desc)
    {
        KeyValuePair<string, string>[] keyVals = new KeyValuePair<string, string>[]
 {
            AppApiManager.UserDataKV.event_type.Value(eventType),
            AppApiManager.UserDataKV.process_name.Value(process),
            AppApiManager.UserDataKV.step_description.Value(desc)
 };
        Debug.Log("<color=white>[Logging]</color> " +"event: "+eventType+", process:"+process);

        ProcessLog(keyVals);
    }



    public static void LogProcessType(string processType,string desc)
    {
         KeyValuePair<string, string>[] keyVals = new KeyValuePair<string, string>[]
       {
            AppApiManager.UserDataKV.event_type.Value(processType),
            AppApiManager.UserDataKV.step_description.Value(desc)
       };
        ProcessLog(keyVals);
    }
    public static void ProcessLog(KeyValuePair<string, string>[] keyValues)
    {
        onProcessLog?.Invoke(keyValues);
        Log(keyValues);
    }
    public static void Log(string eventName)
    {
       Log(new KeyValuePair<string, string>[] { AppApiManager.UserDataKV.event_type.Value(eventName) });
    }
    public static void Log(KeyValuePair<string,string> keyValue)
    {
       Log(new KeyValuePair<string, string>[] { keyValue });
    }
    public static void Log(KeyValuePair<string,string>[] keyValues)
    {
        onLog?.Invoke(keyValues);
    }
}
