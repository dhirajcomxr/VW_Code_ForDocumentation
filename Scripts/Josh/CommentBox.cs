using mixpanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommentBox : MonoBehaviour
{
    [SerializeField] UIScreenManager screen;
    public InputField Comment;
    public GameObject YesButton;
    UIButton UIButtonscript;
    public AppApiManager apiManager;
    
    public void CheckComment()
    {
        Debug.Log(apiManager.serverData.vehicle_details.VIN);
        string VIn = apiManager.serverData.vehicle_details.VIN;
        string FirstName = apiManager.serverData.profile_info.FirstName;
        string LastName = apiManager.serverData.profile_info.LastName;
        string Email = apiManager.serverData.profile_info.EmailID;
        string Coment = Comment.text;


        Debug.Log("Check comment" + apiManager.Save_Log);
        string Log_JSON = JsonUtility.ToJson(apiManager.Save_Log);
        Debug.Log(Log_JSON);

     
            apiManager.SaveComment(Log_JSON);
        Mixpanel.Track("Issue_Resolved");
        Mixpanel.Track("Task_Completed");
    }

    public void TaskIncomplete()
    {
        Mixpanel.Track("Task_Incompleted");
    }

    public GameObject ParentStarRate;

    public void Rating(int rate)
    {
        Debug.Log("rATING"+rate);
        for(int i=0; i <= 4; i++)
        {
            if (i<=rate)
            {
                ParentStarRate.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ParentStarRate.transform.GetChild(i).gameObject.SetActive(false);
            }
           
        }
    }
    // Start is called before the first frame update
    void Start()
    {
     /*   UIButtonscript =YesButton.GetComponent<UIButton>();
        UIButtonscript.enabled = false;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
