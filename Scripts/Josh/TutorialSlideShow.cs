using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using mixpanel;

public class TutorialSlideShow : MonoBehaviour
{
    
    [SerializeField] UnityEvent onFinish;
    [SerializeField] Image display;
    [SerializeField] Sprite[] tutorialScreens;
    [SerializeField] public Sprite curSprite;
    [SerializeField] GameObject prevButton, nextButton;
    int curScreenId=0;
    // Start is called before the first frame update
    bool started = false;
    
    private void OnEnable()
    {
        curScreenId = 0;
        started = true;
    }
    public void OnNext()
    {
        curScreenId++;
        if (tutorialScreens.Length > 0)
        {
            if (curScreenId >= (tutorialScreens.Length - 1))
            {
                curScreenId = tutorialScreens.Length - 1;

                onFinish?.Invoke();
                started = false;
            }
            
        }
        DisplayScreen();
    }
    void DisplayScreen()
    {
        curSprite = tutorialScreens[curScreenId];
        display.sprite = tutorialScreens[curScreenId];
        string temp = tutorialScreens[tutorialScreens.Length - 1].name;
        if (curSprite.name  == temp )
        {
            Mixpanel.Track("Tutorial_Completed");
        }
    }

    public void OnPrevious()
    {
        curScreenId--;
        if (curScreenId <= 0)
        {
            curScreenId = 0;
            prevButton.SetActive(false);
        }
        else
            prevButton.SetActive(true);
        DisplayScreen();
    }
}
