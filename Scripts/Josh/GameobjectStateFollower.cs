using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameobjectStateFollower : MonoBehaviour
{
    public GameObject[] followers;
    public UnityEvent onEnable, onDisable;
    [SerializeField] bool debug = false;
    private void OnEnable()
    {
        if(debug)
        Debug.Log("Enabled: " + gameObject.name);
        SetState(true);
        onEnable?.Invoke();
    }
    private void OnDisable()
    {
        if (debug)
            Debug.Log("Disabled " + gameObject.name);
       
        SetState(false);
        onDisable?.Invoke();
    }
    void SetState(bool state)
    {
        for (int i = 0; i < followers.Length; i++)
        {
            followers[i].SetActive(state);
        }
    }
}
