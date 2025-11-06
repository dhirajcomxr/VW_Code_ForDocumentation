using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
public class PartHighlighter : MonoBehaviour
{
    public Outline sample;
   
    public FullScreenObjectCamera camera;
    public List<GameObject> list;


    [Header("TEST")]
    [SerializeField] bool test = false;
    public string[] highlightNames;
    public List<Renderer> curList;
    [SerializeField] bool next = false;
    [SerializeField] int cur = 0;
  public  int totalHighlights = 0;
    public void Highlight(string[] parts)
    {     
        if(curList!=null)
        if(curList.Count>0)
        RemoveHighLight();
        curList = new List<Renderer>();
        for (int i = 0; i < parts.Length; i++)
        {
            GameObject g = GameObject.Find(parts[i]);
            Highlight(g);
        }
        if (camera)
            camera.Focus(curList.ToArray());     
    }
   public void RemoveAllHighlights()
    {
        Outline[] allH = FindObjectsOfType<Outline>();
        foreach (var item in allH)
        {
            Destroy(item);
        }
        //Debug.Log("Removed ALL highlights");
        totalHighlights =0;
    }
    void AddToListAndHighlight(Renderer r)
    {
        curList.Add(r);
        AddOutLineTo(r.gameObject);

    }
    void AddOutLineTo(GameObject g)
    {
        Outline h = g.GetComponent<Outline>();
        if(h==null)
         h = g.AddComponent<Outline>();
        totalHighlights++;
        if (sample)
        h.color = sample.color;
    }
   public void Highlight(GameObject[] gos)
    {
        if(gos!=null)
        foreach (var item in gos) Highlight(item);

    }
   public void Highlight(GameObject g)
    {
        if (g != null)
        {
            if (g.transform.childCount >= 1)
            {
                int total = g.transform.childCount;
                List<Renderer> children = new List<Renderer>(g.GetComponentsInChildren<Renderer>());
                foreach (var item in children)
                {
                    AddToListAndHighlight(item.gameObject);
                }
            }
            else
            {
                AddToListAndHighlight(g);
            }
        }
    }
   void AddToListAndHighlight(GameObject g)
    {
        Renderer hRenderer = g.GetComponent<Renderer>();
     //   if(g.GetComponent<Renderer>())
        if(hRenderer!=null)
        {
            bool hasNull = false;
            for (int i = 0; i < hRenderer.materials.Length; i++)
            {
                if (hRenderer.materials[i] == null)
                {
                    hasNull = true;
                    Debug.LogError("Material Missing here:" + g.name, g);
                }
            }
            if (!hasNull)
            {
                curList.Add(g.GetComponent<Renderer>());
                AddOutLineTo(g);
            }
            else
                Debug.LogError("Material Missing here:" + g.name, g);
        }
    }
    public void RemoveHighlightFor(GameObject[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if(arr[i].GetComponent<Outline>())
            Destroy(arr[i].GetComponent<Outline>());

        }
        totalHighlights -= arr.Length;
    }
   public void RemoveHighLight()
    {
     
        for (int i = 0; i < curList.Count; i++)
        {
            Destroy(curList[i].GetComponent<Outline>());
           
        }
        totalHighlights -= curList.Count;
        curList = new List<Renderer>();
        if (totalHighlights > 0)
            RemoveAllHighlights();
    }
    // Start is called before the first frame update
    void Start()
    {
      if(test)
        if (highlightNames.Length>=1)
            Highlight(highlightNames);
    }

    // Update is called once per frame
    void Update()
    {
        if (next)
        {
            cur++;
            if (cur >= highlightNames.Length)
                cur = 0;
            Highlight(new string[]  { highlightNames[cur]});
            next = false;
        }
    }
}
