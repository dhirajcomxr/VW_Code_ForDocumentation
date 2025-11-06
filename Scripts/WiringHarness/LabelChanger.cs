using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelChanger : MonoBehaviour
{
    public GameObject TMPrefab;
    public GameObject[] allLabels;
    GameObject temp;
    // Start is called before the first frame update
    void Start()
    {
        //allLabels = GameObject.FindGameObjectsWithTag("Label");
        //StartChanging();
    }
    [ContextMenu("ReplaceLabels")]
    void StartChanging()
    {
        //Debug.Log("Started");
        allLabels = GameObject.FindGameObjectsWithTag("Label");

        for (int i = 0; i < allLabels.Length; i++)
        {
            if(allLabels[i].transform.childCount > 0)
            {
                if(allLabels[i].transform.GetChild(0).childCount > 0)
                {
                    for (int j = 0; j < allLabels[i].transform.GetChild(0).childCount; j++)
                    {
                        temp = Instantiate(TMPrefab, allLabels[i].transform.GetChild(0).GetChild(j).position, allLabels[i].transform.GetChild(0).GetChild(j).rotation, allLabels[i].transform);
                        //Debug.Log("Instantiated");
                        if(temp.GetComponent<TextMeshPro>() && allLabels[i].transform.GetChild(0).GetChild(j).GetComponent<TextMeshProUGUI>())
                        {
                            if(allLabels[i].transform.GetChild(0).GetChild(j).GetComponent<GizmoMapper>() != null)
                            {
                                temp.AddComponent<GizmoMapper>();
                                temp.GetComponent<GizmoMapper>().connectedGizmo = allLabels[i].transform.GetChild(0).GetChild(j).GetComponent<GizmoMapper>().connectedGizmo;
                            }
                            


                            temp.GetComponent<TextMeshPro>().text = allLabels[i].transform.GetChild(0).GetChild(j).GetComponent<TextMeshProUGUI>().text;
                            temp.transform.localScale = new Vector3(0.0008091448f, 0.0008091448f, 0.0008091448f);
                        }
                        allLabels[i].transform.GetChild(0).gameObject.SetActive(false);
                        //Debug.Log("Done " + i + " " + j);
                    }
                }
                

            }
        }
    }
}
