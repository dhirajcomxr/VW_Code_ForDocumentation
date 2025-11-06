using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class DropdownSelectorFix : MonoBehaviour
{
    Dropdown dropdown;
   // public UnityEvent onDropdownOpen, onDropdownClosed;
    private void Reset()
    {
        dropdown = GetComponent<Dropdown>();
        if (!dropdown)
        {
            DestroyImmediate(this);
            Debug.LogError("No Dropdown present");
        }
        DropdownSelectorFix fix = GetComponent<DropdownSelectorFix>();
        if (fix)
        {
            DestroyImmediate(this);
            Debug.LogError("Already Exists!");
        }
    }
    private void OnEnable()
    {
      
    }
}
