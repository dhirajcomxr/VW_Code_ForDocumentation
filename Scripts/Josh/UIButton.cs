using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class UIButton : MonoBehaviour
{
    public int index = -1;
    public Button button;
    [SerializeField] bool openAfterLoad = false;
    [SerializeField] UIScreenManager manager;
    private void OnEnable()
    {
        if (button)
            if (manager)
            {
                button.onClick.RemoveListener(CallAction);
                button.onClick.AddListener(CallAction);
              
            }
      //          CallAction());
    }
    void CallAction()
    {
        //   Debug.Log("Called for:" + index);
        if (openAfterLoad)
            manager.OpenAfterLoading(index);
        else
        manager.SelectScreenByButton(index);
    }
    private void OnDisable()
    {
        if (button)
            if (manager)
                button.onClick.RemoveListener(() => CallAction());
    }
    private void Reset()
    {
        _initialise();
    }
    private void OnValidate()
    {
        _initialise();
    }

    void _initialise()
    {
        if (!button)
            button = GetComponent<Button>();
        if (!button)
        {
            button = gameObject.AddComponent<Button>();
            Debug.Log("Added Button to:" + gameObject.name, gameObject);
        }
        if (index <= 0)
        {
            Debug.Log("Check Index :"+index+" for "+gameObject.name, gameObject);
            if (int.TryParse(gameObject.name, out index))
                Debug.Log("INDEX:" + index, gameObject);
        }
        
        if (!manager)
        {
            manager = FindObjectOfType<UIScreenManager>();
            if (manager != null)
                Debug.Log("Found ScreenManager", gameObject);
        }
    }

}
