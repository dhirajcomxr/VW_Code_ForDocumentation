using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatLerp : MonoBehaviour
{

    public Color colA1, colA2, colB1, colB2;
        private Color resultColA,resultColB;
    public float pulsePerS = 0.7f;
        private float pulseStat;
     Material outlineMat;

    [SerializeField] Renderer thisRenderer;
    void Reset()
    {
      
        thisRenderer = GetComponent<Renderer>();

        colA1 = new Color(0, 0.55f, 1, 1);
        colB1 = new Color(0, 0, 0, 0.25f);

        colA2 = new Color(86, 134, 135, 255);
        colB2 = new Color(86, 134, 135, 100);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!thisRenderer)
            thisRenderer = GetComponent<Renderer>();
        if (!outlineMat)
            outlineMat = GetComponent<Renderer>().material;

        

    }
    float stat;
    //int test = 0;
    // Update is called once per frame
    void Update()
    {
        stat += Time.deltaTime * pulsePerS;
        pulseStat = Mathf.PingPong(stat, 1);
        resultColA = Color.Lerp(colA1, colA2, pulseStat);
        resultColB = Color.Lerp(colB1, colB2, pulseStat);
        if (thisRenderer)
        {
          
            outlineMat.SetColor("_Color1", resultColA);
            outlineMat.SetColor("_Color2", resultColB);
        }
    }
}
