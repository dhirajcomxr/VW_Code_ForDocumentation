using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[DisallowMultipleComponent]
public class UIScreenSimple : UIScreen
{
    // Start is called before the first frame update
  
    public UnityEvent onScreenEnable;
    private void OnEnable()
    {
        onScreenEnable.Invoke();
    }
}
