using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiInputFieldLabel : MonoBehaviour
{
    public int id = -1;
    public Text label;
    public InputField inputField;
    private void Reset()
    {
        inputField = GetComponent<InputField>();
        if (!inputField)
            inputField = GetComponentInChildren<InputField>();
        if(inputField)
        label = inputField.placeholder.GetComponent<Text>();
    }
}
