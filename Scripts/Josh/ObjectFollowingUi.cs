using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollowingUi : MonoBehaviour
{
    public Transform target;
    public bool hideOnOverlap = false;
   [SerializeField]  RectTransform[] otherRects;
    RectTransform rect;
    CanvasGroup thisCanvas;
    int myIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        if (hideOnOverlap)
        {
            myIndex = transform.GetSiblingIndex();
          int total=  transform.parent.childCount;
            otherRects = new RectTransform[total];
            for (int i = 0; i < total; i++)
            {            
                otherRects[i] = transform.parent.GetChild(i).GetComponent<RectTransform>();
            }
            rect = GetComponent<RectTransform>();
            thisCanvas = GetComponent<CanvasGroup>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var wantedPos = Camera.main.WorldToScreenPoint(target.position);
        transform.position = wantedPos;
        if (hideOnOverlap)
            CheckOverlap();
    }
    void CheckOverlap()
    {
        bool overlap = false;
        for (int i = myIndex; i < otherRects.Length; i++)
        {
            if(i!=myIndex)
            if (otherRects[i].rect.Overlaps(rect.rect))
                overlap = true;

        }

        if (thisCanvas)
        {
            if (overlap)
                thisCanvas.alpha = 0.1f;
            else
                if (thisCanvas.alpha < 0.5f)
                thisCanvas.alpha = 1;
        }
    }
}
