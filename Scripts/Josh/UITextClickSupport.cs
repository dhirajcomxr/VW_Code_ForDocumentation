using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UITextClickSupport : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Text textElement;
   
    // add callbacks in the inspector like for buttons
    [Header("Click Functions")]
    [Space(10)]
    [SerializeField] bool copyToClipboardOnClick;
    public string altStringToSendOnClick;
    public string onClickPrefix, onClickSuffix;
    public UnityEvent<string> onClick;
    
    
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
        Debug.Log(name + " Game Object Clicked!", this);

        // invoke your event
        if (textElement != null)
            if (copyToClipboardOnClick)
                CopyToClipboard();
    }
    public void CopyToClipboard()
    {
        CopyToClipboard(textElement);
        if (altStringToSendOnClick.Length > 0)
            onClick.Invoke(onClickPrefix + altStringToSendOnClick + onClickSuffix);
        else
            onClick.Invoke(onClickPrefix + textElement.text + onClickSuffix);
    }
   public void CopyToClipboard(Text textEl)
    {
        if (textEl.text.Length > 0)
            GUIUtility.systemCopyBuffer = textEl.text;
    }
    // Start is called before the first frame update
    private void Reset()
    {
        textElement = GetComponent<Text>();
        if (textElement != null)
            if (!textElement.raycastTarget)
                Debug.LogError("Click Support won't work with Raycast Target disabled", textElement);
    }
 
}
