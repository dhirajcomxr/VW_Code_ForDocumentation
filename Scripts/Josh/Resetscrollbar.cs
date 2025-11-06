using UnityEngine.UI;
using UnityEngine;

public class Resetscrollbar : MonoBehaviour
{
    public ScrollRect ticketScroll;

    private void Start()
    {
        Debug.Log("ONENABLE WORKING");
        //ticketScroll.verticalScrollbar.value = 1f;
        //ticketScroll.verticalScrollbar.size = 0.632f;

        ticketScroll.verticalNormalizedPosition = 1f;
        ticketScroll.verticalScrollbar.size = 0.6f;
    }



}
