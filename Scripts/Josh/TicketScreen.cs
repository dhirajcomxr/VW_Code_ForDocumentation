using UnityEngine.UI;
using UnityEngine;
public partial class ScreenLinker
{
    [System.Serializable]
    public class TicketScreen
    {
        public TicketType ticketType;
        public Text pausedTicketsCount, openTicketsCount, vehicleDetails;
        public InputField issueIp, vinIp, escalateRem;
        public ImageLabelList pausedTickets, openTickets,escalatedTickets, closeTickets, reassignedTicket, resolvedTicket;
        public Button openTicketBtn, escalatedTicketBtn, closedTicketBtn, reassignedTicketBtn, resolvedTicketBtn;
        public enum TicketType
        {
            Open,Escalated,Close,Reassigned, Resolved
        }
        public void SelectTicketType(TicketType type)
        {
            Debug.LogError(" Select Ticket type ------->>>>>>   "+type);
            switch (type)
            {
                case TicketType.Open:
                    Focus(openTicketBtn, new Button[] { escalatedTicketBtn, closedTicketBtn, reassignedTicketBtn, resolvedTicketBtn });
                    break;
                case TicketType.Escalated:
                    Focus(escalatedTicketBtn, new Button[] { openTicketBtn, closedTicketBtn, reassignedTicketBtn, resolvedTicketBtn });
                    break;
                case TicketType.Close:
                    Focus(closedTicketBtn, new Button[] { openTicketBtn, escalatedTicketBtn, reassignedTicketBtn, resolvedTicketBtn });
                    break;
                case TicketType.Reassigned:
                    Focus(reassignedTicketBtn, new Button[] { openTicketBtn, closedTicketBtn, resolvedTicketBtn, escalatedTicketBtn });
                    break;
                case TicketType.Resolved:
                    Focus(resolvedTicketBtn, new Button[] { openTicketBtn, escalatedTicketBtn, reassignedTicketBtn, escalatedTicketBtn });
                    break;
                default:
                    break;
            }
            ticketType = type;
        }
        public void Focus(Button focusOn,Button[] outOfFocus)
        {
            SetColForButton(focusOn, 1f);
            foreach (var item in outOfFocus)
            {
                SetColForButton(item, 0.5f);
            }
         

          void  SetColForButton(Button btn, float alpha){
                Color btnCol = btn.image.color;
                btnCol.a = alpha;
                btn.image.color = btnCol;
            }
        }
    }
}
