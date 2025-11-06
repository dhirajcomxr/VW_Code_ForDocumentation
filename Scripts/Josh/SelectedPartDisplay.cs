using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectedPartDisplay : MonoBehaviour
{
    //   [HideInInspector]
    [SerializeField] bool disableIfNoText;
    [SerializeField] string prefix, suffix;
   [SerializeField] VWReferencesManager vwRef;
    [SerializeField] Text display;
    // Start is called before the first frame update
    private void Reset()
    {
        vwRef = FindObjectOfType<VWReferencesManager>();
        display = GetComponent<Text>();
        if (display == null)
            display = GetComponentInChildren<Text>();
    }
    
    private void OnEnable()
    {
        //Debug.Log("Enabled ", gameObject);
        Refresh();
    }
    public void Refresh()
    {
        string result = "";
        if (display != null & vwRef != null)
            result = vwRef.GetSelectedName();
        display.text = prefix + result + suffix;
        if(disableIfNoText)
        gameObject.SetActive(result.Length > 0);
    }
}
