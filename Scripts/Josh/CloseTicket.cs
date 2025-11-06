using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseTicket : MonoBehaviour
{
    public ScreenLinker screenLinker;

    public void CloseCurrentTicket(InputField input)
    {
        // Call the method to close the current ticket
        if (input.text.Trim() != "")
        {
            screenLinker._closingRemark = input.text.Trim();
            screenLinker.CloseCurrentTicket();
        }
        else
        {
            screenLinker._closingRemark = "Close Remark is empty";
            screenLinker.CloseCurrentTicket();
            Debug.LogWarning("Input field is empty. Please provide a reason for closing the ticket.");
        }
        
        //screenLinker.CloseCurrentTicket();
    }
}
