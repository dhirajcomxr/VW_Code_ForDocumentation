using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
public class OutlineFillColorPulse : MonoBehaviour
{
   [SerializeField] OutlineEffect outlineCam;
    [SerializeField] Color col1, col2;
    [SerializeField] float pulsePerS = 2f, pipi;
    [SerializeField] float t;
    void Reset()
    {
        Debug.Log("Outline camear find");
        //outlineCam = Camera.main.GetComponent<OutlineEffect>();
        outlineCam = GetComponent<OutlineEffect>();
        if (outlineCam == null)
        {
           // outlineCam = Camera.main.GetComponent<OutlineEffect>();
            outlineCam = FindObjectOfType<OutlineEffect>();
           
        }
          
        col1 = Color.red;
        col2 = Color.black;
    }
    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        if (outlineCam)
        {
            outlineCam.fillColor = (Color.Lerp(col1, col2, Mathf.PingPong(Time.time * pulsePerS, 1)));
            outlineCam.lineColor0= (Color.Lerp(col1, col2, Mathf.PingPong(Time.time * pulsePerS, 1)));
        }
    }
}
