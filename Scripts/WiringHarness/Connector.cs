using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Connector : MonoBehaviour 
{

    public GameObject component;
    public Transform localPivot;
    public string componentDesignation;
    public string connectorDesignation;
    public string connectorName;
    public Wire[] wires;
    public string componentName;
    private void Start()
    {
        if (component == null)
        {
            component = gameObject;
        }
    }
}
