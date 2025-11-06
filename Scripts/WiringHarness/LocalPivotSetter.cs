using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPivotSetter : MonoBehaviour
{
    public Connector[] connectors;

    public void Start()
    {

    }

    [ContextMenu("SetLocalPivot")]
    public void SetLocalPivot()
    {
        connectors = FindObjectsOfType<Connector>();
        foreach (Connector c in connectors)
        {
            GameObject pivot = new GameObject(c.name + " pivot");
            pivot.transform.parent = c.transform;
            pivot.transform.localPosition = Vector3.zero;
            c.localPivot = pivot.transform;
        }
    }
}
