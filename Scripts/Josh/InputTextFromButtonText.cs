using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InputTextFromButtonText : MonoBehaviour
{
    [SerializeField] InputField inputField;
    private void Reset()
    {
        inputField = GetComponent<InputField>();
    }
    public void SetTextTo(Text buttonText)
    {
        inputField.text = buttonText.text;
        inputField.onValueChanged.Invoke(buttonText.text);
        inputField.onEndEdit.Invoke(buttonText.text);
    }
}
