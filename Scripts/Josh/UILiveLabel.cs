using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UILiveLabel : MonoBehaviour
{
   
    public Text label;
    public bool deactivateParent=false;
    // Start is called before the first frame update
    [SerializeField] float maxLifeTime = 2f;
    float disableTime;
    private void Reset()
    {
        if (!label)
            label = GetComponent<Text>();
        if (!label)
            label = GetComponentInChildren<Text>();
    }
    void Start()
    {
        disableTime = Time.time + maxLifeTime;
    }
   public void UpdateLabel(string newLabel)
    {
        if (label != null)
            label.text = newLabel;
        GameObject par = gameObject.transform.parent.gameObject;
        par.SetActive(deactivateParent ?true:par.activeSelf);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        disableTime = Time.time+maxLifeTime;
    }
    // Update is called once per frame
    void Update()
    {

        if (Time.time > disableTime)
        {
            if (deactivateParent)
                gameObject.transform.parent.gameObject.SetActive(false);
            else
            gameObject.SetActive(false);
        }
    }
}
