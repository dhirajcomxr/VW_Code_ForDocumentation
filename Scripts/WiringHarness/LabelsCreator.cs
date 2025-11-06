using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelsCreator : MonoBehaviour
{
    //public GameObject lblPrefab;
    int lblCount;
    public GameObject[] lblsFront, lblsBack, planes;

    private void Start()
    {
        Debug.Log(this.gameObject.name);
        lblCount = this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform.childCount;
        lblsFront = new GameObject[lblCount];
        lblsBack = new GameObject[lblCount];
        planes = new GameObject[lblCount];
        StartCreating();
    }

    private void StartCreating()
    {
        for(int i = 0; i<lblCount-1; i++) // lblcount-1 because there will be an extra plane for connector designation
        {
            planes[i] = this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject;
            //lblsFront[i] = Instantiate(lblPrefab, this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(1).transform);
            //lblsBack[i] = Instantiate(lblPrefab, this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(2).transform);

            lblsFront[i] = this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(1).GetChild(i).gameObject;
            lblsBack[i] = this.transform.GetChild(2).transform.GetChild(0).transform.GetChild(2).GetChild(i).gameObject;


        }
    }
}
