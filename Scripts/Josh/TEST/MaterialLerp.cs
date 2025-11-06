using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
public class MaterialLerp : MonoBehaviour
{
  
    [SerializeField] Color col1, col2,resultCol;
    [SerializeField] float pulsePerS = 0.7f,pulseStat;
    [SerializeField] Material outlineMat;
    [SerializeField] Renderer thisRenderer;
    void Reset()
    {
        Debug.Log("Updating Outline Effect Color Config");
        thisRenderer = GetComponent<Renderer>();
      
        col1 = new Color(0, 0.55f, 1, 1);
        col2 = new Color(0, 0, 0, 0.25f);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!thisRenderer)
            thisRenderer = GetComponent<Renderer>();
        if(!outlineMat)
        outlineMat = GetComponent<Renderer>().material;

    }
    float stat;
    int test = 0;
    // Update is called once per frame
    void Update()
    {
        stat += Time.deltaTime * pulsePerS;
        
        pulseStat = Mathf.PingPong(stat, 1);
        resultCol = Color.Lerp(Color.white, Color.black,pulseStat);
        if (thisRenderer)
        {
          //  resultCol = Color.Lerp(col1, col2, Mathf.PingPong(Time.time * pulsePerS, 1));         
          //  outlineMat.color = (Color.Lerp(col1, col2, Mathf.PingPong(Time.time * pulsePerS, 1)));
            thisRenderer.material.SetColor("_Color1", resultCol);
        //    outlineCam.lineColor0 = (Color.Lerp(col1, col2, Mathf.PingPong(Time.time * pulsePerS, 1)));
        }
    }
}
