using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelAnim : MonoBehaviour
{
    [SerializeField] CanvasGroup cGroup;
    
  
    [Header("Settings")]
    bool activate = false;
    [SerializeField] float fadeSpeed = 0.2f, animStat;
    [SerializeField] bool loop = false, fade=false,scale=false;
    private void Reset()
    {
        cGroup = GetComponent<CanvasGroup>();
        cGroup = gameObject.AddComponent<CanvasGroup>();
    }
    private void OnEnable()
    {
        AnimFin();
        AnimBegin();
    }
    void AnimBegin()
    {
        activate = true;
        animStat = 0;
    }
    private void OnDisable()
    {
        AnimFin();
    }
    void AnimFin()
    {
        if(fade)
        cGroup.alpha = 0;
        if(scale)
        transform.localScale = Vector3.one * 0.8f;
        if (loop)
            AnimBegin();
    }
    private void Update()
    {
        if (activate)
        {
            if (animStat <= 1)
            {
                animStat += Time.deltaTime * fadeSpeed;
                if(fade)
                cGroup.alpha = animStat;
                float sc = Mathf.Lerp(0.8f, 1, animStat);
                if(scale)
                transform.localScale = Vector3.one * sc;
            }
            else
            {
                activate = false;
                if (loop)
                    AnimFin();
              
            }
        }
    }
}
