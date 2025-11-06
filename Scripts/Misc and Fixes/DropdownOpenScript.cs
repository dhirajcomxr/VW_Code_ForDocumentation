using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class DropdownOpenScript : MonoBehaviour
{
    public GameObject self;
    public GameObject raycastBlocker;      //This is the object containing my controller script

    public UnityEvent onListOpen, onListClose;
    // Start is called before the first frame update
    void Start()
    {
        if (self.name == "Dropdown List")
        {
            //ControllScript is my main script controlling my program opperation.
            //It contains a public boolean bDropdownOpen to indicate whether the
            //dropdown list is open or not
            //controller.GetComponent<ControllScript>().bDropdownOpen = true;
            raycastBlocker.SetActive(true);
            Debug.Log("Open");
            onListOpen?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if(self!=null)
        if (self.name == "Dropdown List")
        {
            //controller.GetComponent<ControllScript>().bDropdownOpen = false;
            Debug.Log("Close");
            raycastBlocker.SetActive(false);
                onListClose?.Invoke();
        }
    }
   public void BlockRaycast()
    {
        raycastBlocker.SetActive(true);
    }
    public void AllowRaycast()
    {
        raycastBlocker.SetActive(false);
    }
    void DisableRaycastBlocker()
    {
        raycastBlocker.SetActive(false);
    }
}
