using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class CSVSimple : MonoBehaviour
{
   // static string SPLIT_RE = @",";
    static string SPLIT_RE = @",";
 //   static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static string LINE_SPLIT_RE = @"\r\n|\n\r";
    static char[] TRIM_CHARS = { '\"' };
    public string fileName;
    public static string[] LoadData(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load(file) as TextAsset;
       // var lines = 
           return Regex.Split(data.text, System.Environment.NewLine);
        
      //  var lines = Regex.Split(data.text, LINE_SPLIT_RE);
    }


    // Start is called before the first frame update
    public string[] keys;
    void Start()
    {
        if (fileName != "")
        {
            Debug.Log("1:" + LoadData(fileName).Length);
            Debug.Log("2:"+Read(fileName).Count);
            var d = ReadV2(fileName);
            Debug.Log("ReadV2:" + d.Count);
       
             keys =d[0].Keys.ToArray();
            string headers = "";
           
            Debug.Log("ReadV2 Keys:" +keys[0].ToString());
            for (int i = 0; i < d.Count; i++)
            {
                string s = "";
                for (int j = 0; j < keys.Length; j++)
                {
                    s += d[i][keys[j]].ToString()+"|";
                  
                }
                Debug.Log("D:" + i + "::" + s);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static List<Dictionary<string, object>> Read(TextAsset data)
    {
        var list = new List<Dictionary<string, object>>();
      //  TextAsset data = Resources.Load(file) as TextAsset;

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
    //    var lines = Regex.Split(data.text, System.Environment.NewLine);
        // Debug.Log("L:" + lines.Length);
        if (lines.Length <= 1) return list;

        //    var header = Regex.Split(lines[0], SPLIT_RE);
        var header = Regex.Split(lines[0], ",");
        for (int i = 0; i < header.Length; i++)
        {
            // Debug.Log("H" + i + ":" + header[i]);
        }
        for (var i = 1; i < lines.Length; i++)
        {

            //  var values = Regex.Split(lines[i], SPLIT_RE);
            var values = Regex.Split(lines[i], ",");
            if (values.Length == 0 || values[0] == "") continue;

            // Debug.Log("V"+i+":" + values.Length);
            var entry = new Dictionary<string, object>();
            //for (var j = 0; j < header.Length && j < values.Length; j++)
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
    public static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load(file) as TextAsset;

    //    var lines = Regex.Split(data.text, LINE_SPLIT_RE);
        var lines = Regex.Split(data.text, System.Environment.NewLine);
       // Debug.Log("L:" + lines.Length);
        if (lines.Length <= 1) return list;

    //    var header = Regex.Split(lines[0], SPLIT_RE);
        var header = Regex.Split(lines[0], ",");
        for (int i = 0; i < header.Length; i++)
        {
           // Debug.Log("H" + i + ":" + header[i]);
        }
        for (var i = 1; i < lines.Length; i++)
        {

          //  var values = Regex.Split(lines[i], SPLIT_RE);
            var values = Regex.Split(lines[i], ",");
            if (values.Length == 0|| values[0] == "") continue;

           // Debug.Log("V"+i+":" + values.Length);
            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
          
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }               
                entry[header[j]] = finalvalue;
              
            }
            list.Add(entry);
        }
        return list;
    }
    public static List<Dictionary<string, object>> ReadV2(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load(file) as TextAsset;
        int startDataLine = -1,totalNAS=0;
            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
      //  var lines = Regex.Split(data.text, System.Environment.NewLine);
         Debug.Log("Total Rows:" + lines.Length);
        if (lines.Length <= 1) return list;
        if (Regex.IsMatch(lines[0].ToUpper().ToString(), "COMPLAINT"))
        {
            startDataLine = 1;
            Debug.Log("COMPLAINT SECTION FOUND!");
            var complaint = new Dictionary<string, object>();
            complaint["COMPLAINT"] = lines[0];
            list.Add(complaint);
        }
        else
            startDataLine = 0;
        
        var header = Regex.Split(lines[1], SPLIT_RE);
        //    var header = Regex.Split(lines[0], ",");
        for (int i = 0; i < header.Length; i++)
        {
            // Debug.Log("H" + i + ":" + header[i]);
        }
        for (var i = startDataLine; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);       
            if (values.Length == 0 || values[0] == "") continue;

            
            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)

            {
                string value = values[j];
         //       Debug.Log("V:" + i + " : " + value.ToString());
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                // value.Replace(":,:", ",");
                if (value.ToString().ToUpper() == "N/A" || value.ToString().ToUpper() == "NA")
                {
                    totalNAS++;
                    value = "";
                }
                value = value.ToString().Replace(":;:",",").Replace(":::", System.Environment.NewLine);
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        if(totalNAS>0)
        Debug.Log("Removed " + totalNAS + " N/A elements");
        return list;
    }
}
