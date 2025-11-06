using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiToast : MonoBehaviour
{
    [SerializeField] Text toastTxt;
    [SerializeField] Image image;
    [SerializeField] float time=2.0f;
    // Start is called before the first frame update
    public void Toast(string msg)
    {
        gameObject.SetActive(true);
        toastTxt.text = msg;
        Invoke("ToastOff", time);
    }

  public void ToastOff()
    {
        gameObject.SetActive(false);
        toastTxt.text = "error";
    }

    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
