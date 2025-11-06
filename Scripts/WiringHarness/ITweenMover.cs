using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITweenMover : MonoBehaviour
{
    Vector3 temp;
     Vector3  newPos;
    //public int xt, yt, zt;
    float x1;
    float x2;
    // Start is called before the first frame update
    void Start()
    {
        temp = this.transform.position;
//<<<<<<< Updated upstream
  //      newPos = new Vector3(temp.x, temp.y - (Screen.height*0.205f), temp.z);
//=======
        //newPos = new Vector3(temp.x, temp.y - 380, temp.z);
        newPos = new Vector3(temp.x, temp.y + (Screen.height * 0.168f), temp.z);
//>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0)) { x1 = Input.mousePosition.y; }
       // if (Input.GetMouseButtonUp(0)) { x2 = Input.mousePosition.y; UpdateTable(); }
    }

   public void SwipeDown()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", temp, "time", 0.5f, "easetype", iTween.EaseType.easeInOutSine));
    }
   public void SwipeUp()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", 0.5f, "easetype", iTween.EaseType.easeInOutSine));
    }
     void UpdateTable()
    {
        if (x1 > x2)
        {
            if(x1-x2 > 20f)
            {
                //Debug.Log(x1 - x2);
                SwipeDown();
            }
            
        }
        if (x1 < x2)
        {
            if(x2-x1 > 20f)
            {
                //Debug.Log(x2 - x1);
                SwipeUp();
            }
            
        }
    }

}

