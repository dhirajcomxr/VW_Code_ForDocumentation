using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DisablePopupButton : MonoBehaviour
{
    public Button[] issueResolved;
   // public Image[] issueResolvedObj;
    public Toggle manualToggle;
    public bool isToggleOn;
    private void Update()
    {
        disableRnRPopupTicket();
    }
    public void disableRnRPopupTicket()
    {
        isToggleOn = manualToggle.GetComponent<Toggle>().isOn;
        if (isToggleOn)
        {
            InteractableOnOff(false);
        }
        else
        {
            InteractableOnOff(true);
        }
    }

    void InteractableOnOff(bool value)
    {
        for (int i = 0; i < issueResolved.Length; i++)
        {
            issueResolved[i].GetComponent<Button>().interactable = value;
            //issueResolvedObj[i].GetComponent<Image>().color = Color.grey;
        }
    }

    public void EnableDisableScreen()
    {
        VWSectionModule vw = FindObjectOfType<ModuleLoadManager>().transform.GetComponentInChildren<VWSectionModule>();
        if (vw.explodedView == null)
        {
            Camera.main.GetComponent<cakeslice.OutlineEffect>().enabled = false;
            //  gameObject.get
            // GameObject outline = GameObject.Find("Main Camera");
        }
        
    }
}
